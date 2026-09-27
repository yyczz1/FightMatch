using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FightMatch.Tests;
using static FightMatch.Tests.SaveRecoveryProcessCases;

internal static class Program
{
    private const string Unity = @"D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe";
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] != "controller") { SaveRecoveryProcessCases.Run(args); return 0; }
            var root = Path.GetFullPath(args[1]); CheckRoot(root); Need(!Directory.Exists(root), "new controller root"); Directory.CreateDirectory(root);
            var nonce = Path.GetFileName(root); var identity = Identity(root, nonce, root, "controller", "matrix");
            identity.Add("args", string.Join("\n", args)); identity.Add("cwd", Environment.CurrentDirectory);
            foreach (var name in new[] { "FightMatch.Platform.dll", "FightMatch.Core.dll", "FlowPuzzle.Core.dll" })
            {
                var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);
                identity.Add(name, new FileInfo(file).Length + ":" + FileHash(file));
            }
            WriteRecord(root, Path.Combine(root, "controller-start.tsv"), identity);
            Console.WriteLine("B13 controller PID=" + Process.GetCurrentProcess().Id + " root=" + root);
            foreach (var n in Enumerable.Range(1, 10)) RunCase(root, nonce, "dotnet", "P" + n.ToString("00"));
            foreach (var n in new[] { "P03", "P07", "P08", "P09" }) RunCase(root, nonce, "unity", n);
            WriteRecord(root, Path.Combine(root, "controller-result.tsv"), new Dictionary<string, string> {
                { "result", "Passed" }, { "cases", "dotnet:P01,P02,P03,P04,P05,P06,P07,P08,P09,P10;unity:P03,P07,P08,P09" }, { "utc", DateTime.UtcNow.ToString("o") } });
            Console.WriteLine("B13 MATRIX PASSED: 10 .NET cases, 4 real Unity cases"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void RunCase(string root, string nonce, string runtime, string number)
    {
        var caseRoot = Safe(root, Path.Combine(root, runtime + "-" + number)); Need(!Directory.Exists(caseRoot), "new case"); Directory.CreateDirectory(caseRoot);
        using (var writer = Start(root, nonce, caseRoot, runtime, number, "writer"))
        {
            if (number == "P10")
            {
                WaitExit(writer.Process, 180, "normal writer"); Need(writer.Process.ExitCode == 0, "normal writer exit");
                SaveExit(root, caseRoot, "writer", writer.Process, false);
            }
            else
            {
                var barrierPath = Safe(root, Path.Combine(caseRoot, "barrier.tsv")); var deadline = DateTime.UtcNow.AddSeconds(180); var progress = DateTime.UtcNow;
                while (!File.Exists(barrierPath))
                {
                    Need(!writer.Process.HasExited, "writer exited before barrier: " + runtime + " " + number);
                    Need(DateTime.UtcNow < deadline, "barrier timeout; process retained, no unverified kill");
                    if ((DateTime.UtcNow - progress).TotalSeconds >= 15) { Console.WriteLine("Waiting " + runtime + " " + number + " barrier, writer PID=" + writer.Process.Id); progress = DateTime.UtcNow; }
                    Thread.Sleep(100);
                }
                var evidence = ReadRecord(root, barrierPath); VerifyOwnWriter(root, nonce, caseRoot, number, writer, evidence);
                Need(evidence["phase"] == Phase(number), "exact I/O barrier");
                if (number == "P08")
                {
                    var receipt = ReadRecord(root, Path.Combine(caseRoot, "receipt.tsv")); var expected = ReadRecord(root, Path.Combine(caseRoot, "expected.tsv"));
                    Need(receipt["code"] == "Committed" && receipt["commit"] == expected["candidate"], "controller actually received commit receipt");
                    receipt.Add("receivedUtc", DateTime.UtcNow.ToString("o")); WriteRecord(root, Path.Combine(caseRoot, "controller-received-receipt.tsv"), receipt);
                }
                // Recheck the saved process identity immediately before this one permitted destructive operation.
                VerifyOwnWriter(root, nonce, caseRoot, number, writer, evidence);
                var kill = new Dictionary<string, string>(evidence) { { "reason", "authorized own writer reached " + Phase(number) },
                    { "killRequestedUtc", DateTime.UtcNow.ToString("o") }, { "killEntireProcessTree", "False" } };
                WriteRecord(root, Path.Combine(caseRoot, "kill-request.tsv"), kill);
                writer.Process.Kill(false); WaitExit(writer.Process, 60, "killed writer");
                Need(writer.Process.ExitCode != 0, "expected nonzero externally killed writer"); SaveExit(root, caseRoot, "writer", writer.Process, true);
            }
        }
        using (var reader = Start(root, nonce, caseRoot, runtime, number, "reader"))
        {
            WaitExit(reader.Process, 180, "fresh reader"); SaveExit(root, caseRoot, "reader", reader.Process, false);
            Need(reader.Process.ExitCode == 0, "reader nonzero: " + runtime + " " + number);
            var writerIdentity = ReadRecord(root, Path.Combine(caseRoot, "writer-identity.tsv"));
            var readerIdentity = ReadRecord(root, Path.Combine(caseRoot, "reader-identity.tsv"));
            Need(writerIdentity["pid"] != readerIdentity["pid"] || writerIdentity["startTicks"] != readerIdentity["startTicks"], "independent process identity");
            var result = ReadRecord(root, Path.Combine(caseRoot, "reader-result.tsv")); Need(result["result"] == "Passed", "reader assertions");
            Console.WriteLine("PASS " + runtime + " " + number + " head=" + result["head"] + " generation=" + result["generation"] + " lookup=" + result["lookup"]);
        }
    }
    private static string Phase(string number)
    {
        switch (number)
        {
            case "P01": return "P01.PartialWrite";
            case "P02": return "P02.FlushCompleted";
            case "P03": return "Snapshot.Promote.After";
            case "P04": return "P04.PartialWrite";
            case "P05": return "P05.FlushCompleted";
            case "P06": return "Marker.Promote.Before";
            case "P07": return "Marker.Promote.After.WriteNotReturned";
            case "P08": return "Write.Committed.ReceiptPublished";
            case "P09": return "Cleanup.OldMarkerDeleted.BeforeSnapshot";
            default: throw new InvalidOperationException("No crash phase");
        }
    }
    private sealed class Child : IDisposable
    {
        internal Process Process;
        internal Dictionary<string, string> Started;
        internal StreamWriter Output, Error;
        public void Dispose()
        {
            if (Process.HasExited) Process.WaitForExit();
            else { Process.CancelOutputRead(); Process.CancelErrorRead(); }
            Output.Dispose(); Error.Dispose(); Process.Dispose();
        }
    }
    private static Child Start(string root, string nonce, string caseRoot, string runtime, string number, string mode)
    {
        var unity = runtime == "unity";
        if (unity)
        {
            Need(FileVersionInfo.GetVersionInfo(Unity).FileVersion.StartsWith("2022.3.18"), "fixed Unity version");
            foreach (var p in Process.GetProcessesByName("Unity")) using (p) Need(p.HasExited, "an existing Unity prevents this experiment");
        }
        var exe = unity ? Unity : @"C:\Program Files\dotnet\dotnet.exe";
        var info = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = Project, RedirectStandardOutput = true, RedirectStandardError = true };
        if (unity)
        {
            foreach (var arg in new[] { "-batchmode", "-nographics", "-quit", "-projectPath", Project,
                "-executeMethod", "FightMatch.Tests.SaveRecoveryProcessEntry.Run", "-logFile", Safe(root, Path.Combine(caseRoot, mode + "-unity.log")) }) info.ArgumentList.Add(arg);
        }
        else info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        foreach (var arg in new[] { "--b13-root", root, "--b13-case-root", caseRoot, "--b13-nonce", nonce, "--b13-case", number, "--b13-mode", mode }) info.ArgumentList.Add(arg);
        var child = new Child { Process = new Process { StartInfo = info },
            Output = new StreamWriter(new FileStream(Safe(root, Path.Combine(caseRoot, mode + "-stdout.log")), FileMode.CreateNew)) { AutoFlush = true },
            Error = new StreamWriter(new FileStream(Safe(root, Path.Combine(caseRoot, mode + "-stderr.log")), FileMode.CreateNew)) { AutoFlush = true } };
        child.Process.OutputDataReceived += (_, e) => { if (e.Data != null) child.Output.WriteLine(e.Data); };
        child.Process.ErrorDataReceived += (_, e) => { if (e.Data != null) child.Error.WriteLine(e.Data); };
        Need(child.Process.Start(), "child process started"); child.Process.BeginOutputReadLine(); child.Process.BeginErrorReadLine();
        child.Started = new Dictionary<string, string> { { "pid", child.Process.Id.ToString(CultureInfo.InvariantCulture) },
            { "startTicks", child.Process.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture) }, { "exe", Path.GetFullPath(exe) },
            { "root", root }, { "nonce", nonce }, { "caseRoot", caseRoot }, { "mode", mode }, { "case", number },
            { "args", string.Join("\n", info.ArgumentList) }, { "cwd", info.WorkingDirectory }, { "utc", DateTime.UtcNow.ToString("o") } };
        WriteRecord(root, Path.Combine(caseRoot, mode + "-startup.tsv"), child.Started); return child;
    }
    private static void VerifyOwnWriter(string root, string nonce, string caseRoot, string number, Child own, Dictionary<string, string> barrier)
    {
        Need(!own.Process.HasExited, "writer remains alive");
        var saved = ReadRecord(root, Path.Combine(caseRoot, "writer-startup.tsv"));
        var identity = ReadRecord(root, Path.Combine(caseRoot, "writer-identity.tsv"));
        foreach (var key in new[] { "pid", "startTicks", "exe", "root", "nonce", "caseRoot", "mode", "case" })
            Need(saved[key] == own.Started[key] && identity[key] == saved[key] && barrier[key] == saved[key], "writer identity mismatch: " + key);
        Need(saved["mode"] == "writer" && saved["root"] == root && saved["nonce"] == nonce && saved["caseRoot"] == caseRoot && saved["case"] == number, "case authority");
        own.Process.Refresh(); Need(own.Process.Id.ToString(CultureInfo.InvariantCulture) == saved["pid"] &&
            own.Process.StartTime.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture) == saved["startTicks"] &&
            string.Equals(Path.GetFullPath(own.Process.MainModule.FileName), saved["exe"], StringComparison.OrdinalIgnoreCase), "live PID/start/executable mismatch");
        Need(string.Join("\n", own.Process.StartInfo.ArgumentList) == saved["args"], "original explicit arguments");
    }
    private static void WaitExit(Process p, int seconds, string stage)
    {
        var deadline = DateTime.UtcNow.AddSeconds(seconds); var progress = DateTime.UtcNow;
        while (!p.WaitForExit(250))
        {
            Need(DateTime.UtcNow < deadline, stage + " timeout; no extra kill authorized");
            if ((DateTime.UtcNow - progress).TotalSeconds >= 15) { Console.WriteLine("Waiting " + stage + " PID=" + p.Id); progress = DateTime.UtcNow; }
        }
        p.WaitForExit();
    }
    private static void SaveExit(string root, string caseRoot, string mode, Process p, bool expectedNonzero)
    {
        WriteRecord(root, Path.Combine(caseRoot, mode + "-exit.tsv"), new Dictionary<string, string> { { "pid", p.Id.ToString(CultureInfo.InvariantCulture) },
            { "exitCode", p.ExitCode.ToString(CultureInfo.InvariantCulture) }, { "exitedUtc", p.ExitTime.ToUniversalTime().ToString("o") },
            { "expectedNonzero", expectedNonzero.ToString() }, { "observedUtc", DateTime.UtcNow.ToString("o") } });
    }
}
