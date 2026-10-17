using System;
using System.Collections.Generic;
using System.Text;
using FightMatch.Presentation;

namespace FightMatch.Host
{
    internal sealed class FightMatchLocalizedTextSourceAdapter : ILocalizedTextSource
    {
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

        internal FightMatchLocalizedTextSourceAdapter(IReadOnlyList<Entry> catalog)
        {
            foreach (var entry in catalog) entries.Add(entry.Key, entry);
        }

        public LocalizedTextResult Resolve(string key, LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> namedArgs)
        {
            if (locale != LocaleId.En && locale != LocaleId.ZhHans) return LocalizedTextResult.Failure("UnknownLocale");
            if (key == null || !entries.TryGetValue(key, out var entry)) return LocalizedTextResult.Failure("MissingLocalizationKey");
            return entry.Resolve(locale, namedArgs);
        }

        internal sealed class Entry
        {
            internal readonly string Key, English, Chinese;
            internal readonly LocalizedTextSeverity Severity;
            private readonly string[] parameters;

            internal Entry(string key, string english, string chinese, IReadOnlyList<string> parameters, LocalizedTextSeverity severity)
            {
                Key = key; English = english; Chinese = chinese; Severity = severity;
                this.parameters = new string[parameters.Count];
                for (var i = 0; i < parameters.Count; i++) this.parameters[i] = parameters[i];
            }

            internal LocalizedTextResult Resolve(LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> namedArgs)
            {
                var template = locale == LocaleId.En ? English : Chinese;
                if (string.IsNullOrEmpty(template)) return LocalizedTextResult.Failure("MissingLocaleText");
                var values = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var arg in namedArgs ?? Array.Empty<KeyValuePair<string, string>>())
                {
                    if (string.IsNullOrEmpty(arg.Key) || arg.Value == null || values.ContainsKey(arg.Key))
                        return LocalizedTextResult.Failure("InvalidNamedArguments");
                    values.Add(arg.Key, arg.Value);
                }
                if (values.Count != parameters.Length) return LocalizedTextResult.Failure("ParameterMismatch");
                foreach (var name in parameters)
                    if (!values.ContainsKey(name)) return LocalizedTextResult.Failure("ParameterMismatch");

                var text = new StringBuilder();
                var start = 0;
                for (var i = 0; i < template.Length; i++)
                {
                    if (template[i] == '}') return LocalizedTextResult.Failure("InvalidTemplate");
                    if (template[i] != '{') continue;
                    var close = template.IndexOf('}', i + 1);
                    if (close <= i + 1 || !values.TryGetValue(template.Substring(i + 1, close - i - 1), out var value))
                        return LocalizedTextResult.Failure("InvalidTemplate");
                    text.Append(template, start, i - start);
                    text.Append(value);
                    i = close; start = close + 1;
                }
                text.Append(template, start, template.Length - start);
                return LocalizedTextResult.Success(text.ToString(), Severity);
            }
        }
    }
}
