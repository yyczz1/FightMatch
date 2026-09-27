using FightMatch.Input;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    public sealed class RouteGestureUnityTests
    {
        [TestCaseSource(typeof(RouteGestureCases), nameof(RouteGestureCases.CaseIds))]
        public void OriginalG1Case(string caseId)
        {
            Assert.IsTrue(RouteGestureCases.Run(caseId), caseId);
        }
    }
}
