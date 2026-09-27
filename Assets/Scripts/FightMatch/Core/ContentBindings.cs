using System;
using System.Globalization;
using System.Numerics;

namespace FightMatch.Core
{
    // Exact identities only. Neither value proves publication nor grants a save capability.
    public sealed class ContentBinding
    {
        public string PackageId { get; }
        public string ContentFingerprint { get; }
        public string RuleVersion { get; }
        public string NumericContractVersion { get; }
        public string RandomContractVersion { get; }

        private ContentBinding(string package, string fingerprint, string rule, string numeric, string random)
        { PackageId = package; ContentFingerprint = fingerprint; RuleVersion = rule; NumericContractVersion = numeric; RandomContractVersion = random; }

        public static SaveCodecResult<ContentBinding> Prepare(string packageId, string contentFingerprint, string ruleVersion,
            string numericContractVersion, string randomContractVersion, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<ContentBinding>.Run(() =>
            {
                RuleContextChecks.Text(packageId, "Binding.PackageId", budget, true);
                RuleContextChecks.Text(contentFingerprint, "Binding.ContentFingerprint", budget, true);
                RuleContextChecks.Text(ruleVersion, "Binding.RuleVersion", budget, true);
                RuleContextChecks.Text(numericContractVersion, "Binding.NumericContractVersion", budget, true);
                RuleContextChecks.Text(randomContractVersion, "Binding.RandomContractVersion", budget, true);
                return new ContentBinding(packageId, contentFingerprint, ruleVersion, numericContractVersion, randomContractVersion);
            });
        }

        public bool Same(ContentBinding other)
        {
            return other != null && PackageId == other.PackageId && ContentFingerprint == other.ContentFingerprint &&
                RuleVersion == other.RuleVersion && NumericContractVersion == other.NumericContractVersion &&
                RandomContractVersion == other.RandomContractVersion;
        }
    }

    public sealed class DefinitionBinding
    {
        public ContentBinding Content { get; }
        public string LevelId { get; }
        public BigInteger LevelVersion { get; }
        public string CanonicalLevelVersion => LevelVersion.ToString(CultureInfo.InvariantCulture);

        private DefinitionBinding(ContentBinding content, string levelId, BigInteger levelVersion)
        { Content = content; LevelId = levelId; LevelVersion = levelVersion; }

        public static SaveCodecResult<DefinitionBinding> Prepare(ContentBinding content, string levelId, BigInteger levelVersion, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<DefinitionBinding>.Run(() =>
            {
                RuleContextChecks.BindingBudget(content, budget);
                RuleContextChecks.Text(levelId, "Binding.LevelId", budget, true);
                BusinessFields.Take(ExactSaveValueCodec.EncodeInteger(levelVersion, budget), "Binding.LevelVersion");
                SaveCodecFailure.Require(levelVersion.Sign > 0, "InvalidValue", "Binding.LevelVersion");
                return new DefinitionBinding(content, levelId, levelVersion);
            });
        }

        public bool Same(DefinitionBinding other)
        { return other != null && Content.Same(other.Content) && LevelId == other.LevelId && LevelVersion == other.LevelVersion; }

        internal void CheckBudget(ExactMathBudget budget) { budget.CheckInteger(LevelVersion); }
    }
}
