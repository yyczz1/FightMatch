using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using QFramework;

namespace FightMatch.Framework.Tests
{
    [TestFixture]
    public sealed class QFrameworkIntegrationTests
    {
        // This Unity NUnit build omits NonParallelizable; serialize this fixture explicitly.
        private static readonly object FixtureGate = new object();

        [SetUp]
        public void EnterFixture() { Monitor.Enter(FixtureGate); }

        [TearDown]
        public void ExitFixture() { Monitor.Exit(FixtureGate); }

        [Test]
        public void InterfaceInitializesModelsBeforeSystemsOnceAndRebuildsAfterDeinit()
        {
            IArchitecture owned = null;
            try
            {
                owned = ProbeArchitecture.Interface;
                var first = (ProbeArchitecture)owned;
                var model = owned.GetModel<ProbeModel>();
                var system = owned.GetSystem<ProbeSystem>();
                Assert.AreSame(first, ProbeArchitecture.Interface);
                Assert.AreSame(model, ProbeArchitecture.Interface.GetModel<ProbeModel>());
                Assert.AreSame(system, ProbeArchitecture.Interface.GetSystem<ProbeSystem>());
                Assert.AreEqual(1, first.InitCount);
                Assert.AreEqual(1, model.InitCount);
                Assert.AreEqual(1, system.InitCount);
                Assert.IsTrue(system.SawInitializedModel);
                CollectionAssert.AreEqual(new[] { "model.init", "system.init" }, first.Trace);
                model.Value = 99;

                owned.Deinit(); owned = null;
                Assert.AreEqual(1, first.DeinitCount);
                Assert.AreEqual(1, model.DeinitCount);
                Assert.AreEqual(1, system.DeinitCount);
                owned = ProbeArchitecture.Interface;
                var rebuilt = (ProbeArchitecture)owned;
                var freshModel = owned.GetModel<ProbeModel>();
                var freshSystem = owned.GetSystem<ProbeSystem>();
                Assert.AreNotSame(first, rebuilt);
                Assert.AreNotSame(model, freshModel);
                Assert.AreNotSame(system, freshSystem);
                Assert.AreEqual(0, freshModel.Value);
                Assert.AreEqual(1, rebuilt.InitCount);
                Assert.AreEqual(1, freshModel.InitCount);
                Assert.AreEqual(1, freshSystem.InitCount);
                CollectionAssert.AreEqual(new[] { "model.init", "system.init" }, rebuilt.Trace);
                owned.Deinit(); owned = null;
                Assert.AreEqual(1, rebuilt.DeinitCount);
                Assert.AreEqual(1, freshModel.DeinitCount);
                Assert.AreEqual(1, freshSystem.DeinitCount);
                Assert.AreEqual(1, model.DeinitCount);
            }
            finally { owned?.Deinit(); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CommandQueryAndEventCompleteSynchronouslyOnTheirCallingThread(bool workerThread)
        {
            var runnerThread = Thread.CurrentThread.ManagedThreadId;
            var callerThread = 0;
            string evidence = null;
            Exception failure = null;
            Action run = () =>
            {
                callerThread = Thread.CurrentThread.ManagedThreadId;
                try { evidence = ExerciseSynchronousCalls(); }
                catch (Exception ex) { failure = ex; }
            };
            if (workerThread)
            {
                var worker = new Thread(() => run());
                worker.Start();
                worker.Join();
                Assert.AreNotEqual(runnerThread, callerThread);
            }
            else { run(); Assert.AreEqual(runnerThread, callerThread); }
            Assert.IsNull(failure, failure?.ToString());
            TestContext.Out.WriteLine("runnerThread=" + runnerThread + "; " + evidence);
        }

        private static string ExerciseSynchronousCalls()
        {
            IArchitecture owned = null;
            IUnRegister subscription = null;
            try
            {
                owned = ProbeArchitecture.Interface;
                var model = owned.GetModel<ProbeModel>();
                model.Trace.Clear();
                var caller = Thread.CurrentThread.ManagedThreadId;
                var notifications = 0;
                subscription = owned.RegisterEvent<ValueChanged>(e =>
                {
                    model.Trace.Add("event");
                    model.Threads.Add(Thread.CurrentThread.ManagedThreadId);
                    Assert.AreEqual(7, e.Value);
                    Assert.AreEqual(7, model.Value);
                    notifications++;
                });
                model.Trace.Add("caller.before");
                owned.SendCommand(new SetValueCommand());
                model.Trace.Add("caller.after");
                Assert.AreEqual(7, model.Value);
                Assert.AreEqual(1, notifications);
                Assert.AreEqual(7, owned.SendQuery(new ReadValueQuery()));
                model.Trace.Add("query.returned");
                CollectionAssert.AreEqual(new[]
                {
                    "caller.before", "command", "event", "command.after-event",
                    "caller.after", "query", "query.returned"
                }, model.Trace);
                CollectionAssert.AreEqual(new[] { caller, caller, caller }, model.Threads);
                return "callerThread=" + caller + "; command/event/queryThreads=" +
                    string.Join(",", model.Threads) + "; order=" + string.Join(",", model.Trace);
            }
            finally { subscription?.UnRegister(); owned?.Deinit(); }
        }

        [Test]
        public void DuplicateSubscriptionsUnregisterOneAtATimeAndDoNotLeakIntoRebuiltArchitecture()
        {
            IArchitecture owned = null;
            IUnRegister first = null, second = null, fresh = null;
            try
            {
                owned = ProbeArchitecture.Interface;
                var oldCalls = 0;
                Action<ValueChanged> handler = _ => oldCalls++;
                first = owned.RegisterEvent(handler);
                second = owned.RegisterEvent(handler);
                owned.SendEvent(new ValueChanged(1));
                Assert.AreEqual(2, oldCalls);
                first.UnRegister(); first = null;
                owned.SendEvent(new ValueChanged(2));
                Assert.AreEqual(3, oldCalls);
                second.UnRegister(); second = null;
                owned.SendEvent(new ValueChanged(3));
                Assert.AreEqual(3, oldCalls);
                var oldArchitecture = owned;
                owned.Deinit(); owned = null;

                owned = ProbeArchitecture.Interface;
                Assert.AreNotSame(oldArchitecture, owned);
                var freshCalls = 0;
                fresh = owned.RegisterEvent<ValueChanged>(_ => freshCalls++);
                owned.SendEvent(new ValueChanged(4));
                Assert.AreEqual(1, freshCalls);
                Assert.AreEqual(3, oldCalls);
                fresh.UnRegister(); fresh = null;
                owned.SendEvent(new ValueChanged(5));
                Assert.AreEqual(1, freshCalls);
                TestContext.Out.WriteLine("same-handler callbacks per send: 2,1,0; rebuilt: old=0,new=1");
            }
            finally
            {
                first?.UnRegister(); second?.UnRegister(); fresh?.UnRegister();
                owned?.Deinit();
            }
        }

        [Test]
        public void ExistingPureAssembliesKeepTheirCompiledReferenceBoundary()
        {
            Assert.AreEqual("QFramework", typeof(IArchitecture).Assembly.GetName().Name);
            foreach (var name in new[] { "FightMatch.Core", "FightMatch.Platform", "FightMatch.Core.Tests" })
            {
                var assembly = Assembly.Load(name);
                var references = assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
                Assert.IsFalse(references.Any(x => (x == "QFramework" && name != "FightMatch.Core.Tests") ||
                    (name != "FightMatch.Core.Tests" && (x.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    x.StartsWith("UnityEditor", StringComparison.Ordinal)))), name);
                TestContext.Out.WriteLine(name + " references: " + string.Join(",", references));
            }
        }

        private sealed class ProbeArchitecture : Architecture<ProbeArchitecture>
        {
            internal readonly List<string> Trace = new List<string>();
            internal int InitCount, DeinitCount;
            public ProbeArchitecture() { }
            protected override void Init()
            {
                InitCount++;
                RegisterSystem(new ProbeSystem(Trace));
                RegisterModel(new ProbeModel(Trace));
            }
            protected override void OnDeinit() { DeinitCount++; }
        }

        private sealed class ProbeModel : AbstractModel
        {
            internal readonly List<string> Trace;
            internal readonly List<int> Threads = new List<int>();
            internal int Value, InitCount, DeinitCount;
            internal ProbeModel(List<string> trace) { Trace = trace; }
            protected override void OnInit() { InitCount++; Trace.Add("model.init"); }
            protected override void OnDeinit() { DeinitCount++; }
        }

        private sealed class ProbeSystem : AbstractSystem
        {
            private readonly List<string> trace;
            internal int InitCount, DeinitCount;
            internal bool SawInitializedModel;
            internal ProbeSystem(List<string> trace) { this.trace = trace; }
            protected override void OnInit()
            {
                InitCount++;
                SawInitializedModel = this.GetModel<ProbeModel>().Initialized;
                trace.Add("system.init");
            }
            protected override void OnDeinit() { DeinitCount++; }
        }

        private struct ValueChanged
        {
            internal readonly int Value;
            internal ValueChanged(int value) { Value = value; }
        }

        private sealed class SetValueCommand : AbstractCommand
        {
            protected override void OnExecute()
            {
                var model = this.GetModel<ProbeModel>();
                model.Trace.Add("command");
                model.Threads.Add(Thread.CurrentThread.ManagedThreadId);
                model.Value = 7;
                this.SendEvent(new ValueChanged(model.Value));
                model.Trace.Add("command.after-event");
            }
        }

        private sealed class ReadValueQuery : AbstractQuery<int>
        {
            protected override int OnDo()
            {
                var model = this.GetModel<ProbeModel>();
                model.Trace.Add("query");
                model.Threads.Add(Thread.CurrentThread.ManagedThreadId);
                return model.Value;
            }
        }
    }
}
