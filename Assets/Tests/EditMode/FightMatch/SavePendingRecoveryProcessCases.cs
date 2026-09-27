using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FightMatch.Core;
using FightMatch.Core.Tests;
using FightMatch.Platform;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Tests
{
    public static class SavePendingRecoveryProcessCases
    {
        public static void Run(string[] arguments)
        {
            var root = EvidenceRoot(arguments);
            var scenario = Argument(arguments, "--b14c1-case");
            var mode = Argument(arguments, "--b14c1-mode");
            Require(new[] { "P01", "P02", "P03" }.Contains(scenario), "fixed scenario");
            Require(mode == "writer" || mode == "reader", "fixed mode");
            var directory = Safe(root, Path.Combine(root, scenario));
            Directory.CreateDirectory(directory);
            var noncePath = Safe(root, Path.Combine(directory, "nonce.txt"));
            if (mode == "writer") WriteNew(noncePath, Guid.NewGuid().ToString("N"));
            Require(File.Exists(noncePath), "writer nonce exists");
            var nonce = File.ReadAllText(noncePath);
            using (var process = Process.GetCurrentProcess())
            {
                var identity = Identity(process, root, directory, nonce, scenario, mode, arguments);
                WriteNew(Path.Combine(directory, mode + "-identity.txt"), identity);
                if (mode == "writer") Writer(directory, scenario);
                else Reader(directory, scenario, process);
                WriteNew(Path.Combine(directory, mode + "-completed.txt"),
                    identity + "\ncompletedUtc=" + DateTime.UtcNow.ToString("o") + "\nassertions=passed\n");
            }
        }

        public static string TestRoot()
        {
            return NewCase();
        }

        private static void Writer(string directory, string scenario)
        {
            using (var rig = new PendingRig("player:015b", Path.Combine(directory, "save")))
            {
                PreparedCandidateApplicationIntent intent;
                var ticket = rig.Application(out intent);
                var fault = scenario == "P01" ? "Snapshot.Flush.after" :
                    scenario == "P02" ? "Snapshot.Write.partial" : "Marker.Promote.after";
                rig.FailWrite(ticket, fault);
                var bytes = PendingRig.Bytes(ticket.Envelope);
                File.WriteAllBytes(Path.Combine(directory, "original-envelope.bin"), bytes);
                File.WriteAllBytes(Path.Combine(directory, "original-intent.bin"), intent.CanonicalBytes.ToArray());
                WriteNew(Path.Combine(directory, "original-metadata.txt"), Metadata(ticket));
                WriteNew(Path.Combine(directory, "writer-fault.txt"), "point=" + fault +
                    "\nused=" + rig.Storage.Base.FaultUsed + "\nbuilds=" + rig.Builds);
                WriteDisk(directory, "writer", rig);
                Require(rig.Builds == 1, "one original business build");
                Require(scenario == "P03" == File.Exists(rig.Path(MarkerName(ticket.Metadata.CommitId))), "publication state");
                if (scenario == "P01")
                    Require(File.ReadAllBytes(rig.Path(PendingRig.Work(ticket))).SequenceEqual(bytes), "complete original work");
                if (scenario == "P02")
                    Require(File.ReadAllBytes(rig.Path(PendingRig.Work(ticket))).Length < bytes.Length, "incomplete original work");
            }
        }

        private static void Reader(string directory, string scenario, Process process)
        {
            var writer = Fields(File.ReadAllText(Path.Combine(directory, "writer-identity.txt")));
            Require(int.Parse(writer["pid"], CultureInfo.InvariantCulture) != process.Id, "different real writer and reader PID");
            Require(long.Parse(writer["startTicks"], CultureInfo.InvariantCulture) < process.StartTime.ToUniversalTime().Ticks, "ordered starts");
            var original = Fields(File.ReadAllText(Path.Combine(directory, "original-metadata.txt")));
            var originalBytes = File.ReadAllBytes(Path.Combine(directory, "original-envelope.bin"));
            Require(Hex(Hash(originalBytes)) == original["sha256"], "original evidence hash");
            var commit = original["commit"];
            using (var rig = new PendingRig("player:015b", Path.Combine(directory, "save"), mode: SaveOpenMode.Existing))
            {
                var before = rig.Disk();
                var view = rig.View();
                if (scenario == "P01")
                {
                    var candidate = Ok(rig.Store.ReadUncommittedCandidate(view, commit, B()), "CandidateRead");
                    Require(candidate.Metadata.SaveGeneration.ToString(CultureInfo.InvariantCulture) == original["generation"], "original generation");
                    Require((candidate.Metadata.ParentCommitId ?? "") == original["parent"], "original parent");
                    Require(string.Join("|", candidate.OperationIds) == original["operations"], "original operations");
                    Require(candidate.Descriptor.TotalLength.ToString(CultureInfo.InvariantCulture) == original["length"], "original length");
                    Require(Hex(candidate.Descriptor.Sha256.ToArray()) == original["sha256"], "original descriptor");
                    Require(PendingRig.Bytes(candidate.Envelope).SequenceEqual(originalBytes), "all original envelope bytes");
                    var decoded = BusinessSaveScenario.Accept(CandidateApplicationSaveCodec.Decode(candidate.Envelope, B().Codec));
                    Require(candidate.Envelope.SliceBytes.Count == 6, "all six original slices decoded");
                    var intent = CandidateApplicationIntentCodec.Read(File.ReadAllBytes(Path.Combine(directory, "original-intent.bin")), B().Codec);
                    var lookup = BusinessSaveScenario.Accept(CandidateApplicationProtocol.Lookup(decoded, intent, B().Codec));
                    Require(lookup.IsFound && lookup.OriginalCommitId == commit && lookup.Initialization.CharacterId == "W", "original operation result");
                    rig.SameDisk(before);
                    Bad(rig.Store.Load(B()), "CommitUnknown");
                    var ticket = Ok(rig.Store.ResumeRecoveredCandidate(candidate, rig.Caps(view), B()), "Resumed");
                    Require(Metadata(ticket) == File.ReadAllText(Path.Combine(directory, "original-metadata.txt")), "entire original metadata");
                    rig.SameDisk(before);
                    Ok(rig.Store.Write(ticket, B()), "Committed");
                    Require(PendingRig.Bytes(Ok(rig.Store.Load(B()), "Loaded")).SequenceEqual(originalBytes), "committed original bytes");
                    Ok(rig.Store.Lookup(commit, "init", B()), "Committed");
                    Require(rig.Builds == 0, "reader never rebuilds business");
                }
                else if (scenario == "P02")
                {
                    Require(view.EvidenceComplete && !view.RequirementsComplete, "complete byte evidence of partial candidate");
                    Bad(rig.Store.ReadUncommittedCandidate(view, commit, B()), "RecoveryBlocked");
                    var end = Ok(rig.Store.EndObservedCandidate(view, commit, B()), "Ended");
                    Require(end.RemovedNames.SequenceEqual(new[] { "w-" + commit + ".snapshot.tmp" }), "exact partial group ended");
                    rig.SameDisk(before, end.RemovedNames);
                    Require(rig.View().Status == SaveHeadStatus.NoSave, "no fabricated initialization");
                    Bad(rig.Store.EndObservedCandidate(rig.View(), commit, B()), "CandidateNotFound");
                }
                else
                {
                    var committed = Ok(rig.Store.Lookup(commit, "init", B()), "Committed");
                    Require(committed.Descriptor.CommitId == commit, "original committed lookup");
                    Bad(rig.Store.ReadUncommittedCandidate(view, commit, B()), "AlreadyCommitted");
                    Bad(rig.Store.EndObservedCandidate(view, commit, B()), "AlreadyCommitted");
                    Require(PendingRig.Bytes(Ok(rig.Store.Load(B()), "Loaded")).SequenceEqual(originalBytes), "original committed bytes");
                    rig.SameDisk(before);
                    Require(rig.Storage.Deletes == 0 && rig.Builds == 0, "reader no deletion or rebuild");
                }
                WriteDisk(directory, "reader", rig);
                WriteNew(Path.Combine(directory, "reader-assertions.txt"),
                    "case=" + scenario + "\ncommit=" + commit + "\nwriterPid=" + writer["pid"] +
                    "\nreaderPid=" + process.Id + "\noriginalSha256=" + original["sha256"] +
                    "\nfinalStatus=" + rig.View().Status + "\nreaderBuilds=" + rig.Builds + "\npassed=true");
            }
        }

        private static string Metadata(SaveCommitTicket ticket)
        {
            return "commit=" + ticket.Metadata.CommitId + "\ngeneration=" + ticket.Metadata.SaveGeneration.ToString(CultureInfo.InvariantCulture) +
                "\nparent=" + ticket.Metadata.ParentCommitId + "\nplayer=" + ticket.Metadata.PlayerId + "\npurpose=" + ticket.Metadata.Purpose +
                "\noperations=" + string.Join("|", ticket.OperationIds) + "\nlength=" + PendingRig.Descriptor(ticket).TotalLength.ToString(CultureInfo.InvariantCulture) +
                "\nsha256=" + Hex(PendingRig.Descriptor(ticket).Sha256.ToArray()) + "\nindex=" +
                string.Join(";", ticket.Metadata.CommitIndex.Select(x => x.Generation + "," + x.CommitId + "," + x.ParentCommitId + "," +
                    x.SnapshotLength + "," + (x.SnapshotSha256 == null ? "" : Hex(x.SnapshotSha256.ToArray())) + "," + string.Join("|", x.OperationIds)));
        }
        private static void WriteDisk(string directory, string mode, PendingRig rig)
        {
            WriteNew(Path.Combine(directory, mode + "-files.txt"), string.Join("\n", rig.Disk().OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => x.Key + "\t" + x.Value.Length + "\t" + Hex(Hash(x.Value)))));
        }
        private static string Identity(Process process, string root, string directory, string nonce, string scenario, string mode, string[] args)
        {
            return "pid=" + process.Id + "\nstartTicks=" + process.StartTime.ToUniversalTime().Ticks +
                "\nstartUtc=" + process.StartTime.ToUniversalTime().ToString("o") + "\nexe=" + process.MainModule.FileName +
                "\ncwd=" + Environment.CurrentDirectory + "\nroot=" + root + "\ncaseRoot=" + directory + "\nnonce=" + nonce +
                "\ncase=" + scenario + "\nmode=" + mode + "\nrecordedUtc=" + DateTime.UtcNow.ToString("o") +
                "\nargumentsBase64=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join("\0", args)));
        }
        private static Dictionary<string, string> Fields(string text)
        {
            return text.Split('\n').Where(x => x.Contains("=")).ToDictionary(x => x.Substring(0, x.IndexOf('=')),
                x => x.Substring(x.IndexOf('=') + 1), StringComparer.Ordinal);
        }
        private static string Argument(string[] args, string name)
        {
            var at = Array.IndexOf(args, name);
            Require(at >= 0 && at + 1 < args.Length && Array.LastIndexOf(args, name) == at, name);
            return args[at + 1];
        }
        private static string EvidenceRoot(string[] args)
        {
            var root = Path.GetFullPath(Argument(args, "--b14c1-root"));
            var parent = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "TestArtifacts", "FMDemoB14C1"));
            Require(Path.GetDirectoryName(root) == parent && Guid.TryParseExact(Path.GetFileName(root), "N", out _), "dedicated evidence root");
            Safe(parent, root);
            Require(File.Exists(Path.Combine(root, "baseline.json")), "frozen task baseline exists");
            return root;
        }
        private static string Safe(string root, string path)
        {
            var full = Path.GetFullPath(path);
            Require(full.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "path containment");
            for (var p = full; !string.IsNullOrEmpty(p); p = Path.GetDirectoryName(p))
                if (File.Exists(p) || Directory.Exists(p))
                    Require((File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0, "no reparse");
            return full;
        }
        private static void WriteNew(string path, string text)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) writer.Write(text);
        }
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("B14C1: " + message);
        }
    }
}

