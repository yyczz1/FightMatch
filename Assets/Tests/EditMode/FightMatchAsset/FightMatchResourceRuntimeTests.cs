using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FightMatch.YooAssetAdapter;
using NUnit.Framework;
using UnityEngine;
using Code = FightMatch.AssetAccess.FightMatchAssetDiagnosticCode;
using Stage = FightMatch.AssetAccess.FightMatchAssetDiagnosticStage;

namespace FightMatch.AssetAccess.Tests
{
    public sealed class FightMatchResourceRuntimeTests
    {
        private const string Vector0 = "{\"descriptor\":{\"businessContent\":{\"binding\":{\"ContentFingerprint\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"NumericContractVersion\":\"RC01\",\"PackageId\":\"package:fightmatch-demo-r1\",\"RandomContractVersion\":\"PC01+SC01\",\"RuleVersion\":\"demo-r1\"},\"businessReleaseSetId\":\"release-set:fightmatch-demo-r1\",\"contentReleaseSetSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[{\"length\":20443,\"name\":\"first-release.fmsource.json\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"length\":11486,\"name\":\"first-release.fmpackage.bytes\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"length\":664,\"name\":\"first-release.fmvalidation.bytes\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"length\":4766,\"name\":\"first-release.fmreview.json\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"length\":682,\"name\":\"first-release.fmpublish.json\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"length\":411,\"name\":\"first-release.fmrelease.json\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"}],\"publicationReceiptSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},\"codeCompatibility\":{\"appBuildIdentity\":\"codec-v1\",\"baseProtocolVersion\":1,\"requiredCapabilities\":[\"resource-schema-v1\"]},\"publication\":{\"authorizationEvidenceId\":\"local-test-only\",\"operationId\":\"build:codec:1\",\"receipt\":{\"authorizationEvidenceId\":\"local-test-only\",\"businessInputsSha256\":\"091fd6bd86a735f2030cec61f5cd6fd17fbc7ca9ffc87dc9f2cdd7c41e105be0\",\"kind\":\"resource-inputs-receipt-v1\",\"operationId\":\"build:codec:1\",\"schemaVersion\":1,\"sourcePlanSha256\":\"148de9c5a7a44d19e56cd9ae1a554bf67847afb0c58f6e12fa29ac7ddfca9940\",\"textSourceReceiptSha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},\"receiptSha256\":\"5d2d2a30236f965ba985ceb90ce9e62b2c21d8faaf9038005ec06d92bd9ee812\"},\"releaseSetId\":\"fm.codec.r1\",\"resources\":{\"manifestLength\":1,\"manifestName\":\"manifest/main.bytes\",\"manifestSha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\",\"mappingDescriptorSha256\":\"ef622bf08c4209e93bd527bb9938312279c39314f42d265e05ef788d25882fcd\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"requiredScopeHashes\":[{\"scopeId\":\"fm.content.first-release\",\"sha256\":\"dab086f2f4f73484bfcbfd9d2f1f3564af2bc303795c704680bcd4f968ea22ef\"},{\"scopeId\":\"fm.text.full\",\"sha256\":\"5a8840058b97e4a8b5ce3356b096232e1465b17d59ef4b260b80305dac592424\"}],\"yooAssetPackageVersion\":\"3.0.6\",\"yooManifestPackageVersion\":\"codec-v1\"},\"schemaVersion\":1,\"text\":{\"artifactLength\":1,\"artifactName\":\"fm-text-v1.json\",\"artifactSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"manifestLength\":1,\"manifestName\":\"fm-text-v1.manifest.json\",\"manifestSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"schemaVersion\":1,\"sourceReceipt\":{\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"}}},\"descriptorSha256\":\"87b31f3bf93bf7456a44ccfc7d42f747b22166b8731ddd2c446ada067d156f8a\",\"mapping\":{\"entries\":[{\"assetId\":\"fm.content.package\",\"contentLength\":11486,\"contentSha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"files\":[\"raw/b2\"],\"kind\":\"raw\",\"location\":\"first-release.fmpackage.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.publication\",\"contentLength\":682,\"contentSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\",\"files\":[\"raw/b5\"],\"kind\":\"raw\",\"location\":\"first-release.fmpublish.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.release-set\",\"contentLength\":411,\"contentSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[\"raw/b6\"],\"kind\":\"raw\",\"location\":\"first-release.fmrelease.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.review\",\"contentLength\":4766,\"contentSha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\",\"files\":[\"raw/b4\"],\"kind\":\"raw\",\"location\":\"first-release.fmreview.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.source\",\"contentLength\":20443,\"contentSha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\",\"files\":[\"raw/b1\"],\"kind\":\"raw\",\"location\":\"first-release.fmsource.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.validation\",\"contentLength\":664,\"contentSha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\",\"files\":[\"raw/b3\"],\"kind\":\"raw\",\"location\":\"first-release.fmvalidation.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.artifact\",\"contentLength\":1,\"contentSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"files\":[\"raw/text\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.manifest\",\"contentLength\":1,\"contentSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"files\":[\"raw/text-manifest\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.manifest.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"}],\"schemaVersion\":1},\"physicalFiles\":[{\"kind\":\"receipt\",\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},{\"kind\":\"manifest\",\"length\":1,\"name\":\"manifest/main.bytes\",\"sha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\"},{\"kind\":\"raw\",\"length\":20443,\"name\":\"raw/b1\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"kind\":\"raw\",\"length\":11486,\"name\":\"raw/b2\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"kind\":\"raw\",\"length\":664,\"name\":\"raw/b3\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"kind\":\"raw\",\"length\":4766,\"name\":\"raw/b4\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"kind\":\"raw\",\"length\":682,\"name\":\"raw/b5\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"kind\":\"raw\",\"length\":411,\"name\":\"raw/b6\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text\",\"sha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text-manifest\",\"sha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\"}],\"schemaVersion\":1}";
        private const string Vector1 = "{\"descriptor\":{\"businessContent\":{\"binding\":{\"ContentFingerprint\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"NumericContractVersion\":\"RC01\",\"PackageId\":\"package:fightmatch-demo-r1\",\"RandomContractVersion\":\"PC01+SC01\",\"RuleVersion\":\"demo-r1\"},\"businessReleaseSetId\":\"release-set:fightmatch-demo-r1\",\"contentReleaseSetSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[{\"length\":20443,\"name\":\"first-release.fmsource.json\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"length\":11486,\"name\":\"first-release.fmpackage.bytes\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"length\":664,\"name\":\"first-release.fmvalidation.bytes\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"length\":4766,\"name\":\"first-release.fmreview.json\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"length\":682,\"name\":\"first-release.fmpublish.json\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"length\":411,\"name\":\"first-release.fmrelease.json\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"}],\"publicationReceiptSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},\"codeCompatibility\":{\"appBuildIdentity\":\"codec-v1\",\"baseProtocolVersion\":1,\"requiredCapabilities\":[\"resource-schema-v1\"]},\"publication\":{\"authorizationEvidenceId\":\"local-test-only\",\"operationId\":\"build:codec:1\",\"receipt\":{\"authorizationEvidenceId\":\"local-test-only\",\"businessInputsSha256\":\"091fd6bd86a735f2030cec61f5cd6fd17fbc7ca9ffc87dc9f2cdd7c41e105be0\",\"kind\":\"resource-inputs-receipt-v1\",\"operationId\":\"build:codec:1\",\"schemaVersion\":1,\"sourcePlanSha256\":\"148de9c5a7a44d19e56cd9ae1a554bf67847afb0c58f6e12fa29ac7ddfca9940\",\"textSourceReceiptSha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},\"receiptSha256\":\"5d2d2a30236f965ba985ceb90ce9e62b2c21d8faaf9038005ec06d92bd9ee812\"},\"releaseSetId\":\"fm.codec.r1\",\"resources\":{\"manifestLength\":1,\"manifestName\":\"manifest/main.bytes\",\"manifestSha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\",\"mappingDescriptorSha256\":\"8bfba8d3420a26110e057021cfb62c29f159c56300890eae407846aacf9f4c6b\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"requiredScopeHashes\":[{\"scopeId\":\"fm.content.first-release\",\"sha256\":\"dab086f2f4f73484bfcbfd9d2f1f3564af2bc303795c704680bcd4f968ea22ef\"},{\"scopeId\":\"fm.text.full\",\"sha256\":\"5a8840058b97e4a8b5ce3356b096232e1465b17d59ef4b260b80305dac592424\"},{\"scopeId\":\"fm.ui.runtime\",\"sha256\":\"63dfba3b30cdaf0372e7fe132a79f60916359ae1b6263c0d63cfbe76282b5884\"}],\"yooAssetPackageVersion\":\"3.0.6\",\"yooManifestPackageVersion\":\"codec-v1\"},\"schemaVersion\":1,\"text\":{\"artifactLength\":1,\"artifactName\":\"fm-text-v1.json\",\"artifactSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"manifestLength\":1,\"manifestName\":\"fm-text-v1.manifest.json\",\"manifestSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"schemaVersion\":1,\"sourceReceipt\":{\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"}}},\"descriptorSha256\":\"f570efbe5ee762e035f957ad9be05f3521f94437e8a938ca8125df9524dc0936\",\"mapping\":{\"entries\":[{\"assetId\":\"fm.content.package\",\"contentLength\":11486,\"contentSha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\",\"files\":[\"raw/b2\"],\"kind\":\"raw\",\"location\":\"first-release.fmpackage.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.publication\",\"contentLength\":682,\"contentSha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\",\"files\":[\"raw/b5\"],\"kind\":\"raw\",\"location\":\"first-release.fmpublish.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.release-set\",\"contentLength\":411,\"contentSha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\",\"files\":[\"raw/b6\"],\"kind\":\"raw\",\"location\":\"first-release.fmrelease.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.review\",\"contentLength\":4766,\"contentSha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\",\"files\":[\"raw/b4\"],\"kind\":\"raw\",\"location\":\"first-release.fmreview.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.source\",\"contentLength\":20443,\"contentSha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\",\"files\":[\"raw/b1\"],\"kind\":\"raw\",\"location\":\"first-release.fmsource.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.content.validation\",\"contentLength\":664,\"contentSha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\",\"files\":[\"raw/b3\"],\"kind\":\"raw\",\"location\":\"first-release.fmvalidation.bytes\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.content.first-release\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.artifact\",\"contentLength\":1,\"contentSha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\",\"files\":[\"raw/text\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.text.full.manifest\",\"contentLength\":1,\"contentSha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\",\"files\":[\"raw/text-manifest\"],\"kind\":\"raw\",\"location\":\"fm-text-v1.manifest.json\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.text.full\",\"unityType\":\"\"},{\"assetId\":\"fm.ui.root\",\"contentLength\":1,\"contentSha256\":\"e2fd09070ebe49d531e84a0d865741edb28afe10363124dd5476aa678a7705f8\",\"files\":[\"bundles/ui\"],\"kind\":\"object\",\"location\":\"ui/root\",\"packageName\":\"FightMatchMain\",\"platform\":\"android\",\"releaseSetId\":\"fm.codec.r1\",\"scopeId\":\"fm.ui.runtime\",\"unityType\":\"UnityEngine.GameObject\"}],\"schemaVersion\":1},\"physicalFiles\":[{\"kind\":\"bundle\",\"length\":1,\"name\":\"bundles/ui\",\"sha256\":\"3e23e8160039594a33894f6564e1b1348bbd7a0088d42c4acb73eeaed59c009d\"},{\"kind\":\"receipt\",\"length\":1,\"name\":\"inputs/text.receipt\",\"sha256\":\"594e519ae499312b29433b7dd8a97ff068defcba9755b6d5d00e84c524d67b06\"},{\"kind\":\"manifest\",\"length\":1,\"name\":\"manifest/main.bytes\",\"sha256\":\"62c66a7a5dd70c3146618063c344e531e6d4b59e379808443ce962b3abd63c5a\"},{\"kind\":\"raw\",\"length\":20443,\"name\":\"raw/b1\",\"sha256\":\"fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a\"},{\"kind\":\"raw\",\"length\":11486,\"name\":\"raw/b2\",\"sha256\":\"b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129\"},{\"kind\":\"raw\",\"length\":664,\"name\":\"raw/b3\",\"sha256\":\"512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d\"},{\"kind\":\"raw\",\"length\":4766,\"name\":\"raw/b4\",\"sha256\":\"16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75\"},{\"kind\":\"raw\",\"length\":682,\"name\":\"raw/b5\",\"sha256\":\"feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910\"},{\"kind\":\"raw\",\"length\":411,\"name\":\"raw/b6\",\"sha256\":\"03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text\",\"sha256\":\"2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881\"},{\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text-manifest\",\"sha256\":\"a1fce4363854ff888cff4b8e7875d600c2682390412a8cf79b37d0b11148b0fa\"}],\"schemaVersion\":1}";
        private const int Large = 16777216, Chunked = 131073;
        private const string Set = "fm.codec.r1", Root = "/test-owned/builtin";
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        private static byte[] Boot(int length = 1)
        {
            if (length == 1) return Utf8.GetBytes(Vector0);
            Assert.IsTrue(length == Large || length == Chunked);
            var old = new[] { "2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881", "5a8840058b97e4a8b5ce3356b096232e1465b17d59ef4b260b80305dac592424", "ef622bf08c4209e93bd527bb9938312279c39314f42d265e05ef788d25882fcd", "87b31f3bf93bf7456a44ccfc7d42f747b22166b8731ddd2c446ada067d156f8a" };
            var hashes = length == Large ? new[] { "a06c26cbac8b80704f420222dae5658b88ff2da96702d12ef7a4223e9361f7c1", "8d6d85e610081c3f75b936bf47e8b1a3ce7930ce3a1efd0feea64bd227469a07", "815ba55ea827dfecb2a69ccd25e37fb5ccd4d1f8d2dce272c0eb14dd64e2f4b8", "ce968f24af9e28b1ec7fb2b8a7e7aaa9fe07b28f8f2ce766f38531fd008caf7e" } :
                new[] { "0c5c5c759aa8164f9fb53c471ff060903c99edcb520fb7d9cb4bf7a45755f1c2", "883f3692f75223d8029ecb6cddb375db9d31885f034ba6f97bdff6300edbd884", "100e47fddb3aa137430152ccc275e6a4c2a27f66856673bce54a800cd67800a1", "a93fd6e8ba822fea25975621e251fa673a4b8e9e2b894b41bd6abf070d364e4f" };
            var text = Vector0.Replace("\"artifactLength\":1,", "\"artifactLength\":" + length + ",")
                .Replace("\"contentLength\":1,\"contentSha256\":\"" + old[0] + "\"",
                    "\"contentLength\":" + length + ",\"contentSha256\":\"" + old[0] + "\"")
                .Replace("\"kind\":\"raw\",\"length\":1,\"name\":\"raw/text\"",
                    "\"kind\":\"raw\",\"length\":" + length + ",\"name\":\"raw/text\"");
            for (var index = 0; index < old.Length; index++) text = text.Replace(old[index], hashes[index]);
            return Utf8.GetBytes(text);
        }
        private static FightMatchAssetId Id(string value = "fm.text.full.artifact")
        {
            Assert.IsTrue(FightMatchAssetId.TryCreate(value, out var id));
            return id;
        }
        private static FightMatchResourceReleaseSet Decode(byte[] bytes)
        {
            Assert.IsTrue(FightMatchResourceReleaseSet.TryDecodePinned(bytes, Hash(bytes), out var value, out var code), code);
            return value;
        }
        private static AssetMapping Map(int length = 1)
        {
            Assert.IsTrue(AssetMapping.TryFromPinnedRaw(Decode(Boot(length)), Id(), Set, "android", Root, out var map));
            return map;
        }
        private static AssetMapping UnityMap() => new AssetMapping("fm.ui.root", Set, "FightMatchMain", "codec-v1",
            "ui/root", typeof(Texture2D), Root);
        private static AssetAcquireBudget Budget(long raw = Large, int pending = 32, int callbacks = 1024) =>
            new AssetAcquireBudget(raw, pending, callbacks, 4096);
        private static void Reject<T>(Task<FightMatchAssetAcquireResult<T>> task, Code code, Stage stage, bool retry = false) where T : class
        {
            Assert.IsTrue(task.IsCompleted);
            var result = task.Result;
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Lease);
            Assert.AreEqual(code, result.Diagnostic.Code); Assert.AreEqual(stage, result.Diagnostic.Stage);
            Assert.AreEqual(retry, result.Diagnostic.Retryable);
            Assert.AreEqual("status=failed", result.Diagnostic.SafeDetail);
        }
        private static void Background(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() => { try { action(); } catch (Exception error) { failure = error; } });
            thread.Start();
            Assert.IsTrue(thread.Join(5000));
            Assert.IsNull(failure);
        }

        private sealed class Dispatcher : IAssetDispatcher
        {
            private readonly int main = Thread.CurrentThread.ManagedThreadId;
            private readonly Queue<Action> queue = new Queue<Action>();
            internal bool FailPost;
            internal int Posts, Maximum;
            public bool IsMain => Thread.CurrentThread.ManagedThreadId == main;
            public void Post(Action action)
            {
                lock (queue)
                {
                    Posts++;
                    if (FailPost) throw new IOException("secret post failure");
                    queue.Enqueue(action); Maximum = Math.Max(Maximum, queue.Count);
                }
            }
            internal bool Step()
            {
                Action action;
                lock (queue) { if (queue.Count == 0) return false; action = queue.Dequeue(); }
                action(); return true;
            }
            internal void Run()
            {
                for (var count = 0; count < 4096; count++) if (!Step()) return;
                Assert.Fail("Bounded dispatch did not drain.");
            }
        }
        private sealed class Body : Stream
        {
            internal long Declared, Actual;
            internal int MaxChunk = 65536, Reads, Maximum, Closes;
            internal byte Value = (byte)'x';
            internal bool Unknown, ThrowRead, ChangeLength, ThrowClose;
            private long position;
            internal Body(long length) { Declared = Actual = length; }
            public override bool CanRead => true;
            public override bool CanSeek => !Unknown;
            public override bool CanWrite => false;
            public override long Length => ChangeLength && position >= Actual ? Declared + 1 : Declared;
            public override long Position { get => position; set => position = value; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                Reads++; Maximum = Math.Max(Maximum, count);
                if (ThrowRead) throw new IOException("/Users/private?token=secret");
                var amount = (int)Math.Min(Math.Min(count, MaxChunk), Math.Max(0, Actual - position));
                for (var index = 0; index < amount; index++) buffer[offset + index] = Value;
                position += amount; return amount;
            }
            public override long Seek(long offset, SeekOrigin origin) => position = offset;
            public override void Flush() { }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
            protected override void Dispose(bool disposing)
            {
                if (ThrowClose) { ThrowClose = false; throw new IOException("secret close failure"); }
                Closes++; base.Dispose(disposing);
            }
        }
        private class Signal : IYooOperation
        {
            private Action<IYooOperation> callbacks;
            internal Action<IYooOperation> Captured;
            internal bool ThrowSubscribe, ThrowRelease;
            internal int Releases, ReleaseThread;
            internal UnityEngine.Object Value;
            public bool Done { get; protected set; }
            public bool Success { get; protected set; }
            public UnityEngine.Object Asset => Value;
            internal Signal(bool done = true, bool success = true) { Done = done; Success = success; }
            public event Action<IYooOperation> Completed
            {
                add { Captured = value; if (ThrowSubscribe) throw new IOException("secret subscribe"); callbacks += value; if (Done) value(this); }
                remove { callbacks -= value; }
            }
            internal void Finish(bool success)
            {
                Done = true; Success = success; callbacks?.Invoke(this);
            }
            internal void Emit(IYooOperation sender = null) { Captured?.Invoke(sender ?? this); }
            public virtual void Release()
            {
                Assert.IsTrue(Done); Assert.IsNull(callbacks);
                if (ThrowRelease) throw new IOException("secret release");
                Releases++; ReleaseThread = Thread.CurrentThread.ManagedThreadId;
            }
        }
        private sealed class RawOperation : Signal, IRawYooOperation
        {
            private readonly FakeSdk sdk;
            private readonly AssetMapping map;
            private readonly long limit;
            internal bool EnsureDone;
            internal RawReadState Reader;
            internal Body Body;
            internal int Transfers;
            public Code? Error { get; private set; }
            public object Payload
            {
                get { var result = Reader?.Payload; if (result != null) Transfers++; return result; }
            }
            internal RawOperation(FakeSdk sdk, AssetMapping map, long limit) : base(false)
            { this.sdk = sdk; this.map = map; this.limit = limit; EnsureDone = sdk.AutoEnsure; }
            public void Pump(bool stopRequested, Func<bool> continueRaw)
            {
                if (Done || !EnsureDone) return;
                if (stopRequested) { Reader?.Pump(true); Error = Code.SdkFailure; Finish(false); return; }
                if (sdk.FailEnsure) { Finish(false); return; }
                if (!RealYooSdk.IsPlainRawBundle(sdk.Kind, sdk.Encrypted)) { Error = Code.WrongAssetType; Finish(false); return; }
                if (Reader == null)
                {
                    Body = new Body(map.RawBytes.Value);
                    sdk.EditBody?.Invoke(Body);
                    Reader = new RawReadState(Body, map.RawBytes.Value, sdk.PayloadSha, limit, length =>
                    {
                        sdk.Allocations++;
                        if (sdk.ThrowAllocate) throw new IOException("secret allocation");
                        return new byte[length];
                    });
                }
                Reader.PumpNext(continueRaw);
                Error = Reader.Error;
                if (Reader.Done) Finish(!Error.HasValue);
            }
            public override void Release() { Reader?.Dispose(); base.Release(); }
        }
        private sealed class FakeSdk : IYooSdk, IRawYooSdk, IDisposable
        {
            private object package;
            private bool ready, empty = true;
            private string version;
            private Texture2D texture;
            internal bool AutoEnsure = true, ThrowBegin, ThrowAllocate, FailEnsure, FailInit, FailManifest, ThrowSubscribe;
            internal bool Encrypted, Unsafe;
            internal int Kind = 3, Begins, Allocations, Loads, Creates;
            internal string PayloadSha = "2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881";
            internal Action<Body> EditBody;
            internal readonly List<RawOperation> Raw = new List<RawOperation>();
            public bool Initialized { get; private set; }
            public int PackageCount => package == null ? 0 : 1;
            public void Initialize() { Initialized = true; }
            public void Destroy() { Assert.IsNull(package); Initialized = false; }
            public object Find(string name) => package;
            public object Create(string name) { Creates++; return package = new object(); }
            public bool Ready(object value) => ready;
            public bool Busy(object value) => false;
            public bool Empty(object value) => empty;
            public string Version(object value) => version;
            public IYooOperation Initialize(object value, string root)
            { ready = !FailInit; empty = FailInit; return new Signal(true, !FailInit); }
            public IYooOperation Manifest(object value, string selected)
            { version = selected; return new Signal(true, !FailManifest); }
            public bool HasLocation(object value, string location, Type type) => true;
            public IYooOperation Load(object value, string location, Type type)
            { Loads++; texture = texture ?? new Texture2D(1, 1); return new Signal { Value = texture }; }
            public IRawYooOperation BeginRaw(object value, AssetMapping map, long limit, Action wake)
            {
                if (Unsafe) throw new YooAssetPackageLifecycle.Failure(Code.PackageUnavailable, Stage.ValidateResult);
                Begins++;
                if (ThrowBegin) throw new IOException("https://private?token=secret");
                var operation = new RawOperation(this, map, limit) { ThrowSubscribe = ThrowSubscribe };
                Raw.Add(operation); return operation;
            }
            public IYooOperation Destroy(object value) { ready = false; empty = true; return new Signal(); }
            public void Remove(string name) { package = null; }
            internal void External()
            { Initialized = true; package = new object(); ready = true; empty = false; version = "codec-v1"; }
            public void Dispose() { if (texture != null) UnityEngine.Object.DestroyImmediate(texture); }
        }
        private sealed class Rig : IDisposable
        {
            internal readonly Dispatcher Dispatch = new Dispatcher();
            internal readonly FakeSdk Sdk;
            internal readonly YooAssetAssetProvider Provider;
            internal readonly List<Task<FightMatchAssetAcquireResult<FightMatchRawBytes>>> Tasks =
                new List<Task<FightMatchAssetAcquireResult<FightMatchRawBytes>>>();
            internal Rig(int length = 1, bool ensure = true, FakeSdk sdk = null, bool unity = false)
            {
                Sdk = sdk ?? new FakeSdk(); Sdk.AutoEnsure = ensure;
                Sdk.PayloadSha = length == Large ? "a06c26cbac8b80704f420222dae5658b88ff2da96702d12ef7a4223e9361f7c1" :
                    length == Chunked ? "0c5c5c759aa8164f9fb53c471ff060903c99edcb520fb7d9cb4bf7a45755f1c2" : "2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881";
                Provider = new YooAssetAssetProvider(unity ? new[] { Map(length), UnityMap() } : new[] { Map(length) }, Dispatch, Sdk);
            }
            internal Task<FightMatchAssetAcquireResult<FightMatchRawBytes>> Request(AssetAcquireBudget budget = null, long epoch = 1)
            {
                var task = Provider.AcquireAsync<FightMatchRawBytes>(Id(), Set, budget ?? Budget(), epoch);
                Tasks.Add(task); return task;
            }
            internal IFightMatchAssetLease<FightMatchRawBytes> Lease(Task<FightMatchAssetAcquireResult<FightMatchRawBytes>> task)
            {
                Dispatch.Run(); Assert.IsTrue(task.IsCompleted); Assert.IsTrue(task.Result.IsAccepted, task.Result.Diagnostic?.SafeDetail);
                return task.Result.Lease;
            }
            public void Dispose()
            {
                Dispatch.FailPost = false;
                foreach (var task in Tasks) if (task.IsCompleted && task.Result.IsAccepted) task.Result.Lease.Dispose();
                foreach (var operation in Sdk.Raw) { operation.ThrowRelease = false; operation.EnsureDone = true; operation.Emit(); }
                var close = Provider.CloseAsync(); Dispatch.Run(); Assert.IsTrue(close.IsCompleted);
                Sdk.Dispose();
            }
        }
        private static void CapacityRestored()
        {
            var holders = new List<Rig>();
            try
            {
                for (var index = 0; index < 4; index++)
                {
                    var rig = new Rig(Large, false); holders.Add(rig);
                    Assert.IsFalse(rig.Request().IsCompleted); Assert.AreEqual(1, rig.Sdk.Begins);
                }
                using (var extra = new Rig(Large, false))
                    Reject(extra.Request(), Code.BudgetExceeded, Stage.ValidateRequest);
            }
            finally { foreach (var holder in holders) holder.Dispose(); }
        }

        [Test]
        public void RAW01_PinnedRawMappingOnly()
        {
            var boot = Boot();
            var source = Decode(boot);
            Assert.IsFalse(AssetMapping.TryFromPinnedRaw(source, Id(), "wrong", "android", Root, out var wrong));
            Assert.IsNull(wrong);
            Assert.IsFalse(AssetMapping.TryFromPinnedRaw(source, Id(), Set, "ios", Root, out wrong));
            Assert.IsFalse(AssetMapping.TryFromPinnedRaw(Decode(Utf8.GetBytes(Vector1)), Id("fm.ui.root"), Set, "android", Root, out wrong));
            Assert.IsTrue(AssetMapping.TryFromPinnedRaw(source, Id(), Set, "android", Root, out var map));
            boot[0] = 0;
            Assert.AreEqual(1, map.RawBytes);
            using (var rig = new Rig())
            {
                var lease = rig.Lease(rig.Request());
                using (var stream = lease.Asset.OpenRead()) Assert.AreEqual((byte)'x', stream.ReadByte());
            }
            using (var sdk = new FakeSdk())
            {
                var untrusted = new AssetMapping(Id().Value, Set, "FightMatchMain", "codec-v1", "raw", typeof(FightMatchRawBytes), Root, true, 1);
                var provider = new YooAssetAssetProvider(new[] { untrusted }, new Dispatcher(), sdk);
                Reject(provider.AcquireAsync<FightMatchRawBytes>(Id(), Set, Budget(), 1), Code.WrongAssetType, Stage.ValidateRequest);
                Assert.AreEqual(0, sdk.Begins);
                Assert.IsTrue(provider.CloseAsync().IsCompleted);
            }
        }

        [Test]
        public void RAW02_LengthAndBudgetBeforeAllocation()
        {
            foreach (var body in new[] { new Body(1) { Unknown = true }, new Body(2), new Body(Large + 1) })
            {
                var allocations = 0;
                using (var state = new RawReadState(body, 1, Hash(new[] { (byte)'x' }), Large, n => { allocations++; return new byte[n]; }))
                { Assert.IsTrue(state.Done); Assert.IsNotNull(state.Error); Assert.IsNull(state.Payload); Assert.AreEqual(0, allocations); }
            }
            var boundaryAllocations = 0;
            using (var state = new RawReadState(new Body(1), 1, Hash(new[] { (byte)'x' }), 1, n => { boundaryAllocations++; return new byte[n]; }))
            { state.Pump(); state.Pump(); Assert.IsTrue(state.Done); Assert.IsNull(state.Error); Assert.AreEqual(1, boundaryAllocations); }
            var holders = new List<Rig>();
            try
            {
                for (var index = 0; index < 4; index++)
                { var holder = new Rig(Large, false); holders.Add(holder); Assert.IsFalse(holder.Request().IsCompleted); }
                using (var probe = new Rig(Large, false))
                {
                    Reject(probe.Request(), Code.BudgetExceeded, Stage.ValidateRequest);
                    var first = holders[0];
                    var operation = first.Sdk.Raw[0]; operation.EnsureDone = true; operation.Emit();
                    var lease = first.Lease(first.Tasks[0]);
                    Assert.AreEqual(1, first.Sdk.Allocations);
                    Reject(first.Request(Budget(Large - 1)), Code.BudgetExceeded, Stage.ValidateRequest);
                    Reject(probe.Request(), Code.BudgetExceeded, Stage.ValidateRequest);
                    operation.ThrowRelease = true; lease.Dispose(); lease.Dispose(); first.Dispatch.Run();
                    Reject(probe.Request(), Code.BudgetExceeded, Stage.ValidateRequest);
                    Assert.IsFalse(first.Provider.CloseAsync().IsCompleted);
                    operation.ThrowRelease = false;
                    Assert.IsTrue(first.Provider.CloseAsync().IsCompleted);
                    Assert.AreEqual(1, operation.Releases);
                    Assert.IsFalse(probe.Request().IsCompleted);
                }
            }
            finally { foreach (var holder in holders) holder.Dispose(); }
            foreach (var fault in new[] { "begin", "allocate", "hash", "io" })
            {
                using (var rig = new Rig())
                {
                    rig.Sdk.ThrowBegin = fault == "begin"; rig.Sdk.ThrowAllocate = fault == "allocate";
                    rig.Sdk.EditBody = b => { b.Value = fault == "hash" ? (byte)'z' : (byte)'x'; b.ThrowRead = fault == "io"; };
                    var task = rig.Request(); rig.Dispatch.Run();
                    Reject(task, Code.SdkFailure, fault == "begin" ? Stage.Acquire : Stage.ValidateResult, fault == "begin");
                }
                CapacityRestored();
            }
        }

        [Test]
        public void RAW03_ExactBodyAndHashBeforePublication()
        {
            using (var rig = new Rig(Chunked))
            {
                rig.Sdk.EditBody = b => b.MaxChunk = 20000;
                var task = rig.Request(); Assert.IsFalse(task.IsCompleted);
                var lease = rig.Lease(task);
                Assert.AreEqual(1, rig.Sdk.Allocations);
                Assert.LessOrEqual(rig.Sdk.Raw[0].Body.Maximum, 65536);
                using (var stream = lease.Asset.OpenRead())
                {
                    var bytes = new byte[1024]; long count = 0; int read;
                    while ((read = stream.Read(bytes, 0, bytes.Length)) != 0)
                    { count += read; for (var index = 0; index < read; index++) Assert.AreEqual((byte)'x', bytes[index]); }
                    Assert.AreEqual(Chunked, count);
                }
                Assert.AreEqual(1, rig.Sdk.Raw[0].Transfers); Assert.IsNull(rig.Sdk.Raw[0].Payload);
            }
            foreach (var fault in new[] { "eof", "tail", "length", "hash", "io", "close" })
                using (var rig = new Rig())
                {
                    rig.Sdk.EditBody = b =>
                    { b.Actual = fault == "eof" ? 0 : fault == "tail" ? 2 : 1; b.ChangeLength = fault == "length";
                      b.Value = fault == "hash" ? (byte)'z' : (byte)'x'; b.ThrowRead = fault == "io"; b.ThrowClose = fault == "close"; };
                    var task = rig.Request(); rig.Dispatch.Run();
                    Reject(task, Code.SdkFailure, Stage.ValidateResult);
                    Assert.AreEqual(0, rig.Sdk.Raw[0].Transfers);
                }
        }

        [Test]
        public void RAW04_ReadOnlyBoundedStream()
        {
            using (var rig = new Rig())
            {
                var view = rig.Lease(rig.Request()).Asset;
                var stream = view.OpenRead();
                Assert.IsFalse(stream is MemoryStream); Assert.IsFalse(stream.CanWrite); Assert.IsTrue(stream.CanSeek);
                Assert.Throws<InvalidOperationException>(() => view.OpenRead());
                Assert.AreEqual(1, stream.Length); Assert.AreEqual((byte)'x', stream.ReadByte()); Assert.AreEqual(-1, stream.ReadByte());
                Assert.AreEqual(0, stream.Seek(-1, SeekOrigin.End)); Assert.AreEqual(0, stream.Position);
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-1, SeekOrigin.Begin));
                Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(long.MaxValue, SeekOrigin.Current));
                Assert.Throws<NotSupportedException>(() => stream.Write(new byte[1], 0, 1));
                Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
                stream.Dispose();
                using (var reopened = view.OpenRead()) Assert.AreEqual(0, reopened.Position);
            }
        }

        [Test]
        public void RAW05_TwoLeasesShareOneVerifiedPayload()
        {
            using (var rig = new Rig())
            {
                var firstTask = rig.Request(); var secondTask = rig.Request();
                var first = rig.Lease(firstTask); var second = rig.Lease(secondTask);
                var a = first.Asset; var b = second.Asset;
                Assert.AreNotSame(a, b); Assert.AreEqual(1, rig.Sdk.Begins); Assert.AreEqual(1, rig.Sdk.Allocations);
                var staleStream = a.OpenRead(); using (var live = b.OpenRead())
                {
                    Assert.AreEqual((byte)'x', staleStream.ReadByte()); Assert.AreEqual(0, live.Position);
                    first.Dispose(); rig.Dispatch.Run();
                    Assert.Throws<ObjectDisposedException>(() => a.OpenRead());
                    Assert.Throws<ObjectDisposedException>(() => staleStream.ReadByte());
                    Assert.Throws<ObjectDisposedException>(() => staleStream.Seek(0, SeekOrigin.Begin));
                    Assert.AreEqual((byte)'x', live.ReadByte()); Assert.AreEqual(0, rig.Sdk.Raw[0].Releases);
                }
                second.Dispose(); rig.Dispatch.Run();
                Assert.AreEqual(1, rig.Sdk.Raw[0].Releases);
                Assert.Throws<ObjectDisposedException>(() => { var ignored = staleStream.Position; });
            }
            CapacityRestored();
        }

        [Test]
        public void RAW06_StreamAndLeaseHaveSeparateOwners()
        {
            using (var rig = new Rig())
            {
                var lease = rig.Lease(rig.Request()); var view = lease.Asset;
                view.OpenRead().Dispose(); Assert.IsFalse(lease.IsReleased); Assert.AreEqual(0, rig.Sdk.Raw[0].Releases);
                var stream = view.OpenRead();
                Background(() => { lease.Dispose(); lease.Dispose(); });
                Assert.AreEqual(1, view.Length); Assert.AreEqual(0, rig.Sdk.Raw[0].Releases);
                Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
                Assert.Throws<ObjectDisposedException>(() => view.OpenRead());
                rig.Dispatch.Run();
                Assert.AreEqual(1, rig.Sdk.Raw[0].Releases);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, rig.Sdk.Raw[0].ReleaseThread);
            }
        }

        [Test]
        public void RAW07_StalePendingCannotPublish()
        {
            using (var rig = new Rig(ensure: false))
            {
                var old = rig.Request(); var oldOperation = rig.Sdk.Raw[0];
                rig.Provider.AdvanceEpoch(2); var fresh = rig.Request(epoch: 2); var freshOperation = rig.Sdk.Raw[1];
                freshOperation.EnsureDone = true; freshOperation.Emit();
                var lease = rig.Lease(fresh); Assert.IsFalse(old.IsCompleted);
                var posts = rig.Dispatch.Posts; oldOperation.Emit(freshOperation); Assert.AreEqual(posts, rig.Dispatch.Posts);
                oldOperation.EnsureDone = true; oldOperation.Emit(); rig.Dispatch.Run();
                Reject(old, Code.StaleEpoch, Stage.ValidateResult);
                Assert.IsNull(oldOperation.Reader); Assert.AreEqual(1, oldOperation.Releases);
                oldOperation.Emit(); oldOperation.Emit(); rig.Dispatch.Run(); Assert.AreEqual(1, oldOperation.Releases);
                rig.Provider.AdvanceEpoch(3);
                using (var stream = lease.Asset.OpenRead()) Assert.AreEqual((byte)'x', stream.ReadByte());
            }
            CapacityRestored();
        }

        [Test]
        public void RAW08_CloseWaitsForEnsureAndLease()
        {
            using (var rig = new Rig(ensure: false))
            {
                var task = rig.Request(); var close = rig.Provider.CloseAsync();
                Assert.IsFalse(close.IsCompleted); Assert.IsFalse(task.IsCompleted); Assert.AreEqual(0, rig.Sdk.Raw[0].Releases);
                rig.Sdk.Raw[0].EnsureDone = true; rig.Sdk.Raw[0].Emit(); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted); Assert.AreEqual(0, rig.Sdk.Allocations); Assert.AreEqual(1, rig.Sdk.Raw[0].Releases);
                Assert.AreSame(close, rig.Provider.CloseAsync());
            }
            using (var rig = new Rig(Chunked))
            {
                var task = rig.Request(); var body = rig.Sdk.Raw[0].Body; var reads = body.Reads;
                var close = rig.Provider.CloseAsync(); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted); Assert.AreEqual(reads, body.Reads);
                Reject(task, Code.StaleEpoch, Stage.ValidateResult); Assert.AreEqual(0, rig.Sdk.Raw[0].Transfers);
            }
            using (var rig = new Rig())
            {
                var lease = rig.Lease(rig.Request()); var close = rig.Provider.CloseAsync();
                Assert.IsFalse(close.IsCompleted); lease.Dispose(); rig.Dispatch.Run(); Assert.IsTrue(close.IsCompleted);
            }
            CapacityRestored();
        }

        [Test]
        public void RAW09_CallbackBudgetAndThreadRulesRemain()
        {
            using (var rig = new Rig(ensure: false, unity: true))
            {
                var first = rig.Request();
                Reject(rig.Request(Budget(pending: 1)), Code.BudgetExceeded, Stage.ValidateRequest);
                var second = rig.Request();
                Reject(rig.Provider.AcquireAsync<Texture2D>(Id("fm.ui.root"), Set, Budget(callbacks: 2), 1), Code.BudgetExceeded, Stage.ValidateRequest);
                var unity = rig.Provider.AcquireAsync<Texture2D>(Id("fm.ui.root"), Set, Budget(callbacks: 3), 1);
                Assert.IsTrue(unity.Result.IsAccepted); unity.Result.Lease.Dispose();
                rig.Dispatch.Run(); var operation = rig.Sdk.Raw[0]; var posts = rig.Dispatch.Posts;
                for (var repeat = 0; repeat < 20; repeat++) operation.Emit();
                Assert.AreEqual(posts + 1, rig.Dispatch.Posts); rig.Dispatch.Run();
                rig.Dispatch.FailPost = true; operation.EnsureDone = true; operation.Emit();
                var third = rig.Request(); rig.Dispatch.FailPost = false;
                rig.Lease(first); rig.Lease(second); rig.Lease(third);
                Assert.LessOrEqual(rig.Dispatch.Maximum, 1); Assert.AreEqual(1, rig.Sdk.Begins);
                var before = operation.Releases;
                Background(() => first.Result.Lease.Dispose());
                Assert.AreEqual(before, operation.Releases);
            }
            using (var rig = new Rig())
            {
                rig.Sdk.ThrowSubscribe = true;
                var task = rig.Request(); Assert.IsFalse(task.IsCompleted);
                var close = rig.Provider.CloseAsync(); rig.Dispatch.Run();
                Assert.IsTrue(close.IsCompleted); Reject(task, Code.StaleEpoch, Stage.ValidateResult);
            }
            CapacityRestored();
        }

        [Test]
        public void RAW10_UnsafePackageAndBundleKindAreRejected()
        {
            var borrowed = new FakeSdk(); borrowed.External();
            using (var rig = new Rig(sdk: borrowed, unity: true))
            {
                Reject(rig.Request(), Code.PackageUnavailable, Stage.ValidateResult);
                Assert.AreEqual(0, borrowed.Begins); Assert.AreEqual(0, borrowed.Creates);
                var unity = rig.Provider.AcquireAsync<Texture2D>(Id("fm.ui.root"), Set, Budget(), 1);
                Assert.IsTrue(unity.Result.IsAccepted); unity.Result.Lease.Dispose();
            }
            var unsafeSdk = new FakeSdk { Unsafe = true };
            using (var rig = new Rig(sdk: unsafeSdk))
            { Reject(rig.Request(), Code.PackageUnavailable, Stage.ValidateResult); Assert.AreEqual(0, unsafeSdk.Begins); }
            var failure = Assert.Throws<YooAssetPackageLifecycle.Failure>(() => RealYooSdk.Instance.BeginRaw(new object(), Map(), 1, () => { }));
            Assert.AreEqual(Code.PackageUnavailable, failure.Code); Assert.AreEqual(Stage.ValidateResult, failure.Stage);
            foreach (var kind in new[] { 2, 4, 13, 3 })
                using (var rig = new Rig())
                {
                    rig.Sdk.Kind = kind; rig.Sdk.Encrypted = kind == 3;
                    var task = rig.Request(); rig.Dispatch.Run();
                    Reject(task, Code.WrongAssetType, Stage.ValidateResult); Assert.AreEqual(0, rig.Sdk.Allocations);
                }
        }

        [Test]
        public void RAW11_FailureDiagnosticsAreSafe()
        {
            foreach (var phase in new[] { "init", "manifest", "ensure", "hash" })
                using (var rig = new Rig())
                {
                    rig.Sdk.FailInit = phase == "init"; rig.Sdk.FailManifest = phase == "manifest"; rig.Sdk.FailEnsure = phase == "ensure";
                    rig.Sdk.EditBody = b => b.Value = (byte)'!';
                    var task = rig.Request(); rig.Dispatch.Run();
                    Reject(task, phase == "init" ? Code.PackageUnavailable : phase == "manifest" ? Code.ManifestUnavailable : Code.SdkFailure,
                        phase == "init" ? Stage.InitializePackage : phase == "manifest" ? Stage.SelectManifest :
                        phase == "ensure" ? Stage.Acquire : Stage.ValidateResult, phase != "hash");
                    var begins = rig.Sdk.Begins;
                    foreach (var operation in rig.Sdk.Raw) operation.Emit();
                    rig.Dispatch.Run(); Assert.AreEqual(begins, rig.Sdk.Begins);
                }
        }

        [Test]
        public void RAW12_RawInterfaceAndRegressionSurfaceStayClosed()
        {
            var type = typeof(FightMatchRawBytes);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            Assert.IsTrue(type.IsSealed); Assert.IsEmpty(type.GetConstructors()); Assert.IsEmpty(type.GetFields(flags));
            CollectionAssert.AreEqual(new[] { "Length" }, type.GetProperties(flags).Select(p => p.Name));
            Assert.IsNull(type.GetProperty("Length").GetSetMethod(true));
            var methods = type.GetMethods(flags).Where(m => !m.IsSpecialName).ToArray();
            Assert.AreEqual(1, methods.Length); Assert.AreEqual("OpenRead", methods[0].Name); Assert.AreEqual(typeof(Stream), methods[0].ReturnType);
            Assert.IsFalse(type.Assembly.GetReferencedAssemblies().Any(a => a.Name.StartsWith("UnityEngine") || a.Name.Contains("YooAsset")));
            var friends = type.Assembly.GetCustomAttributes<System.Runtime.CompilerServices.InternalsVisibleToAttribute>().Select(a => a.AssemblyName);
            CollectionAssert.AreEqual(new[] { "FightMatch.YooAssetAdapter" }, friends);
            CollectionAssert.AreEquivalent(new[] { "SchemaVersion", "ReleaseSetId", "BusinessReleaseSetId", "Platform", "DescriptorSha256" },
                typeof(FightMatchResourceReleaseSet).GetProperties(flags).Select(p => p.Name));
            CollectionAssert.AreEqual(Boot(), Decode(Boot()).EncodeCanonical());
        }

        [Test]
        public void RAW13_FailedChunkSchedulingTerminatesWithoutExternalDrain()
        {
            var ledger = typeof(YooAssetAssetProvider).GetField("globalRawBytes", BindingFlags.NonPublic | BindingFlags.Static);
            var reservedBefore = (long)ledger.GetValue(null);
            RawOperation operation;
            Body body;
            using (var rig = new Rig(Chunked, ensure: false))
            {
                var first = rig.Request(); var second = rig.Request();
                operation = rig.Sdk.Raw[0]; rig.Dispatch.Run();
                operation.EnsureDone = true; operation.Emit();
                Assert.IsTrue(rig.Dispatch.Step());
                body = operation.Body;
                Assert.Greater(body.Position, 0); Assert.Less(body.Position, Chunked);
                Assert.AreEqual(65536, body.Position);
                Assert.AreEqual(reservedBefore + Chunked, ledger.GetValue(null));
                var reads = body.Reads; var posts = rig.Dispatch.Posts;
                rig.Dispatch.FailPost = true;
                Assert.IsTrue(rig.Dispatch.Step());
                Reject(first, Code.SdkFailure, Stage.ValidateResult);
                Reject(second, Code.SdkFailure, Stage.ValidateResult);
                Assert.IsFalse(rig.Dispatch.Step()); Assert.LessOrEqual(rig.Dispatch.Maximum, 1);
                Assert.Greater(rig.Dispatch.Posts, posts); Assert.LessOrEqual(rig.Dispatch.Posts - posts, 3);
                Assert.AreEqual(reads + 1, body.Reads); Assert.AreEqual(131072, body.Position);
                Assert.Less(body.Position, Chunked); Assert.LessOrEqual(body.Maximum, 65536);
                Assert.IsTrue(operation.Done); Assert.IsTrue(operation.Reader.Done); Assert.IsFalse(operation.Success);
                Assert.AreEqual(0, operation.Transfers); Assert.IsNull(operation.Payload);
                Assert.AreEqual(1, body.Closes); Assert.AreEqual(1, operation.Releases);
                Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, operation.ReleaseThread);
                Assert.AreEqual(reservedBefore, ledger.GetValue(null));
                CapacityRestored();
            }
            Assert.AreEqual(1, body.Closes); Assert.AreEqual(1, operation.Releases);
            Assert.AreEqual(reservedBefore, ledger.GetValue(null));
        }
    }
}
