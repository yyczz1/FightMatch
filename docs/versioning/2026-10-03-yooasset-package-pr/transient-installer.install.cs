using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace FightMatch.Editor.ProjectBootstrap
{
    [InitializeOnLoad]
    public static class FightMatchYooAssetProbeInstaller
    {
        private const string Evidence = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/RES-01A/P02";
        private const string Project = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/RES-01A/P01/projection";
        private const string Url = "https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804";
        private const string Key = "FightMatch.RES-01A-P02.RequestStarted";
        private static AddAndRemoveRequest request;
        private static DateTime started;
        private static bool locked;
        private static Receipt receipt;

        [Serializable] private sealed class Owner { public string thread; public string host; public string turn; }
        [Serializable] private sealed class Identity { public long bytes; public string sha256; }
        [Serializable] private sealed class Pair { public Identity manifest; public Identity packagesLock; }
        [Serializable] private sealed class Run { public string status; public string runId; public string projectionRoot; public string evidenceRoot; public Owner executionOwner; public Pair before; }
        [Serializable] private sealed class Dependency { public string name; public string version; }
        [Serializable] private sealed class Package
        {
            public string name; public string version; public string source; public string packageId; public string resolvedPath;
            public string gitHash; public string gitRevision; public string registryUrl; public Dependency[] dependencies; public string[] errors;
        }
        [Serializable] private sealed class Receipt
        {
            public string runId; public string runSha256; public Owner owner; public string startedUtc; public string finishedUtc;
            public string status; public string error; public string requestedUrl; public int requestCount; public Package[] beforePackages; public Package[] packages;
        }

        static FightMatchYooAssetProbeInstaller()
        {
            if (SessionState.GetBool(Key, false))
                EditorApplication.delayCall += Recover;
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, name);
            Require(index >= 0 && index + 1 < args.Length, "Missing argument " + name);
            return args[index + 1];
        }

        private static Package[] Describe(PackageInfo[] packages)
        {
            return packages.Select(p => new Package {
                name = p.name, version = p.version, source = p.source.ToString(), packageId = p.packageId, resolvedPath = p.resolvedPath,
                gitHash = p.git == null ? null : p.git.hash, gitRevision = p.git == null ? null : p.git.revision,
                registryUrl = p.registry == null ? null : p.registry.url,
                dependencies = p.dependencies.Select(d => new Dependency { name = d.name, version = d.version }).ToArray(),
                errors = p.errors == null ? Array.Empty<string>() : p.errors.Select(e => e.message).ToArray()
            }).ToArray();
        }

        public static void Install()
        {
            receipt = new Receipt { runId = "RES-01A-P02", requestedUrl = Url, requestCount = 0 };
            try
            {
                Require(!SessionState.GetBool(Key, false), "Request already started");
                Require(Arg("-fmRun") == Evidence + "/run.json" && Arg("-fmEvidence") == Evidence && Arg("-fmTimeout") == "300", "Argument binding mismatch");
                var bytes = File.ReadAllBytes(Evidence + "/run.json");
                receipt.runSha256 = Hash(bytes);
                Require(receipt.runSha256 == Environment.GetEnvironmentVariable("FM_RES_RUN_SHA256"), "Run hash mismatch");
                var run = JsonUtility.FromJson<Run>(Encoding.UTF8.GetString(bytes));
                Require(run.status == "EXECUTION_BOUND" && run.runId == receipt.runId && run.evidenceRoot == Evidence, "Run is not bound");
                Require(run.executionOwner != null && run.executionOwner.thread == "01a0fdbc-bf1e-7780-8f7f-dec13d6d590c" && run.executionOwner.host == "local" && !string.IsNullOrEmpty(run.executionOwner.turn), "Owner unbound");
                receipt.owner = run.executionOwner;
                Require(Path.GetFullPath(UnityEngine.Application.dataPath + "/..") == Project && run.projectionRoot == Project, "Project mismatch");
                Require(UnityEngine.Application.unityVersion == "2022.3.18f1" && RuntimeInformation.ProcessArchitecture == Architecture.X64, "Editor mismatch");
                Require(Hash(File.ReadAllBytes(run.projectionRoot + "/Packages/manifest.json")) == run.before.manifest.sha256, "Manifest changed before request");
                Require(Hash(File.ReadAllBytes(run.projectionRoot + "/Packages/packages-lock.json")) == run.before.packagesLock.sha256, "Lock changed before request");
                Require(!File.Exists(Evidence + "/upm-install-result.json"), "Receipt already exists");
                receipt.beforePackages = Describe(PackageInfo.GetAllRegisteredPackages());
                Require(!receipt.beforePackages.Any(p => p.name == "com.tuyoogame.yooasset"), "YooAsset already registered");
                started = DateTime.UtcNow; receipt.startedUtc = started.ToString("o");
                SessionState.SetBool(Key, true); EditorApplication.LockReloadAssemblies(); locked = true;
                receipt.requestCount = 1;
                request = Client.AddAndRemove(new[] { Url }, Array.Empty<string>());
                EditorApplication.update += Poll;
            }
            catch (Exception ex) { Finish("FAILED", ex.ToString()); }
        }

        private static void Poll()
        {
            try
            {
                if ((DateTime.UtcNow - started).TotalSeconds >= 300) { Finish("TIMEOUT", "UPM request exceeded 300 seconds"); return; }
                if (!request.IsCompleted) return;
                if (request.Status != StatusCode.Success) { Finish("FAILED", request.Error == null ? "UPM failure without error" : request.Error.message); return; }
                receipt.packages = Describe(request.Result.ToArray());
                Finish("SUCCEEDED", null);
            }
            catch (Exception ex) { Finish("FAILED", ex.ToString()); }
        }

        private static void Recover()
        {
            if (!SessionState.GetBool(Key, false)) return;
            receipt = new Receipt { runId = "RES-01A-P02", requestedUrl = Url, requestCount = 1 };
            Finish("FAILED_DOMAIN_RELOAD", "Request state lost across domain reload; no request repeated");
        }

        private static void Finish(string status, string error)
        {
            EditorApplication.update -= Poll; SessionState.SetBool(Key, false);
            receipt.status = status; receipt.error = error; receipt.finishedUtc = DateTime.UtcNow.ToString("o");
            var exitCode = status == "SUCCEEDED" ? 0 : 1;
            try
            {
                using (var stream = new FileStream(Evidence + "/upm-install-result.json", FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) { writer.Write(JsonUtility.ToJson(receipt, true)); writer.Flush(); stream.Flush(true); }
            }
            catch (Exception ex) { Debug.LogException(ex); exitCode = 1; }
            finally { if (locked) { locked = false; EditorApplication.UnlockReloadAssemblies(); } EditorApplication.Exit(exitCode); }
        }
    }
}

