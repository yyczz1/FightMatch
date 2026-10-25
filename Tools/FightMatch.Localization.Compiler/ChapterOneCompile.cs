using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal static partial class Program
{
    private const string ChapterTextSource = "Config/FightMatch/Localization/FightMatchText.csv";
    private const string ChapterTextSourceSha256 = "3a47be4b0efbddeccf185aea28be9336f4ed1a1fb755a294d0e3b224800dde35";
    private const string ChapterTextRaw = "TestArtifacts/FightMatch/CH1-IMPL-001/host-text/luban-r2/fm-text-v1.json";
    private const string ChapterTextOutput = "Generated/FightMatch/Localization/ChapterOneTrial-r2/";
    private const int ChapterTextCount = 279;

    // Data projection only. It does not invoke tools, historical validation runs or publication.
    private static int RunCompileChapterOne(string[] args)
    {
        var options = ParseOptions(args, 1, "--source", "--luban-json", "--artifact", "--manifest");
        var root = Path.GetFullPath(Directory.GetCurrentDirectory());
        var sourcePath = ChapterTextPath(root, options["--source"], ChapterTextSource);
        var rawPath = ChapterTextPath(root, options["--luban-json"], ChapterTextRaw);
        var artifactPath = ChapterTextPath(root, options["--artifact"], ChapterTextOutput + "fm-text-v1.json");
        var manifestPath = ChapterTextPath(root, options["--manifest"], ChapterTextOutput + "fm-text-v1.manifest.json");
        Require(!File.Exists(artifactPath) && !Directory.Exists(artifactPath) &&
            !File.Exists(manifestPath) && !Directory.Exists(manifestPath), "chapter text output already exists");

        var source = ParseAndValidateFormal(sourcePath, ChapterTextCount);
        Require(source.Sha256 == ChapterTextSourceSha256, "chapter text source identity differs");
        var raw = File.ReadAllBytes(rawPath);
        var rows = ParseRaw(raw, ChapterTextCount);
        ValidateOneToOne(source.Rows, rows);
        var canonical = BuildCanonical(rows);
        ValidateRoundTrip(canonical, rows);
        var canonicalSha = Sha256(canonical);
        var manifest = BuildChapterTextManifest(source, rows, Sha256(raw), canonicalSha);
        Require(Sha256File(sourcePath) == source.Sha256 && Sha256File(rawPath) == Sha256(raw),
            "chapter text input changed during projection");

        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);
        ChapterTextPath(root, options["--artifact"], ChapterTextOutput + "fm-text-v1.json");
        ChapterTextPath(root, options["--manifest"], ChapterTextOutput + "fm-text-v1.manifest.json");
        WriteChapterTextNew(artifactPath, canonical);
        WriteChapterTextNew(manifestPath, manifest);
        Console.Out.WriteLine($"PROJECTED chapter-one rows={rows.Count} canonical={canonicalSha} manifest={Sha256(manifest)}; publication not invoked");
        return 0;
    }

    private static string ChapterTextPath(string root, string requested, string fixedRelative)
    {
        Require(requested == fixedRelative, "chapter text path differs from the fixed input/output mapping");
        var full = ResolveUnder(root, fixedRelative);
        for (string? current = full; current != null; current = Path.GetDirectoryName(current))
        {
            Require(!IsSymlink(current), "chapter text path contains a symlink");
            if (PathEquals(current, root)) return full;
        }
        throw new ContractException("BLOCKED_INPUT_DRIFT", "chapter text path is outside the repository", 21);
    }

    private static void WriteChapterTextNew(string path, byte[] bytes)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush(true);
    }

    private static byte[] BuildChapterTextManifest(FormalSource source, IReadOnlyList<LocalizationRow> rows,
        string rawSha, string artifactSha)
    {
        var severities = SeverityCounts(rows);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = NewJsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", 1);
            writer.WriteNumber("artifactSchemaVersion", 1);
            writer.WritePropertyName("luban");
            writer.WriteStartObject();
            writer.WriteString("tag", LubanTag);
            writer.WriteString("commit", LubanCommit);
            writer.WriteEndObject();
            writer.WriteString("sdkVersion", SdkVersion);
            writer.WriteString("sourceAuthority", ChapterTextSource);
            writer.WriteString("sourceRevision", "chapter-one-trial-r2");
            writer.WriteString("formalSourceSha256", source.Sha256);
            writer.WriteString("lubanJsonSha256", rawSha);
            writer.WriteString("canonicalArtifactSha256", artifactSha);
            writer.WriteNumber("recordCount", rows.Count);
            writer.WriteNumber("uniqueKeyCount", rows.Select(row => row.Key).Distinct(StringComparer.Ordinal).Count());
            writer.WritePropertyName("localeMapping");
            writer.WriteStartObject();
            writer.WriteString("en", "en");
            writer.WriteString("zh_cn", "zh-Hans");
            writer.WriteEndObject();
            writer.WriteNumber("parameterizedRecordCount", rows.Count(row => row.Parameters.Length > 0));
            writer.WritePropertyName("severityCounts");
            writer.WriteStartObject();
            writer.WriteNumber("blocking", severities[0]);
            writer.WriteNumber("error", severities[1]);
            writer.WriteNumber("info", severities[2]);
            writer.WriteNumber("warning", severities[3]);
            writer.WriteEndObject();
            writer.WriteString("semanticDigest", artifactSha);
            writer.WriteEndObject();
            writer.Flush();
        }
        return WithFinalLf(buffer.WrittenSpan);
    }
}
