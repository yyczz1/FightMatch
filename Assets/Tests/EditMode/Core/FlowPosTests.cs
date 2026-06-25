using FlowPuzzle.Core;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Core
{
    [TestFixture]
    public class FlowPosTests
    {
        [Test]
        public void Constructor_SetsFields()
        {
            var pos = new FlowPos(3, 7);

            Assert.AreEqual(3, pos.x);
            Assert.AreEqual(7, pos.y);
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new FlowPos(2, 5);
            var b = new FlowPos(2, 5);

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentX_ReturnsFalse()
        {
            var a = new FlowPos(1, 5);
            var b = new FlowPos(2, 5);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentY_ReturnsFalse()
        {
            var a = new FlowPos(2, 4);
            var b = new FlowPos(2, 5);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentBoth_ReturnsFalse()
        {
            var a = new FlowPos(1, 4);
            var b = new FlowPos(2, 5);

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Object_SameValues_ReturnsTrue()
        {
            object a = new FlowPos(2, 5);
            object b = new FlowPos(2, 5);

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            var pos = new FlowPos(2, 5);
            object other = "not a FlowPos";

            Assert.IsFalse(pos.Equals(other));
        }

        [Test]
        public void Equals_Object_Null_ReturnsFalse()
        {
            var pos = new FlowPos(2, 5);

            Assert.IsFalse(pos.Equals(null));
        }

        [Test]
        public void OperatorEquals_SameValues_ReturnsTrue()
        {
            var a = new FlowPos(2, 5);
            var b = new FlowPos(2, 5);

            Assert.IsTrue(a == b);
        }

        [Test]
        public void OperatorEquals_DifferentValues_ReturnsFalse()
        {
            var a = new FlowPos(1, 4);
            var b = new FlowPos(2, 5);

            Assert.IsFalse(a == b);
        }

        [Test]
        public void OperatorNotEquals_DifferentValues_ReturnsTrue()
        {
            var a = new FlowPos(1, 4);
            var b = new FlowPos(2, 5);

            Assert.IsTrue(a != b);
        }

        [Test]
        public void OperatorNotEquals_SameValues_ReturnsFalse()
        {
            var a = new FlowPos(2, 5);
            var b = new FlowPos(2, 5);

            Assert.IsFalse(a != b);
        }

        [Test]
        public void GetHashCode_SameValues_ReturnsEqual()
        {
            var a = new FlowPos(2, 5);
            var b = new FlowPos(2, 5);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
