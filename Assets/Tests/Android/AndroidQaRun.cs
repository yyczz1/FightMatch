using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using FightMatch.Host;
using FightMatch.Platform;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(FightMatch.Android.Tests.AndroidQaRun))]

namespace FightMatch.Android.Tests
{
    [Preserve]
    public sealed class AndroidQaRun : ITestRunCallback
    {
        internal static readonly string[] Phases = {
            "ordinary-storage-and-content", "player-holds-lock", "shell-holds-lock", "holder-death-release",
            "snapshot-promoted-before-marker", "marker-published-response-lost", "locator-created-before-initialize", "initialized-before-active"
        };
        internal static Control Current { get; private set; }
        internal static string Root { get; private set; }
        internal static string RunDirectory { get; private set; }
        internal static string StorageRoot { get; private set; }
        internal static int Pid => getpid();
        internal static uint Uid => getuid();
        [DllImport("libc")] private static extern int getpid();
        [DllImport("libc")] private static extern uint getuid();
        [DllImport("libc", SetLastError = true)] private static extern int kill(int pid, int signal);
        private static bool initialized;

        internal static void Initialize()
        {
            if (initialized) return;
            Assert.AreEqual(RuntimePlatform.Android, UnityEngine.Application.platform);
            Assert.AreEqual("com.yyczz1.fightmatch.qa", UnityEngine.Application.identifier);
            var systemPath = UnityEngine.Application.persistentDataPath;
            Root = Path.Combine(AndroidLocalSaveStorage.CanonicalPersistentDataPath(systemPath), "FightMatchQa029");
            new AndroidContentPublicationStorage(Root);
            Directory.CreateDirectory(Root);
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
                Current = new Control { runId = intent.Call<string>("getStringExtra", "fm029Run"),
                    phase = intent.Call<string>("getStringExtra", "fm029Phase"), step = intent.Call<string>("getStringExtra", "fm029Step") };
            Assert.IsTrue(Regex.IsMatch(Current.runId ?? "", "^[0-9a-f]{32}$"), "An explicit bounded QA run ID is required.");
            Assert.Contains(Current.phase, Phases);
            Assert.That(Current.step, Is.EqualTo("prepare").Or.EqualTo("resume"));
            Assert.IsTrue(Current.step == "prepare" || Array.IndexOf(Phases, Current.phase) >= 4, "Only crash recovery phases resume.");
            var runs = Path.Combine(Root, "runs");
            Directory.CreateDirectory(runs);
            RunDirectory = Path.Combine(runs, Current.runId);
            StorageRoot = Path.Combine(Root, "storage", Current.runId);
            new AndroidContentPublicationStorage(RunDirectory);
            new AndroidContentPublicationStorage(StorageRoot);
            if (!Directory.Exists(RunDirectory)) Assert.Less(Directory.GetDirectories(runs).Length, 8, "Eight QA runs per device maximum.");
            Directory.CreateDirectory(RunDirectory);
            Directory.CreateDirectory(StorageRoot);
            var control = Path.Combine(RunDirectory, "control.json");
            if (File.Exists(control))
            {
                var original = Read<Control>("control.json");
                Assert.AreEqual(original.runId, Current.runId);
                Assert.AreEqual(original.phase, Current.phase);
                Assert.AreEqual("resume", Current.step, "Do not overwrite a prior prepare run.");
            }
            else
            {
                Assert.AreEqual("prepare", Current.step);
                Write("control.json", Current);
            }
            Assert.IsFalse(File.Exists(Path.Combine(RunDirectory, "tests.xml")), "Completed QA evidence is immutable.");
            Write("start-" + Pid + ".json", new Witness { phase = Current.phase, runId = Current.runId,
                point = Current.step, pid = Pid, uid = Uid, systemPath = systemPath, root = Root, utc = DateTime.UtcNow.ToString("O") });
            initialized = true;
            Debug.Log("FM029_QA_ROOT " + Root + " RUN " + Current.runId + " PID " + Pid + " UID " + Uid);
        }
        internal static string FilePath(string leaf)
        {
            Assert.IsTrue(Regex.IsMatch(leaf, "^[a-z0-9-]+\\.(json|xml)$"));
            var path = Path.Combine(RunDirectory, leaf);
            new AndroidContentPublicationStorage(path);
            return path;
        }
        internal static void Write(string leaf, object value)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(value, true));
            Assert.LessOrEqual(bytes.Length, 65536);
            WriteBytes(leaf, bytes);
        }
        private static void WriteBytes(string leaf, byte[] bytes)
        {
            using (var stream = new FileStream(FilePath(leaf), FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }
        internal static T Read<T>(string leaf)
        {
            var path = FilePath(leaf);
            Assert.LessOrEqual(new FileInfo(path).Length, 65536);
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        internal static IEnumerator WaitFor(string leaf)
        {
            var until = Time.realtimeSinceStartupAsDouble + 120;
            while (!File.Exists(FilePath(leaf)))
            {
                Assert.Less(Time.realtimeSinceStartupAsDouble, until, "Missing external witness " + leaf);
                yield return null;
            }
        }
        internal static IEnumerator AwaitExternalTermination()
        {
            var until = Time.realtimeSinceStartupAsDouble + 120;
            while (Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.Fail("The approved QA process was not terminated at the witnessed boundary; no crash pass is claimed.");
        }
        internal static bool ProcessAbsent(int pid)
        {
            Assert.Greater(pid, 0);
            if (kill(pid, 0) == 0) return false;
            return Marshal.GetLastWin32Error() == 3;
        }
        internal static Witness Boundary(string point, FightMatchHostSession host, string commit = null)
        {
            var value = new Witness { phase = Current.phase, runId = Current.runId, pid = Pid, uid = Uid,
                point = point, root = StorageRoot, utc = DateTime.UtcNow.ToString("O"),
                playerId = host.OriginalProfile?.PlayerId, operationId = host.OriginalProfile?.OperationId,
                createHash = host.OriginalProfile?.CreateRecord.RecordSha256, commitId = commit };
            Write("boundary.json", value);
            Debug.Log("FM029_BOUNDARY " + point + " PID " + Pid);
            return value;
        }
        public void RunStarted(ITest testsToRun) { Initialize(); }
        public void TestStarted(ITest test) { }
        public void TestFinished(ITestResult result) { }
        public void RunFinished(ITestResult testResults)
        {
            Initialize();
            var xml = Encoding.UTF8.GetBytes(testResults.ToXml(true).OuterXml);
            Assert.LessOrEqual(xml.Length, 4 * 1024 * 1024);
            WriteBytes("tests.xml", xml);
            Write("result.json", new RunResult { phase = Current.phase, runId = Current.runId, pid = Pid, uid = Uid,
                result = testResults.ResultState.ToString(), passed = testResults.PassCount, failed = testResults.FailCount,
                skipped = testResults.SkipCount, assertions = testResults.AssertCount, utc = DateTime.UtcNow.ToString("O") });
        }
        [Serializable] internal sealed class Control { public string runId, phase, step; }
        [Serializable] internal sealed class Witness
        {
            public string phase, runId, point, utc, systemPath, root, playerId, operationId, createHash, commitId, lockPath;
            public int pid, otherPid, exitCode;
            public uint uid;
        }
        [Serializable] private sealed class RunResult
        {
            public string phase, runId, result, utc;
            public int pid, passed, failed, skipped, assertions;
            public uint uid;
        }
    }
}
