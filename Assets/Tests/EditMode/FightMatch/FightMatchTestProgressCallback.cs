using System;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(FightMatch.Core.Tests.FightMatchTestProgressCallback))]

namespace FightMatch.Core.Tests
{
    public sealed class FightMatchTestProgressCallback : ITestRunCallback
    {
        private const string Argument = "-fightMatchTestProgress";
        private const int MaxRows = 4096;
        private const int MaxFileBytes = 4 * 1024 * 1024;
        private const int MaxRowBytes = 16 * 1024;
        private static readonly object Sync = new object();
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private static bool initialized;
        private static bool failed;
        private static bool registered;
        private static string outputPath;
        private static string processStartUtc;
        private static string domain;
        private static int pid;
        private static TestRunnerApi api;

        [InitializeOnLoadMethod]
        private static void RegisterEditorCallbacks()
        {
            lock (Sync)
            {
                if (!TryEnable() || registered)
                    return;

                try
                {
                    api = ScriptableObject.CreateInstance<TestRunnerApi>();
                    api.hideFlags = HideFlags.HideAndDontSave;
                    // UTF's assembly listener misses the initial RunStarted; CLI exit has priority -10.
                    api.RegisterCallbacks(new EditorRunCallbacks(), 100);
                    registered = true;
                }
                catch (Exception exception)
                {
                    ReportFailure(exception);
                }
            }
        }

        public void RunStarted(ITest testsToRun) { }

        public void RunFinished(ITestResult testResults) { }

        public void TestStarted(ITest test)
        {
            if (!test.IsSuite)
                Append("TestStarted", "attribute", test.Id, test.FullName, "", 0);
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
                Append("TestFinished", "attribute", result.Test.Id, result.Test.FullName,
                    result.ResultState.ToString(), result.Duration);
        }

