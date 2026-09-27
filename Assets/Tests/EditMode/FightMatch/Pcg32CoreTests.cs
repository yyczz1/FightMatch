using System;
using FightMatch.Core;
using NUnit.Framework;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public class Pcg32CoreTests
    {
        // https://www.pcg-random.org/using-pcg-c-basic.html (seed 42, sequence 54)
        private static readonly uint[] ReferenceWords =
        {
            0xa15c02b7U, 0x7b47f409U, 0xba1d3330U,
            0x83d2f293U, 0xbfa4784bU, 0xcbed606eU
        };

        [Test]
        public void Initialize_ReferenceSeed_ProducesPublishedSixWords()
        {
            var state = Pcg32Core.Initialize(42, 54);
            Assert.AreEqual(109UL, state.Increment);

            foreach (var expected in ReferenceWords)
            {
                var previous = state;
                var previousValue = previous.State;
                Assert.AreEqual(expected, Pcg32Core.Next32(previous, out state));
                Assert.AreEqual(previousValue, previous.State);
                Assert.AreEqual(109UL, state.Increment);
            }
        }

        [Test]
        public void Initialize_ZeroSeedAndSequence_AdvancesTwice()
        {
            var state = Pcg32Core.Initialize(0, 0);

            Assert.AreEqual(0x5851f42d4c957f2eUL, state.State);
            Assert.AreEqual(1UL, state.Increment);
        }

        [Test]
        public void Initialize_MaximumLegalInputs_WrapsSeedAdditionAndMultiplication()
        {
            var state = Pcg32Core.Initialize(ulong.MaxValue, 0x7fffffffffffffffUL);

            Assert.AreEqual(0x4f5c17a566d501a5UL, state.State);
            Assert.AreEqual(ulong.MaxValue, state.Increment);
        }

        [TestCase(0x8000000000000000UL)]
        [TestCase(ulong.MaxValue)]
        public void Initialize_SequenceHighBitSet_RejectsInsteadOfMasking(ulong sequence)
        {
            var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Pcg32Core.Initialize(42, sequence));

            Assert.AreEqual("initSequence", error.ParamName);
        }

        [TestCase(0UL, 1UL)]
        [TestCase(ulong.MaxValue, 1UL)]
        [TestCase(0UL, ulong.MaxValue)]
        [TestCase(ulong.MaxValue, ulong.MaxValue)]
        public void Restore_LegalBitPatterns_PreservesBothFields(ulong value, ulong increment)
        {
            var state = Pcg32Core.Restore(value, increment);

            Assert.AreEqual(value, state.State);
            Assert.AreEqual(increment, state.Increment);
        }

        [TestCase(0UL)]
        [TestCase(2UL)]
        [TestCase(0xfffffffffffffffeUL)]
        public void Restore_EvenIncrement_RejectsInsteadOfChangingIt(ulong increment)
        {
            var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Pcg32Core.Restore(0, increment));

            Assert.AreEqual("increment", error.ParamName);
        }

        [Test]
        public void Next32_ZeroState_UsesOldStateAndPreservesInput()
        {
            var state = Pcg32Core.Restore(0, 1);

            Assert.AreEqual(0U, Pcg32Core.Next32(state, out var next));
            Assert.AreEqual(1UL, next.State);
            Assert.AreEqual(1UL, next.Increment);
            Assert.AreEqual(0UL, state.State);
            CollectionAssert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0
            }, Pcg32Core.EncodeCore(state));
        }

        [Test]
        public void Next32_MaximumState_CoversRotation31AndUnsignedWrap()
        {
            var state = Pcg32Core.Restore(ulong.MaxValue, 1);

            Assert.AreEqual(0xfff00001U, Pcg32Core.Next32(state, out var next));
            Assert.AreEqual(0xa7ae0bd2b36a80d4UL, next.State);
            Assert.AreEqual(1UL, next.Increment);
            Assert.AreEqual(ulong.MaxValue, state.State);
        }

        [Test]
        public void Next32_ZeroRotationWithNonzeroOutput_ReturnsUnrotatedWord()
        {
            var state = Pcg32Core.Restore(0x08000000UL, 1);

            Assert.AreEqual(1U, Pcg32Core.Next32(state, out _));
        }

        [Test]
        public void Next32_ReusingSamePriorState_RepeatsWordAndNextState()
        {
            var state = Pcg32Core.Initialize(42, 54);
            var priorValue = state.State;
            var first = Pcg32Core.Next32(state, out var next);
            var repeated = Pcg32Core.Next32(state, out var repeatedNext);

            Assert.AreEqual(first, repeated);
            Assert.AreEqual(next.State, repeatedNext.State);
            Assert.AreEqual(next.Increment, repeatedNext.Increment);
            Assert.AreEqual(priorValue, state.State);
        }

        [Test]
        public void Next32_InterleavedInstances_KeepSeparateSequences()
        {
            var first = Pcg32Core.Initialize(42, 54);
            var second = Pcg32Core.Initialize(7, 19);
            var secondControl = Pcg32Core.Initialize(7, 19);

            foreach (var expected in ReferenceWords)
            {
                Assert.AreEqual(expected, Pcg32Core.Next32(first, out first));
                var word = Pcg32Core.Next32(second, out second);
                var controlWord = Pcg32Core.Next32(secondControl, out secondControl);
                Assert.AreEqual(controlWord, word);
                Assert.AreEqual(secondControl.State, second.State);
                Assert.AreEqual(39UL, second.Increment);
            }
        }

        [Test]
        public void EncodeAndDecode_AfterThirdWord_ResumeWithRemainingPublishedWords()
        {
            var state = Pcg32Core.Initialize(42, 54);
            for (var i = 0; i < 3; i++)
                Pcg32Core.Next32(state, out state);

            var restored = Pcg32Core.DecodeCore(Pcg32Core.EncodeCore(state));
            Assert.AreEqual(state.State, restored.State);
            Assert.AreEqual(state.Increment, restored.Increment);
            for (var i = 3; i < ReferenceWords.Length; i++)
                Assert.AreEqual(ReferenceWords[i], Pcg32Core.Next32(restored, out restored));
        }

        [Test]
        public void EncodeCore_KnownFields_UsesLittleEndianStateThenIncrement()
        {
            var state = Pcg32Core.Restore(0x0123456789abcdefUL, 0xfedcba9876543211UL);
            var first = Pcg32Core.EncodeCore(state);
            var second = Pcg32Core.EncodeCore(state);

            CollectionAssert.AreEqual(KnownBytes(), first);
            CollectionAssert.AreEqual(KnownBytes(), second);
            Assert.AreNotSame(first, second);
            first[0] = 0;
            first[8] = 0;
            Assert.AreEqual(0x0123456789abcdefUL, state.State);
            Assert.AreEqual(0xfedcba9876543211UL, state.Increment);
            CollectionAssert.AreEqual(KnownBytes(), second);
        }

        [Test]
        public void DecodeCore_KnownBytes_ReadsBothFieldsAndDoesNotRetainArray()
        {
            var bytes = KnownBytes();
            var state = Pcg32Core.DecodeCore(bytes);

            CollectionAssert.AreEqual(KnownBytes(), bytes);
            Array.Clear(bytes, 0, bytes.Length);
            Assert.AreEqual(0x0123456789abcdefUL, state.State);
            Assert.AreEqual(0xfedcba9876543211UL, state.Increment);
            CollectionAssert.AreEqual(KnownBytes(), Pcg32Core.EncodeCore(state));
        }

        [TestCase(0)]
        [TestCase(15)]
        [TestCase(17)]
        public void DecodeCore_WrongLength_RejectsWithoutPaddingOrTruncation(int length)
        {
            var bytes = new byte[length];
            if (length > 8)
                bytes[8] = 1;
            var error = Assert.Throws<ArgumentException>(() => Pcg32Core.DecodeCore(bytes));

            Assert.AreEqual("bytes", error.ParamName);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(254)]
        public void DecodeCore_EvenIncrement_RejectsAndPreservesBytes(int lowByte)
        {
            var bytes = KnownBytes();
            bytes[8] = (byte)lowByte;
            var original = (byte[])bytes.Clone();
            var error = Assert.Throws<ArgumentException>(() => Pcg32Core.DecodeCore(bytes));

            Assert.AreEqual("bytes", error.ParamName);
            CollectionAssert.AreEqual(original, bytes);
        }

        [Test]
        public void NullRequiredArguments_ReportExactExceptionAndParameter()
        {
            Assert.AreEqual("state", Assert.Throws<ArgumentNullException>(() =>
                Pcg32Core.Next32(null, out _)).ParamName);
            Assert.AreEqual("state", Assert.Throws<ArgumentNullException>(() =>
                Pcg32Core.EncodeCore(null)).ParamName);
            Assert.AreEqual("bytes", Assert.Throws<ArgumentNullException>(() =>
                Pcg32Core.DecodeCore(null)).ParamName);
        }

        private static byte[] KnownBytes()
        {
            return new byte[]
            {
                0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01,
                0x11, 0x32, 0x54, 0x76, 0x98, 0xba, 0xdc, 0xfe
            };
        }
    }
}
