namespace FightMatch.Tests
{
    public static class SavePendingRecoveryProcessEntry
    {
        public static void Run()
        {
            SavePendingRecoveryProcessCases.Run(System.Environment.GetCommandLineArgs());
        }
    }
}