        private sealed class EditorRunCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Append("RunStarted", "editor", testsToRun.Id, testsToRun.FullName, "", 0);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Append("RunFinished", "editor", result.Test.Id, result.Test.FullName,
                    result.ResultState, result.Duration);
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result) { }
        }

        private static bool TryEnable()
        {
            if (initialized)
                return outputPath != null && !failed;

            initialized = true;
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, Argument);
            if (index < 0)
                return false;

            try
            {
                if (index + 1 >= arguments.Length ||
                    Array.IndexOf(arguments, Argument, index + 1) >= 0)
                    throw new InvalidDataException("Missing or duplicate progress argument.");

                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string expectedPath = Path.Combine(projectRoot,
                    "TestArtifacts/FightMatch/UGUI-01/q4-correction-18/test-events.jsonl");
                if (!Path.IsPathRooted(arguments[index + 1]) ||
                    !string.Equals(Path.GetFullPath(arguments[index + 1]), expectedPath,
                        StringComparison.Ordinal))
                    throw new InvalidDataException("Progress path is outside the fixed FIX18 output.");

                ValidatePath(expectedPath);
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    pid = process.Id;
                    processStartUtc = process.StartTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
                }
                domain = Guid.NewGuid().ToString("N");
                outputPath = expectedPath;
                return true;
            }
            catch (Exception exception)
            {
                ReportFailure(exception);
                return false;
            }
        }

        private static void ValidatePath(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0)
                throw new InvalidDataException("Progress file must be an existing non-link file.");

            for (var directory = new DirectoryInfo(Path.GetDirectoryName(path));
                 directory != null; directory = directory.Parent)
            {
                if (!directory.Exists || (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Progress parent must exist without links.");
            }
        }

        private static void Append(string eventName, string source, string testId, string fullName,
            string result, double duration)
        {
            lock (Sync)
            {
                if (!TryEnable())
                    return;

                try
                {
                    ValidatePath(outputPath);
                    // Open never creates/truncates: the runner owns CreateNew and the empty-file preflight.
                    using (var stream = new FileStream(outputPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                    {
                        int lastSequence = ReadLastSequence(stream);
                        if (lastSequence == 0 && eventName != "RunStarted")
                            throw new InvalidDataException("Initial RunStarted is missing.");
                        if (lastSequence >= MaxRows)
                            throw new InvalidDataException("Progress row limit reached.");

                        var entry = new ProgressEvent
                        {
                            seq = lastSequence + 1,
                            utc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                            pid = pid,
                            processStartUtc = processStartUtc,
                            domain = domain,
                            source = source,
                            eventName = eventName,
                            testId = testId,
                            fullName = fullName,
                            result = result,
                            duration = duration
                        };
                        ValidateEntry(entry, lastSequence + 1);
                        byte[] row = Utf8.GetBytes(JsonUtility.ToJson(entry) + "\n");
                        if (row.Length > MaxRowBytes || stream.Length + row.Length > MaxFileBytes)
                            throw new InvalidDataException("Progress byte limit reached.");

                        stream.Seek(0, SeekOrigin.End);
                        stream.Write(row, 0, row.Length);
                        stream.Flush(true);
                    }
                }
                catch (Exception exception)
                {
                    ReportFailure(exception);
                }
            }
        }

        private static int ReadLastSequence(FileStream stream)
        {
            if (stream.Length > MaxFileBytes)
                throw new InvalidDataException("Existing progress exceeds the byte limit.");
            if (stream.Length == 0)
                return 0;

            byte[] bytes = new byte[(int)stream.Length];
            int offset = 0;
            while (offset < bytes.Length)
            {
                int read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read == 0)
                    throw new InvalidDataException("Progress file changed while reading.");
                offset += read;
            }
            if (bytes[bytes.Length - 1] != (byte)'\n')
                throw new InvalidDataException("Progress ends in a partial row.");

            string[] rows = Utf8.GetString(bytes).Split('\n');
            int count = rows.Length - 1;
            if (count > MaxRows)
                throw new InvalidDataException("Existing progress exceeds the row limit.");

            int lastSequence = 0;
            for (int i = 0; i < count; i++)
            {
                string row = rows[i];
                if (row.Length == 0 || Utf8.GetByteCount(row) + 1 > MaxRowBytes)
                    throw new InvalidDataException("Empty or oversized progress row.");

                var entry = JsonUtility.FromJson<ProgressEvent>(row);
                ValidateEntry(entry, i + 1);
                // JsonUtility accepts missing/extra fields; canonical equality rejects those and malformed rows.
                if (!string.Equals(JsonUtility.ToJson(entry), row, StringComparison.Ordinal))
                    throw new InvalidDataException("Progress row is not canonical JSON.");
                if (i == 0 && entry.eventName != "RunStarted")
                    throw new InvalidDataException("Persisted initial RunStarted is missing.");
                if (entry.eventName == "RunFinished")
                    throw new InvalidDataException("Cannot append to a completed run.");
                lastSequence = entry.seq;
            }
            // Recover from disk on every event, including a newly constructed callback after domain reload.
            return lastSequence;
        }

        private static void ValidateEntry(ProgressEvent entry, int sequence)
        {
            DateTime timestamp;
            Guid domainId;
            if (entry == null || entry.seq != sequence || entry.pid != pid ||
                entry.processStartUtc != processStartUtc ||
                !Guid.TryParseExact(entry.domain, "N", out domainId) ||
                !DateTime.TryParseExact(entry.utc, "O", CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out timestamp) || timestamp.Kind != DateTimeKind.Utc ||
                string.IsNullOrEmpty(entry.testId) || string.IsNullOrEmpty(entry.fullName) ||
                double.IsNaN(entry.duration) || double.IsInfinity(entry.duration) || entry.duration < 0)
                throw new InvalidDataException("Invalid sequence, event fields, or stale process identity.");

            bool started = entry.eventName == "RunStarted" || entry.eventName == "TestStarted";
            bool finished = entry.eventName == "RunFinished" || entry.eventName == "TestFinished";
            bool editor = entry.eventName == "RunStarted" || entry.eventName == "RunFinished";
            if ((!started && !finished) || entry.source != (editor ? "editor" : "attribute") ||
                (started && (entry.result != "" || entry.duration != 0)) ||
                (finished && string.IsNullOrEmpty(entry.result)))
                throw new InvalidDataException("Invalid progress event kind, source, or result.");
        }

        private static void ReportFailure(Exception exception)
        {
            failed = true;
            string message = "[FightMatchTestProgress] BLOCKED_PROGRESS_EVIDENCE " +
                exception.GetType().Name + ": " + exception.Message.Replace('\r', ' ').Replace('\n', ' ');
            try
            {
                // Bypass Unity/NUnit logging so a diagnostic failure cannot replace the original test outcome.
                // The runner must reject this stderr marker and any incomplete event trace.
                using (var error = new StreamWriter(Console.OpenStandardError(), new UTF8Encoding(false)))
                {
                    error.WriteLine(message);
                    error.Flush();
                }
            }
            catch (Exception)
            {
                // If stderr is unavailable, stop appending: missing RunFinished still fails the evidence gate.
            }
        }

        [Serializable]
        private sealed class ProgressEvent
        {
            public int seq;
            public string utc;
            public int pid;
            public string processStartUtc;
            public string domain;
            public string source;
            public string eventName;
            public string testId;
            public string fullName;
            public string result;
            public double duration;
        }
    }
}
