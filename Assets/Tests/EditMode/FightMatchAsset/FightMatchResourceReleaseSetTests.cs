using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Map = System.Collections.Generic.SortedDictionary<string, object>;
using Items = System.Collections.Generic.List<object>;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchResourceReleaseSetTests
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private const string Vector0 = "{\"descriptor\":{\"businessContent\":{\"binding\":{\"ContentFingerprint\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"NumericContractVersion\":\"RC01\",\"PackageId\":\"package:fightmatch-demo-r1\",\"RandomContractVersion\":\"PC01+SC01\",\"RuleVersion\":\"demo-r1\"},\"businessReleaseSetId\":\"release-set:fightmatch-demo-r1\",\"contentReleaseSetSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[{\"length\":20443,\"name\":\"first-release.fmsource.json\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"length\":11486,\"name\":\"first-release.fmpackage.bytes\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"length\":664,\"name\":\"first-release.fmvalidation.bytes\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"length\":4766,\"name\":\"first-release.fmreview.json\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"length\":682,\"name\":\"first-release.fmpublish.json\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"length\":411,\"name\":\"first-release.fmrelease.json\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"}],\"publicationReceiptSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},\"codeCompatibility\":{\"appBuildIdentity\":\"codec-v1\",\"baseProtocolVersion\":1,\"requiredCapabilities\":[\"resource-schema-v1\"]},\"publication\":{\"authorizationEvidenceId\":\"local-test-only\",\"operationId\":\"build:codec:1\",\"receipt\":{\"authorizationEvidenceId\":\"local-test-only\",\"businessInputsSha256\":\"091fd6bd86a735f2030cec61f5cd6fd17fbc7ca9ffc87dc9f2cdd7c41e105be0\",\"kind\":\"resource-inputs-receipt-v1\",\"operationId\":\"build:codec:1\",\"schemaVersion\":1,\"sourcePlanSha256\":\"148de9c5a7a44d19e56cd9ae1a554bf67847afb0c58f6e12fa29ac7ddfca9940\",\"textSourceReceiptSha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},\"receiptSha256\":\"5d2d2a30236f965ba985ceb90ce9e62b2c21d8faaf9038005ec06d92bd9ee812\"},\"releaseSetId\":\"fm.codec.r1\",\"resources\":{\"manifestLength\":1,\"manifestName\":\"manifest/main.bytes\",\"manifestSha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\",\"mappingDescriptorSha256\":\"ef622bf08c4209e93bd527bb9938312279c39314f42d265e05ef788d25882fcd\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"requiredScopeHashes\":[{\"scopeId\":\"fm.content.first-release\",\"sha256\":\"dab086f2f4f73484bfcbfd9d2f1f3564af2bc303795c704680bcd4f968ea22ef\"},{\"scopeId\":\"fm.text.full\",\"sha256\":\"5a8840058b97e4a8b5ce3356b096232e1465b17d59ef4b260b80305dac592424\"}],\"yooAssetPackageVersion\":\"3.0.6\",\"yooManifestPackageVersion\":\"codec-v1\"},\"schemaVersion\":1,\"text\":{\"artifactLength\":1,\"artifactName\":\"fm-text-v1.json\",\"artifactSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"manifestLength\":1,\"manifestName\":\"fm-text-v1.manifest.json\",\"manifestSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"schemaVersion\":1,\"sourceReceipt\":{\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"}}},\"descriptorSha256\":\"87b31f3bf93bf7456a44ccfc7d42f747b22166b8731ddd2c446ada067d156f8a\",\"mapping\":{\"entries\":[{\"assetId\":\"fm.content.package\",\"contentLength\":11486,\"contentSha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"files\":[\"raw/b2\"],\"kind\":\"raw\",\"location\":\"first-release.fmpackage.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.publication\",\"contentLength\":682,\"contentSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\",\"files\":[\"raw/b5\"],\"kind\":\"raw\",\"location\":\"first-release.fmpublish.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.release-set\",\"contentLength\":411,\"contentSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[\"raw/b6\"],\"kind\":\"raw\",\"location\":\"first-release.fmrelease.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.review\",\"contentLength\":4766,\"contentSha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\",\"files\":[\"raw/b4\"],\"kind\":\"raw\",\"location\":\"first-release.fmreview.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.source\",\"contentLength\":20443,\"contentSha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\",\"files\":[\"raw/b1\"],\"kind\":\"raw\",\"location\":\"first-release.fmsource.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.validation\",\"contentLength\":664,\"contentSha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\",\"files\":[\"raw/b3\"],\"kind\":\"raw\",\"location\":\"first-release.fmvalidation.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.artifact\",\"contentLength\":1,\"contentSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"files\":[\"raw/text\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.manifest\",\"contentLength\":1,\"contentSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"files\":[\"raw/text-manifest\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.manifest.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"}],\"schemaVersion\":1},\"physicalFiles\":[{\"kind\":\"receipt\",\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},{\"kind\":\"manifest\",\"length\":1,\"name\":\"manifest/main.bytes\",\"sha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\"},{\"kind\":\"raw\",\"length\":20443,\"name\":\"raw/b1\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"kind\":\"raw\",\"length\":11486,\"name\":\"raw/b2\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"kind\":\"raw\",\"length\":664,\"name\":\"raw/b3\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"kind\":\"raw\",\"length\":4766,\"name\":\"raw/b4\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"kind\":\"raw\",\"length\":682,\"name\":\"raw/b5\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"kind\":\"raw\",\"length\":411,\"name\":\"raw/b6\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text\",\"sha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text-manifest\",\"sha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\"}],\"schemaVersion\":1}";
        private const string Pin0 = "b7886f1f61f075f30a8191dac86943bfeada316c336bea1dfc578f815f98db46";
        private const string Descriptor0 = "87b31f3bf93bf7456a44ccfc7d42f747b22166b8731ddd2c446ada067d156f8a";
        private const string Vector1 = "{\"descriptor\":{\"businessContent\":{\"binding\":{\"ContentFingerprint\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"NumericContractVersion\":\"RC01\",\"PackageId\":\"package:fightmatch-demo-r1\",\"RandomContractVersion\":\"PC01+SC01\",\"RuleVersion\":\"demo-r1\"},\"businessReleaseSetId\":\"release-set:fightmatch-demo-r1\",\"contentReleaseSetSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[{\"length\":20443,\"name\":\"first-release.fmsource.json\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"length\":11486,\"name\":\"first-release.fmpackage.bytes\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"length\":664,\"name\":\"first-release.fmvalidation.bytes\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"length\":4766,\"name\":\"first-release.fmreview.json\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"length\":682,\"name\":\"first-release.fmpublish.json\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"length\":411,\"name\":\"first-release.fmrelease.json\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"}],\"publicationReceiptSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},\"codeCompatibility\":{\"appBuildIdentity\":\"codec-v1\",\"baseProtocolVersion\":1,\"requiredCapabilities\":[\"resource-schema-v1\"]},\"publication\":{\"authorizationEvidenceId\":\"local-test-only\",\"operationId\":\"build:codec:1\",\"receipt\":{\"authorizationEvidenceId\":\"local-test-only\",\"businessInputsSha256\":\"091fd6bd86a735f2030cec61f5cd6fd17fbc7ca9ffc87dc9f2cdd7c41e105be0\",\"kind\":\"resource-inputs-receipt-v1\",\"operationId\":\"build:codec:1\",\"schemaVersion\":1,\"sourcePlanSha256\":\"148de9c5a7a44d19e56cd9ae1a554bf67847afb0c58f6e12fa29ac7ddfca9940\",\"textSourceReceiptSha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},\"receiptSha256\":\"5d2d2a30236f965ba985ceb90ce9e62b2c21d8faaf9038005ec06d92bd9ee812\"},\"releaseSetId\":\"fm.codec.r1\",\"resources\":{\"manifestLength\":1,\"manifestName\":\"manifest/main.bytes\",\"manifestSha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\",\"mappingDescriptorSha256\":\"8bfba8d3420a26110e057021cfb62c29f159c56300890eae407846aacf9f4c6b\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"requiredScopeHashes\":[{\"scopeId\":\"fm.content.first-release\",\"sha256\":\"dab086f2f4f73484bfcbfd9d2f1f3564af2bc303795c704680bcd4f968ea22ef\"},{\"scopeId\":\"fm.text.full\",\"sha256\":\"5a8840058b97e4a8b5ce3356b096232e1465b17d59ef4b260b80305dac592424\"},{\"scopeId\":\"fm.ui.runtime\",\"sha256\":\"63dfba3b30cdaf0372e7fe132a79f60916359ae1b6263c0d63cfbe76282b5884\"}],\"yooAssetPackageVersion\":\"3.0.6\",\"yooManifestPackageVersion\":\"codec-v1\"},\"schemaVersion\":1,\"text\":{\"artifactLength\":1,\"artifactName\":\"fm-text-v1.json\",\"artifactSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"manifestLength\":1,\"manifestName\":\"fm-text-v1.manifest.json\",\"manifestSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"schemaVersion\":1,\"sourceReceipt\":{\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"}}},\"descriptorSha256\":\"f570efbe5ee762e035f957ad9be05f3521f94437e8a938ca8125df9524dc0936\",\"mapping\":{\"entries\":[{\"assetId\":\"fm.content.package\",\"contentLength\":11486,\"contentSha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"files\":[\"raw/b2\"],\"kind\":\"raw\",\"location\":\"first-release.fmpackage.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.publication\",\"contentLength\":682,\"contentSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\",\"files\":[\"raw/b5\"],\"kind\":\"raw\",\"location\":\"first-release.fmpublish.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.release-set\",\"contentLength\":411,\"contentSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[\"raw/b6\"],\"kind\":\"raw\",\"location\":\"first-release.fmrelease.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.review\",\"contentLength\":4766,\"contentSha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\",\"files\":[\"raw/b4\"],\"kind\":\"raw\",\"location\":\"first-release.fmreview.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.source\",\"contentLength\":20443,\"contentSha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\",\"files\":[\"raw/b1\"],\"kind\":\"raw\",\"location\":\"first-release.fmsource.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.validation\",\"contentLength\":664,\"contentSha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\",\"files\":[\"raw/b3\"],\"kind\":\"raw\",\"location\":\"first-release.fmvalidation.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.artifact\",\"contentLength\":1,\"contentSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"files\":[\"raw/text\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.manifest\",\"contentLength\":1,\"contentSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"files\":[\"raw/text-manifest\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.manifest.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.ui.root\",\"contentLength\":1,\"contentSha256\":\"e2fd09070ebe49d531e84a0d865741edb28afe10363124dd5476aa678a7705f8\",\"files\":[\"bundles/ui\"],\"kind\":\"object\",\"location\":\"ui/root\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.ui.runtime\",\"unityType\":\"UnityEngine.GameObject\"}],\"schemaVersion\":1},\"physicalFiles\":[{\"kind\":\"bundle\",\"length\":1,\"name\":\"bundles/ui\",\"sha256\":\"3e23e8160039594a33894f6564e1b1348bbd7a0088d42c4acb73eeaed59c009d\"},{\"kind\":\"receipt\",\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},{\"kind\":\"manifest\",\"length\":1,\"name\":\"manifest/main.bytes\",\"sha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\"},{\"kind\":\"raw\",\"length\":20443,\"name\":\"raw/b1\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"kind\":\"raw\",\"length\":11486,\"name\":\"raw/b2\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"kind\":\"raw\",\"length\":664,\"name\":\"raw/b3\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"kind\":\"raw\",\"length\":4766,\"name\":\"raw/b4\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"kind\":\"raw\",\"length\":682,\"name\":\"raw/b5\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"kind\":\"raw\",\"length\":411,\"name\":\"raw/b6\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text\",\"sha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text-manifest\",\"sha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\"}],\"schemaVersion\":1}";
        private const string Pin1 = "8a9787948fb67de91cdeb7874cee590803ca80549d2d1663e7246dbc47d2ac4f";
        private const string Descriptor1 = "f570efbe5ee762e035f957ad9be05f3521f94437e8a938ca8125df9524dc0936";
        private const string ZeroHash = "0000000000000000000000000000000000000000000000000000000000000000";
        private static readonly string[] Names =
        {
            "first-release.fmsource.json", "first-release.fmpackage.bytes", "first-release.fmvalidation.bytes",
            "first-release.fmreview.json", "first-release.fmpublish.json", "first-release.fmrelease.json"
        };
        private static readonly long[] Lengths = { 20443, 11486, 664, 4766, 682, 411 };
        private static readonly string[] Hashes =
        {
            "fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a",
            "b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129",
            "512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d",
            "16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75",
            "feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910",
            "03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516"
        };
        private static readonly string[] Ids =
        {
            "fm.content.source", "fm.content.package", "fm.content.validation",
            "fm.content.review", "fm.content.publication", "fm.content.release-set"
        };

        // Independently composed test data, pinned to literal vectors generated outside the codec.
        private static Map M(params object[] fields)
        {
            var result = new Map(StringComparer.Ordinal);
            for (var i = 0; i < fields.Length; i += 2) result.Add((string)fields[i], fields[i + 1]);
            return result;
        }
        private static Items L(params object[] values) { return new Items(values); }
        private static object Get(Map root, params string[] path)
        {
            object value = root;
            foreach (var key in path) value = ((Map)value)[key];
            return value;
        }
        private static Map O(Map root, params string[] path) { return (Map)Get(root, path); }
        private static Items A(Map root, params string[] path) { return (Items)Get(root, path); }
        private static Map Entry(Map root, string id)
        {
            return A(root, "mapping", "entries").Cast<Map>().Single(e => (string)e["assetId"] == id);
        }
        private static Map Physical(Map root, string name)
        {
            return A(root, "physicalFiles").Cast<Map>().Single(f => (string)f["name"] == name);
        }
        private static string H(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        private static string H(object node) { return H(Bytes(node)); }
        private static byte[] Bytes(object node)
        {
            var text = new StringBuilder();
            Write(text, node);
            return Utf8.GetBytes(text.ToString());
        }
        private static void Write(StringBuilder text, object node)
        {
            if (node is string s)
            {
                text.Append('"');
                foreach (var c in s)
                {
                    if (c == '"' || c == '\\') text.Append('\\').Append(c);
                    else if (c < 32) text.Append("\\u").Append(((int)c).ToString("x4"));
                    else text.Append(c);
                }
                text.Append('"');
            }
            else if (node is Map map)
            {
                text.Append('{');
                var first = true;
                foreach (var pair in map)
                {
                    if (!first) text.Append(',');
                    first = false;
                    Write(text, pair.Key);
                    text.Append(':');
                    Write(text, pair.Value);
                }
                text.Append('}');
            }
            else if (node is Items array)
            {
                text.Append('[');
                for (var i = 0; i < array.Count; i++) { if (i > 0) text.Append(','); Write(text, array[i]); }
                text.Append(']');
            }
            else if (node == null) text.Append("null");
            else if (node is bool flag) text.Append(flag ? "true" : "false");
            else text.Append(Convert.ToString(node, CultureInfo.InvariantCulture));
        }
        private static Map F(string name, long length, string hash)
        {
            return M("length", length, "name", name, "sha256", hash);
        }
        private static Map P(string name, long length, string hash, string kind)
        {
            return M("kind", kind, "length", length, "name", name, "sha256", hash);
        }
        private static Map E(string id, string scope, string name, string location, long length, string hash, bool raw = true)
        {
            return M("assetId", id, "contentLength", length, "contentSha256", hash, "files", L(name),
                "kind", raw ? "raw" : "object", "location", location, "packageName", "FightMatchMain",
                "platform", "android", "releaseSetId", "fm.codec.r1", "scopeId", scope,
                "unityType", raw ? "" : "UnityEngine.GameObject");
        }
        private static Map Build(bool withObject = false)
        {
            var files = new Items();
            var physical = new Items();
            var entries = new Items();
            for (var i = 0; i < 6; i++)
            {
                files.Add(F(Names[i], Lengths[i], Hashes[i]));
                physical.Add(P("raw/b" + (i + 1), Lengths[i], Hashes[i], "raw"));
                entries.Add(E(Ids[i], "fm.content.first-release", "raw/b" + (i + 1), Names[i], Lengths[i], Hashes[i]));
            }
            var x = H(Utf8.GetBytes("x"));
            var y = H(Utf8.GetBytes("y"));
            var z = H(Utf8.GetBytes("z"));
            var m = H(Utf8.GetBytes("m"));
            physical.Add(P("raw/text", 1, x, "raw"));
            physical.Add(P("raw/text-manifest", 1, y, "raw"));
            physical.Add(P("inputs/text.receipt", 1, z, "receipt"));
            physical.Add(P("manifest/main.bytes", 1, m, "manifest"));
            entries.Add(E("fm.text.full.artifact", "fm.text.full", "raw/text", "fm-text-v1.json", 1, x));
            entries.Add(E("fm.text.full.manifest", "fm.text.full", "raw/text-manifest", "fm-text-v1.manifest.json", 1, y));
            if (withObject)
            {
                physical.Add(P("bundles/ui", 1, H(Utf8.GetBytes("b")), "bundle"));
                entries.Add(E("fm.ui.root", "fm.ui.runtime", "bundles/ui", "ui/root", 1, ZeroHash, false));
            }
            physical.Sort((a, b) => string.CompareOrdinal((string)((Map)a)["name"], (string)((Map)b)["name"]));
            entries.Sort((a, b) => string.CompareOrdinal((string)((Map)a)["assetId"], (string)((Map)b)["assetId"]));
            var receipt = M("authorizationEvidenceId", "local-test-only", "businessInputsSha256", ZeroHash,
                "kind", "resource-inputs-receipt-v1", "operationId", "build:codec:1", "schemaVersion", 1L,
                "sourcePlanSha256", H(Utf8.GetBytes("p")), "textSourceReceiptSha256", z);
            var descriptor = M(
                "businessContent", M("binding", M("ContentFingerprint", Hashes[1], "NumericContractVersion", "RC01",
                    "PackageId", "package:fightmatch-demo-r1", "RandomContractVersion", "PC01+SC01", "RuleVersion", "demo-r1"),
                    "businessReleaseSetId", "release-set:fightmatch-demo-r1", "contentReleaseSetSha256", Hashes[5],
                    "files", files, "publicationReceiptSha256", Hashes[4]),
                "codeCompatibility", M("appBuildIdentity", "codec-v1", "baseProtocolVersion", 1L, "requiredCapabilities", L("resource-schema-v1")),
                "publication", M("authorizationEvidenceId", "local-test-only", "operationId", "build:codec:1",
                    "receipt", receipt, "receiptSha256", ZeroHash),
                "releaseSetId", "fm.codec.r1",
                "resources", M("manifestLength", 1L, "manifestName", "manifest/main.bytes", "manifestSha256", m,
                    "mappingDescriptorSha256", ZeroHash, "packageName", "FightMatchMain", "platform", "android",
                    "requiredScopeHashes", new Items(), "yooAssetPackageVersion", "3.0.6", "yooManifestPackageVersion", "codec-v1"),
                "schemaVersion", 1L,
                "text", M("artifactLength", 1L, "artifactName", "fm-text-v1.json", "artifactSha256", x,
                    "manifestLength", 1L, "manifestName", "fm-text-v1.manifest.json", "manifestSha256", y,
                    "schemaVersion", 1L, "sourceReceipt", F("inputs/text.receipt", 1, z)));
            var root = M("descriptor", descriptor, "descriptorSha256", ZeroHash,
                "mapping", M("entries", entries, "schemaVersion", 1L), "physicalFiles", physical, "schemaVersion", 1L);
            Rebind(root);
            return root;
        }
        private static void Outer(Map root) { root["descriptorSha256"] = H(root["descriptor"]); }
        private static void Parents(Map root)
        {
            O(root, "descriptor", "resources")["mappingDescriptorSha256"] = H(root["mapping"]);
            var publication = O(root, "descriptor", "publication");
            publication["receiptSha256"] = H(publication["receipt"]);
            Outer(root);
        }
        private static void Rebind(Map root)
        {
            var physical = A(root, "physicalFiles").Cast<Map>().ToDictionary(f => (string)f["name"], StringComparer.Ordinal);
            var entries = A(root, "mapping", "entries").Cast<Map>().ToArray();
            foreach (var entry in entries.Where(e => (string)e["kind"] == "object"))
            {
                var referenced = new Items(((Items)entry["files"]).Cast<string>().Select(n => (object)physical[n]));
                entry["contentLength"] = referenced.Cast<Map>().Sum(f => (long)f["length"]);
                entry["contentSha256"] = H(referenced);
            }
            var scopes = new Items();
            foreach (var group in entries.GroupBy(e => (string)e["scopeId"]).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var assets = new Items(group.Cast<object>());
                var names = group.SelectMany(e => ((Items)e["files"]).Cast<string>()).Distinct().OrderBy(n => n, StringComparer.Ordinal);
                var files = new Items(names.Select(n => (object)physical[n]));
                scopes.Add(M("scopeId", group.Key, "sha256", H(M("assets", assets, "files", files, "scopeId", group.Key))));
            }
            O(root, "descriptor", "resources")["requiredScopeHashes"] = scopes;
            O(root, "descriptor", "publication", "receipt")["businessInputsSha256"] = H(Get(root, "descriptor", "businessContent", "files"));
            O(root, "descriptor", "publication", "receipt")["textSourceReceiptSha256"] =
                Get(root, "descriptor", "text", "sourceReceipt", "sha256");
            Parents(root);
        }
        private static FightMatchResourceReleaseSet Accept(byte[] bytes, string pin = null)
        {
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(bytes, pin ?? H(bytes), out var result, out var code), code);
            Assert.IsNotNull(result);
            Assert.IsNull(code);
            CollectionAssert.AreEqual(bytes, result.EncodeCanonical());
            return result;
        }
        private static void Reject(byte[] bytes, string code, string pin = null)
        {
            Assert.IsFalse(FightMatchResourceReleaseSet.TryDecodePinned(bytes, pin ?? (bytes == null ? ZeroHash : H(bytes)),
                out var result, out var actual));
            Assert.IsNull(result);
            Assert.AreEqual(code, actual);
        }
        private static void Reject(Map root, string code, bool repairParents = true)
        {
            if (repairParents) Parents(root);
            Reject(Bytes(root), code);
        }
        private static void Mutate(Action<Map> edit, string code = "RES_SCHEMA", bool withObject = false)
        {
            var root = Build(withObject);
            edit(root);
            Reject(root, code);
        }
        private static string ReplaceFirst(string input, string oldValue, string newValue)
        {
            var at = input.IndexOf(oldValue, StringComparison.Ordinal);
            Assert.GreaterOrEqual(at, 0);
            return input.Remove(at, oldValue.Length).Insert(at, newValue);
        }
        private static void BusinessLength(Map root, int index, long length)
        {
            ((Map)A(root, "descriptor", "businessContent", "files")[index])["length"] = length;
            Entry(root, Ids[index])["contentLength"] = length;
            Physical(root, "raw/b" + (index + 1))["length"] = length;
        }
        private static void PadSegment(Map root, string part, int target)
        {
            var map = O(root, part);
            var padding = new Items();
            map["zz"] = padding;
            var remaining = target - Bytes(map).Length;
            while (remaining > 258)
            {
                padding.Add(new string('a', 255));
                remaining -= padding.Count == 1 ? 257 : 258;
            }
            if (remaining == 1) padding[padding.Count - 1] = (string)padding[padding.Count - 1] + "a";
            else if (remaining == 2) padding.Add(1L);
            else if (remaining >= 3) padding.Add(new string('a', remaining - 3));
            Assert.AreEqual(target, Bytes(map).Length);
            Assert.LessOrEqual(padding.Count, 512);
        }

        [Test]
        public void D01_CanonicalVectorRoundTrips()
        {
            Assert.AreEqual(Vector0, Utf8.GetString(Bytes(Build())));
            Assert.AreEqual(Vector1, Utf8.GetString(Bytes(Build(true))));
            Assert.AreEqual(Pin0, H(Utf8.GetBytes(Vector0)));
            Assert.AreEqual(Pin1, H(Utf8.GetBytes(Vector1)));
            var first = Accept(Utf8.GetBytes(Vector0), Pin0);
            var second = Accept(Utf8.GetBytes(Vector1), Pin1);
            Assert.AreEqual(1, first.SchemaVersion);
            Assert.AreEqual("fm.codec.r1", first.ReleaseSetId);
            Assert.AreEqual("release-set:fightmatch-demo-r1", first.BusinessReleaseSetId);
            Assert.AreEqual("android", first.Platform);
            Assert.AreEqual(Descriptor0, first.DescriptorSha256);
            Assert.AreEqual(Descriptor1, second.DescriptorSha256);
        }

        [Test]
        public void D02_ExternalPinIsRequired()
        {
            foreach (var pin in new[] { null, "", "bad", Pin0.ToUpperInvariant(), ZeroHash })
            {
                Assert.IsFalse(FightMatchResourceReleaseSet.TryDecodePinned(Utf8.GetBytes(Vector0), pin, out var value, out var code));
                Assert.IsNull(value);
                Assert.AreEqual("RES_TRUST", code);
            }
            Reject((byte[])null, "RES_SCHEMA");
            Reject(new byte[0], "RES_SCHEMA");
            var changed = Utf8.GetBytes(Vector1);
            Reject(changed, "RES_TRUST", Pin0);
            var selfReported = Build();
            selfReported["pin"] = Pin0;
            Reject(selfReported, "RES_SCHEMA");
        }

        [Test]
        public void D03_MatchingPinDoesNotEstablishAuthority()
        {
            var claimed = Build();
            O(claimed, "descriptor", "publication")["authorizationEvidenceId"] = "unapproved-claim";
            O(claimed, "descriptor", "publication", "receipt")["authorizationEvidenceId"] = "unapproved-claim";
            O(claimed, "descriptor", "codeCompatibility")["appBuildIdentity"] = "unsupported-claim";
            O(claimed, "descriptor", "publication", "receipt")["sourcePlanSha256"] = ZeroHash;
            Parents(claimed);
            var bytes = Bytes(claimed);
            Accept(bytes, H(bytes));
            Reject(bytes, "RES_TRUST", Pin0);
            Assert.AreNotEqual(Pin0, H(bytes), "Self-selected matching pin proves consistency only.");
        }

        [Test]
        public void D04_ExactKeysAndTrailingAreRejected()
        {
            foreach (var path in new[]
            {
                new string[0], new[] { "descriptor" }, new[] { "descriptor", "businessContent" },
                new[] { "descriptor", "businessContent", "binding" }, new[] { "descriptor", "codeCompatibility" },
                new[] { "descriptor", "text" }, new[] { "descriptor", "text", "sourceReceipt" },
                new[] { "descriptor", "resources" }, new[] { "descriptor", "publication" },
                new[] { "descriptor", "publication", "receipt" }, new[] { "mapping" }
            })
            {
                var missing = Build();
                var map = O(missing, path);
                map.Remove(map.Keys.First());
                Reject(missing, "RES_SCHEMA", false);
                var extra = Build();
                O(extra, path)["unknown"] = 1L;
                Reject(extra, "RES_SCHEMA", false);
            }
            foreach (var suffix in new[] { " ", "\n", "{}", "x" })
                Reject(Utf8.GetBytes(Vector0 + suffix), "RES_SCHEMA");
            Reject(Utf8.GetBytes(ReplaceFirst(Vector0, "\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1")), "RES_SCHEMA");
            foreach (var value in new object[] { null, true, false, "1", L(1L) })
                Mutate(r => O(r, "descriptor")["schemaVersion"] = value);
            foreach (var edit in new Action<Map>[]
            {
                r => Entry(r, Ids[0]).Remove("files"),
                r => Physical(r, "raw/b1")["unknown"] = 1L,
                r => ((Map)A(r, "descriptor", "resources", "requiredScopeHashes")[0]).Remove("sha256"),
                r => ((Map)A(r, "descriptor", "businessContent", "files")[0]).Remove("length")
            }) Mutate(edit);
        }

        [Test]
        public void D05_EncodingAndIntegerGrammarAreStrict()
        {
            Reject(new byte[] { 0xff }, "RES_SCHEMA");
            Reject(new byte[] { 34, 0xed, 0xa0, 0x80, 34 }, "RES_SCHEMA");
            Reject(new byte[] { 0xef, 0xbb, 0xbf }.Concat(Utf8.GetBytes(Vector0)).ToArray(), "RES_SCHEMA");
            foreach (var raw in new[] { "\"\\ud800\"", "\"\\u0061\"", "\"\\n\"", "1e0", "1.0", "+1", "-1", "0", "01" })
                Reject(Utf8.GetBytes(ReplaceFirst(Vector0, "\"baseProtocolVersion\":1", "\"baseProtocolVersion\":" + raw)), "RES_SCHEMA");
            foreach (var raw in new[] { "2147483648", "9999999999", "10000000000", "9223372036854775808" })
                Reject(Utf8.GetBytes(ReplaceFirst(Vector0, "\"baseProtocolVersion\":1", "\"baseProtocolVersion\":" + raw)), "RES_BUDGET");
            foreach (var number in new long[] { 1, 2147483647 })
            {
                var root = Build();
                O(root, "descriptor", "codeCompatibility")["baseProtocolVersion"] = number;
                Parents(root);
                Accept(Bytes(root));
            }
            Mutate(r => O(r, "descriptor", "codeCompatibility")["appBuildIdentity"] = "non\u00e9ascii");
            Reject(Utf8.GetBytes(ReplaceFirst(Vector0, "codec-v1", "codec\\/v1")), "RES_SCHEMA");
        }

        [Test]
        public void D06_ByteDepthNodeEntryStringBudgetsHold()
        {
            foreach (var length in new[] { 262143, 262144, 262145 })
                Reject(Enumerable.Repeat((byte)'x', length).ToArray(), length > 262144 ? "RES_BUDGET" : "RES_SCHEMA");
            foreach (var depth in new[] { 11, 12, 13 })
            {
                var json = "1";
                for (var i = 1; i < depth; i++) json = "[" + json + "]";
                Reject(Utf8.GetBytes(json), depth > 12 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            foreach (var last in new[] { 509, 510, 511 })
            {
                var root = new Items();
                for (var i = 0; i < 32; i++) root.Add(new Items(Enumerable.Repeat((object)1L, i == 31 ? last : 511)));
                Reject(Bytes(root), last == 511 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            foreach (var length in new[] { 0, 255, 256, 257 })
            {
                var root = Build();
                O(root, "descriptor", "codeCompatibility")["appBuildIdentity"] = new string('a', length);
                Parents(root);
                if (length > 0 && length <= 256) Accept(Bytes(root));
                else Reject(root, length == 0 ? "RES_SCHEMA" : "RES_BUDGET");
            }
            foreach (var length in new[] { 256, 257, 341, 342 })
                Mutate(r => O(r, "descriptor", "codeCompatibility")["appBuildIdentity"] = new string('\u4e2d', length),
                    length == 256 ? "RES_SCHEMA" : "RES_BUDGET");
            foreach (var count in new[] { 0, 255, 256, 257 })
            {
                var root = Build();
                var entries = A(root, "mapping", "entries");
                var entry = entries[0];
                entries.Clear();
                for (var i = 0; i < count; i++) entries.Add(entry);
                Reject(root, count > 256 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            foreach (var count in new[] { 0, 511, 512, 513 })
            {
                var root = Build();
                root["zz"] = new Items(Enumerable.Repeat((object)1L, count));
                Reject(root, count > 512 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            foreach (var length in new[] { 63, 64, 65 })
                Mutate(r => r[new string('z', length)] = 1L, length > 64 ? "RES_BUDGET" : "RES_SCHEMA");
            foreach (var count in new[] { 15, 16, 17 })
            {
                var root = Build();
                for (var i = root.Count; i < count; i++) root["z" + i.ToString("D2")] = 1L;
                Reject(root, count > 16 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            foreach (var part in new[] { "descriptor", "mapping" })
            {
                var limit = part == "descriptor" ? 32768 : 131072;
                foreach (var delta in new[] { -1, 0, 1 })
                {
                    var root = Build();
                    PadSegment(root, part, limit + delta);
                    Reject(root, delta > 0 ? "RES_BUDGET" : "RES_SCHEMA");
                }
            }
            foreach (var count in new[] { 0, 63, 64, 65 })
                Mutate(r => Entry(r, Ids[0])["files"] = new Items(Enumerable.Repeat((object)"raw/b1", count)),
                    count > 64 ? "RES_BUDGET" : "RES_SCHEMA");
            foreach (var count in new[] { 0, 32, 33 })
            {
                var root = Build();
                O(root, "descriptor", "codeCompatibility")["requiredCapabilities"] =
                    new Items(Enumerable.Range(0, count).Select(i => (object)("cap" + i.ToString("D2"))));
                Parents(root);
                if (count == 32) Accept(Bytes(root));
                else Reject(root, count == 0 ? "RES_SCHEMA" : "RES_BUDGET");
            }
        }

        [Test]
        public void D07_PhysicalLengthsAndTotalsAreBounded()
        {
            foreach (var length in new long[] { 0, 268435455, 268435456, 268435457, 9999999999 })
            {
                var root = Build();
                Physical(root, "manifest/main.bytes")["length"] = length;
                O(root, "descriptor", "resources")["manifestLength"] = length;
                Parents(root);
                if (length > 0 && length <= 268435456) Accept(Bytes(root));
                else Reject(root, length == 0 ? "RES_SCHEMA" : "RES_BUDGET");
            }
            foreach (var total in new long[] { 1073741823, 1073741824, 1073741825 })
            {
                var root = Build(true);
                var files = A(root, "physicalFiles");
                var other = files.Cast<Map>().Where(f => (string)f["kind"] != "bundle").Sum(f => (long)f["length"]);
                Physical(root, "bundles/ui")["length"] = 268435456L;
                files.Add(P("bundles/v1", 268435456, ZeroHash, "bundle"));
                files.Add(P("bundles/v2", 268435456, ZeroHash, "bundle"));
                files.Add(P("bundles/v3", total - other - 3 * 268435456L, ZeroHash, "bundle"));
                files.Sort((a, b) => string.CompareOrdinal((string)((Map)a)["name"], (string)((Map)b)["name"]));
                Entry(root, "fm.ui.root")["files"] = L("bundles/ui", "bundles/v1", "bundles/v2", "bundles/v3");
                Rebind(root);
                if (total <= 1073741824) Accept(Bytes(root)); else Reject(root, "RES_BUDGET");
            }
            foreach (var length in new long[] { 16777215, 16777216, 16777217 })
            {
                var root = Build();
                BusinessLength(root, 0, length);
                Rebind(root);
                if (length <= 16777216) Accept(Bytes(root)); else Reject(root, "RES_BUDGET");
                foreach (var slot in new[] { "artifact", "manifest", "sourceReceipt" })
                {
                    var text = Build();
                    if (slot == "sourceReceipt")
                    {
                        O(text, "descriptor", "text", slot)["length"] = length;
                        Physical(text, "inputs/text.receipt")["length"] = length;
                    }
                    else
                    {
                        O(text, "descriptor", "text")[slot + "Length"] = length;
                        Entry(text, "fm.text.full." + slot)["contentLength"] = length;
                        Physical(text, slot == "artifact" ? "raw/text" : "raw/text-manifest")["length"] = length;
                    }
                    Rebind(text);
                    if (length <= 16777216) Accept(Bytes(text)); else Reject(text, "RES_BUDGET");
                }
            }
            foreach (var total in new long[] { 33554431, 33554432, 33554433 })
            {
                var root = Build();
                BusinessLength(root, 0, 16777216);
                BusinessLength(root, 1, total - 16777216 - Lengths.Skip(2).Sum());
                Rebind(root);
                if (total <= 33554432) Accept(Bytes(root)); else Reject(root, "RES_BUDGET");
            }
            foreach (var count in new[] { 0, 511, 512, 513 })
            {
                var root = Build();
                root["physicalFiles"] = new Items(Enumerable.Repeat(A(root, "physicalFiles")[0], count));
                Reject(root, count > 512 ? "RES_BUDGET" : "RES_SCHEMA");
            }
            Mutate(r => O(r, "descriptor", "text", "sourceReceipt")["length"] = 16777217L, "RES_BUDGET");
            Mutate(r => O(r, "descriptor", "text")["manifestLength"] = 16777217L, "RES_BUDGET");
        }

        [Test]
        public void D08_AllNestedHashesAreBound()
        {
            var badRoot = Build();
            badRoot["descriptorSha256"] = ZeroHash;
            Reject(badRoot, "RES_HASH", false);
            foreach (var target in new[] { "mappingDescriptorSha256", "receiptSha256" })
            {
                var root = Build();
                O(root, "descriptor", target == "receiptSha256" ? "publication" : "resources")[target] = ZeroHash;
                Outer(root);
                Reject(root, "RES_HASH", false);
            }
            foreach (var edit in new Action<Map>[]
            {
                r => O(r, "descriptor", "publication", "receipt")["businessInputsSha256"] = ZeroHash,
                r => O(r, "descriptor", "publication", "receipt")["textSourceReceiptSha256"] = ZeroHash,
                r => O(r, "descriptor", "businessContent", "binding")["ContentFingerprint"] = ZeroHash,
                r => O(r, "descriptor", "businessContent")["contentReleaseSetSha256"] = ZeroHash,
                r => O(r, "descriptor", "businessContent")["publicationReceiptSha256"] = ZeroHash,
                r => O(r, "descriptor", "resources")["manifestSha256"] = ZeroHash,
                r => O(r, "descriptor", "text")["artifactSha256"] = ZeroHash,
                r => O(r, "descriptor", "text")["manifestSha256"] = ZeroHash,
                r => Entry(r, Ids[0])["contentSha256"] = ZeroHash,
                r => Physical(r, "raw/b1")["sha256"] = ZeroHash,
                r => ((Map)A(r, "descriptor", "resources", "requiredScopeHashes")[0])["sha256"] = ZeroHash
            }) Mutate(edit, "RES_HASH");
            Mutate(r => Entry(r, "fm.ui.root")["contentSha256"] = ZeroHash, "RES_HASH", true);
            var business = Build();
            ((Map)A(business, "descriptor", "businessContent", "files")[0])["sha256"] = ZeroHash;
            O(business, "descriptor", "publication", "receipt")["businessInputsSha256"] =
                H(Get(business, "descriptor", "businessContent", "files"));
            Reject(business, "RES_HASH");
            var source = Build();
            O(source, "descriptor", "text", "sourceReceipt")["sha256"] = ZeroHash;
            O(source, "descriptor", "publication", "receipt")["textSourceReceiptSha256"] = ZeroHash;
            Reject(source, "RES_HASH");
        }

        [Test]
        public void D09_PhysicalAndScopeReferencesAreClosed()
        {
            var shared = Build(true);
            var another = new Map(Entry(shared, "fm.ui.root"), StringComparer.Ordinal);
            another["assetId"] = "fm.ui.second";
            another["location"] = "ui/second";
            A(shared, "mapping", "entries").Add(another);
            Rebind(shared);
            Accept(Bytes(shared));
            foreach (var edit in new Action<Map>[]
            {
                r => Entry(r, Ids[0])["files"] = L("missing"),
                r => Entry(r, Ids[0])["files"] = L("raw/b1", "raw/b1"),
                r => A(r, "physicalFiles").Add(P("zz/orphan", 1, ZeroHash, "raw")),
                r => A(r, "physicalFiles").Add(P("raw/text", 1, ZeroHash, "raw")),
                r => Physical(r, "raw/b1")["kind"] = "bundle",
                r => Physical(r, "manifest/main.bytes")["kind"] = "raw",
                r => Physical(r, "raw/b1")["kind"] = "manifest",
                r => Physical(r, "raw/b1")["length"] = Lengths[0] + 1,
                r => O(r, "descriptor", "resources")["manifestName"] = "missing",
                r => O(r, "descriptor", "text", "sourceReceipt")["name"] = "missing",
                r => O(r, "descriptor", "resources")["requiredScopeHashes"] = new Items(),
                r => ((Map)A(r, "descriptor", "resources", "requiredScopeHashes")[0])["scopeId"] = "fm.audio.runtime",
                r => Entry(r, Ids[0])["location"] = Entry(r, Ids[1])["location"]
            }) Mutate(edit);
            foreach (var path in new[] { "../a", "a//b", "/a", "a/", "a\\b", "a%2fb", "a:b", ".a", "a/..",
                "fm-resource-boot-v1.json", "x/player-boot-pin.sha256", "resource-build-receipt-v1.json" })
                Mutate(r => Entry(r, Ids[0])["location"] = path);
            var alias = Build();
            A(alias, "physicalFiles").Add(P("RAW/b1", 1, ZeroHash, "raw"));
            A(alias, "physicalFiles").Sort((a, b) => string.CompareOrdinal((string)((Map)a)["name"], (string)((Map)b)["name"]));
            Reject(alias, "RES_SCHEMA");
            Mutate(r => Entry(r, Ids[0])["location"] = ((string)Entry(r, Ids[1])["location"]).ToUpperInvariant());
            Mutate(r => Entry(r, "fm.ui.root")["files"] = L("raw/b1"), "RES_SCHEMA", true);
        }

        [Test]
        public void D10_BusinessAndTextSlotsStayDistinct()
        {
            foreach (var edit in new Action<Map>[]
            {
                r => O(r, "descriptor", "businessContent")["businessReleaseSetId"] = "fm.codec.r1",
                r => O(r, "descriptor")["releaseSetId"] = "release-set:fightmatch-demo-r1",
                r => ((Map)A(r, "descriptor", "businessContent", "files")[0])["name"] = Names[1],
                r => Entry(r, Ids[0])["scopeId"] = "fm.text.full",
                r => Entry(r, "fm.text.full.artifact")["scopeId"] = "fm.content.first-release",
                r => Entry(r, Ids[0])["assetId"] = "fm.content.extra",
                r => O(r, "descriptor", "text")["artifactName"] = Names[0],
                r => O(r, "descriptor", "text")["manifestName"] = "fm-text-v1.json",
                r => Entry(r, Ids[0])["contentLength"] = Lengths[0] + 1,
                r => O(r, "descriptor", "publication", "receipt")["operationId"] = "build:other",
                r => O(r, "descriptor", "publication", "receipt")["authorizationEvidenceId"] = "other",
                r => O(r, "descriptor", "publication", "receipt")["kind"] = "final-build-receipt"
            }) Mutate(edit);
            Mutate(r => Entry(r, "fm.ui.root")["scopeId"] = "fm.text.full", "RES_SCHEMA", true);
        }

        [Test]
        public void D11_SetPlatformKindAndOrderingAreExact()
        {
            foreach (var length in new[] { 1, 127, 128 })
            {
                var root = Build();
                var set = new string('a', length);
                O(root, "descriptor")["releaseSetId"] = set;
                foreach (Map entry in A(root, "mapping", "entries")) entry["releaseSetId"] = set;
                Rebind(root);
                Assert.AreEqual(set, Accept(Bytes(root)).ReleaseSetId);
            }
            foreach (var length in new[] { 239, 240, 241 })
            {
                var root = Build();
                Entry(root, Ids[0])["location"] = new string('a', length);
                Rebind(root);
                if (length <= 240) Accept(Bytes(root)); else Reject(root, "RES_SCHEMA");
            }
            foreach (var platform in new[] { "android", "macOS", "windows" })
            {
                var root = Build(true);
                O(root, "descriptor", "resources")["platform"] = platform;
                foreach (Map entry in A(root, "mapping", "entries")) entry["platform"] = platform;
                Rebind(root);
                Assert.AreEqual(platform, Accept(Bytes(root)).Platform);
            }
            foreach (var id in new[] { "", "latest", "Upper", "a/b", "a:b", new string('a', 129) })
                Mutate(r => O(r, "descriptor")["releaseSetId"] = id);
            foreach (var platform in new[] { "macos", "Android", "linux" })
                Mutate(r => O(r, "descriptor", "resources")["platform"] = platform);
            foreach (var edit in new Action<Map>[]
            {
                r => Entry(r, Ids[0])["releaseSetId"] = "another",
                r => Entry(r, Ids[0])["platform"] = "windows",
                r => Entry(r, Ids[0])["packageName"] = "Other",
                r => O(r, "descriptor", "resources")["yooAssetPackageVersion"] = "3.0.7",
                r => O(r, "descriptor", "resources")["yooManifestPackageVersion"] = "latest",
                r => Entry(r, Ids[0])["kind"] = "unknown",
                r => Entry(r, Ids[0])["unityType"] = "UnityEngine.TextAsset",
                r => O(r, "descriptor", "codeCompatibility")["requiredCapabilities"] = L("z", "a"),
                r => A(r, "mapping", "entries").Reverse(),
                r => A(r, "physicalFiles").Reverse(),
                r => A(r, "descriptor", "resources", "requiredScopeHashes").Reverse()
            }) Mutate(edit);
            foreach (var type in new[] { "", "Texture2D", "UnityEngine..Object", "UnityEngine.9Bad", new string('a', 129) })
                Mutate(r => Entry(r, "fm.ui.root")["unityType"] = type, "RES_SCHEMA", true);
            Mutate(r => Entry(r, "fm.ui.root")["files"] = L("bundles/ui", "bundles/ui"), "RES_SCHEMA", true);
        }

        [Test]
        public void D12_InputAndOutputCopiesAreIsolated()
        {
            var input = Utf8.GetBytes(Vector1);
            var value = Accept(input, Pin1);
            input[0] = 0;
            var one = value.EncodeCanonical();
            var two = value.EncodeCanonical();
            Assert.AreNotSame(one, two);
            CollectionAssert.AreEqual(Utf8.GetBytes(Vector1), one);
            one[0] = 0;
            Assert.AreEqual((byte)'{', two[0]);
            two[1] = 0;
            CollectionAssert.AreEqual(Utf8.GetBytes(Vector1), value.EncodeCanonical());
            Assert.AreEqual(Descriptor1, value.DescriptorSha256);
        }
    }
}
