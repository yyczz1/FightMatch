using System;
using System.Diagnostics;
using System.Globalization;
using UnityEditor;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Host.Tests")]

namespace FightMatch.Host.Editor
{
    [InitializeOnLoad]
    internal static class FightMatchEditorPlaySession
    {
        internal const string SessionKey = "FightMatch.Host.ResourceActivation.PlaySession";

        static FightMatchEditorPlaySession() { Initialize(); }

        internal static void Initialize()
        {
            EditorApplication.playModeStateChanged -= Observe;
            EditorApplication.playModeStateChanged += Observe;
            // Assembly reload restores this Play identity; it must not manufacture another start.
            var token = SessionState.GetString(SessionKey, string.Empty);
            FightMatchResourceActivationStore.SetDesktopSessionToken(string.IsNullOrEmpty(token) ? null : token);
        }

        internal static void Observe(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                var token = NewSessionToken();
                SessionState.SetString(SessionKey, token);
                FightMatchResourceActivationStore.SetDesktopSessionToken(token);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseString(SessionKey);
                FightMatchResourceActivationStore.SetDesktopSessionToken(null);
            }
        }

        private static string NewSessionToken()
        {
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    // Keep schema 1's existing pPID-positive-integer spelling; the Editor's nonce
                    // identifies Play, not OS process creation or an assembly reload.
                    var nonce = (BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0) & long.MaxValue) | 1;
                    return "p" + process.Id.ToString(CultureInfo.InvariantCulture) + "-" + nonce.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception) { return "unavailable"; } // Fail closed in the existing token validator.
        }
    }
}
