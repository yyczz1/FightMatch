using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Generation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Generation
{
    [TestFixture]
    public class FlowPathLengthAllocatorTests
    {
        private static FlowGenerationConfig MakeConfig(int w, int h, int colors, int minLen, int maxLen)
        {
            return new FlowGenerationConfig
            {
                width = w, height = h,
                colorCount = colors,
                minPathLength = minLen, maxPathLength = maxLen
            };
        }

        [Test]
        public void SystemFlowRandom_SameSeed_IdenticalSequence()
        {
            var r1 = new SystemFlowRandom(42);
            var r2 = new SystemFlowRandom(42);

            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(r1.NextInt(0, 100), r2.NextInt(0, 100));
                Assert.AreEqual(r1.NextFloat(0f, 1f), r2.NextFloat(0f, 1f), 0.0001f);
            }
        }

        [Test]
        public void SystemFlowRandom_DifferentSeeds_DifferentSequence()
        {
            var r1 = new SystemFlowRandom(42);
            var r2 = new SystemFlowRandom(99);
            var diff = false;
            for (var i = 0; i < 100; i++)
            {
                if (r1.NextInt(0, 1000) != r2.NextInt(0, 1000)) { diff = true; break; }
            }
            Assert.IsTrue(diff);
        }

        [Test]
        public void SystemFlowRandom_Shuffle_SameSeed_SameOrder()
        {
            var items1 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var items2 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var r1 = new SystemFlowRandom(42);
            var r2 = new SystemFlowRandom(42);
            r1.Shuffle(items1);
            r2.Shuffle(items2);
            Assert.IsTrue(items1.SequenceEqual(items2));
        }

        [Test]
        public void SystemFlowRandom_NextInt_RejectsInvertedRange()
        {
            var r = new SystemFlowRandom(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextInt(5, 3));
        }

        [Test]
        public void SystemFlowRandom_NextFloat_RejectsInvertedRange()
        {
            var r = new SystemFlowRandom(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextFloat(5f, 3f));
        }

        [Test]
        public void SystemFlowRandom_NextFloat_RejectsEqualBounds()
        {
            var r = new SystemFlowRandom(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextFloat(3f, 3f));
        }

        [Test]
        public void SystemFlowRandom_NextInt_EmptyRange_Rejected()
        {
            var r = new SystemFlowRandom(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextInt(3, 3));
        }

        [Test]
        public void Allocator_NullConfig_Throws()
        {
            var a = new FlowPathLengthAllocator();
            Assert.Throws<ArgumentNullException>(() => a.Allocate(null, 10, new SystemFlowRandom(1)));
        }

        [Test]
        public void Allocator_NullRandom_Throws()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 2, 2, 4);
            Assert.Throws<ArgumentNullException>(() => a.Allocate(cfg, 10, null));
        }

        [Test]
        public void Allocator_ImpossibleMinimumOccupancy()
        {
            var a = new FlowPathLengthAllocator();
            // 2 colors * 5 minLength = 10 > 3*3=9 board capacity
            var cfg = MakeConfig(3, 3, 2, 5, 10);
            var result = a.Allocate(cfg, 9, new SystemFlowRandom(1));

            Assert.IsFalse(result.success);
            Assert.AreEqual("ImpossibleMinimumOccupancy", result.diagnostic.errorCode);
        }

        [Test]
        public void Allocator_InvalidDimensions()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(0, 5, 2, 2, 4);
            var result = a.Allocate(cfg, 10, new SystemFlowRandom(1));
            Assert.IsFalse(result.success);
            Assert.AreEqual("InvalidDimensions", result.diagnostic.errorCode);
        }

        [Test]
        public void Allocator_TargetBelowMinimum_ClampsUp()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 2, 4, 6);
            // feasible min = 2 * 4 = 8. Request 5 → clamped to 8
            var result = a.Allocate(cfg, 5, new SystemFlowRandom(1));

            Assert.IsTrue(result.success);
            Assert.AreEqual(8, result.allocatedUsedCellCount);
            Assert.AreEqual(8, result.pathLengthsByColorId.Sum());
        }

        [Test]
        public void Allocator_TargetAboveMaximum_ClampsDown()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 2, 2, 4);
            // board=25, max per color=4, total max = 2*4=8. Request 20 → clamped to 8
            var result = a.Allocate(cfg, 20, new SystemFlowRandom(1));

            Assert.IsTrue(result.success);
            Assert.AreEqual(8, result.allocatedUsedCellCount);
        }

        [Test]
        public void Allocator_LengthsInRange()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 3, 2, 5);
            var result = a.Allocate(cfg, 10, new SystemFlowRandom(42));

            Assert.IsTrue(result.success);
            foreach (var len in result.pathLengthsByColorId)
                Assert.IsTrue(len >= 2 && len <= 5, $"Length {len} out of range");
        }

        [Test]
        public void Allocator_SameSeed_SameAllocation()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(6, 6, 3, 2, 5);

            var r1 = a.Allocate(cfg, 12, new SystemFlowRandom(42));
            var r2 = a.Allocate(cfg, 12, new SystemFlowRandom(42));

            Assert.IsTrue(r1.success);
            Assert.IsTrue(r2.success);
            Assert.IsTrue(r1.pathLengthsByColorId.SequenceEqual(r2.pathLengthsByColorId));
            Assert.IsTrue(r1.generationOrderColorIds.SequenceEqual(r2.generationOrderColorIds));
        }

        [Test]
        public void Allocator_GenerationOrder_DescendingLength_StableColorId()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 3, 3, 3);
            // all equal lengths → stable by ascending color ID
            var result = a.Allocate(cfg, 9, new SystemFlowRandom(1));

            Assert.IsTrue(result.success);
            for (var i = 0; i < result.generationOrderColorIds.Count - 1; i++)
            {
                var cur = result.generationOrderColorIds[i];
                var next = result.generationOrderColorIds[i + 1];
                var curLen = result.pathLengthsByColorId[cur];
                var nextLen = result.pathLengthsByColorId[next];
                Assert.IsTrue(curLen >= nextLen || (curLen == nextLen && cur < next),
                    $"Order violation at {i}: color {cur} len {curLen} before color {next} len {nextLen}");
            }
        }

        [Test]
        public void Allocator_ReturnsExactClampedTarget()
        {
            var a = new FlowPathLengthAllocator();
            var cfg = MakeConfig(5, 5, 3, 2, 5);
            var result = a.Allocate(cfg, 9, new SystemFlowRandom(42));

            Assert.IsTrue(result.success);
            Assert.AreEqual(9, result.allocatedUsedCellCount);
            Assert.AreEqual(9, result.pathLengthsByColorId.Sum());
        }

        [Test]
        public void FlowPathLengthAllocationResult_Success_SetsFields()
        {
            var lengths = new List<int> { 4, 3, 5 };
            var result = FlowPathLengthAllocationResult.Success(12, lengths);

            Assert.IsTrue(result.success);
            Assert.AreEqual(12, result.requestedTargetUsedCellCount);
            Assert.AreEqual(12, result.allocatedUsedCellCount);
            Assert.IsNull(result.diagnostic);
            Assert.IsTrue(lengths.SequenceEqual(result.pathLengthsByColorId));

            // prove list-copy ownership: mutate source, verify result unaffected
            var oldSum = result.allocatedUsedCellCount;
            var oldOrder = new List<int>(result.generationOrderColorIds);
            lengths[0] = 999;
            lengths.Add(99);

            Assert.AreEqual(oldSum, result.allocatedUsedCellCount,
                "mutating source list should not affect result");
            Assert.IsTrue(oldOrder.SequenceEqual(result.generationOrderColorIds),
                "generation order should be stable after source mutation");
            Assert.AreNotSame(lengths, result.pathLengthsByColorId);
            Assert.AreNotSame(lengths, result.generationOrderColorIds);
        }

        [Test]
        public void FlowPathLengthAllocationResult_Failure_SetsFields()
        {
            var result = FlowPathLengthAllocationResult.Failure(10, "ERR", "msg");

            Assert.IsFalse(result.success);
            Assert.AreEqual(10, result.requestedTargetUsedCellCount);
            Assert.IsNotNull(result.diagnostic);
            Assert.AreEqual("ERR", result.diagnostic.errorCode);
        }
    }
}
