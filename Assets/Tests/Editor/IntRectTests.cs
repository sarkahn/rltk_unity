using NUnit.Framework;
using System.Linq;
using Unity.Mathematics;


namespace RLTK.EditorTests
{

    [TestFixture]
    public class IntRectTests
    {
        [Test]
        public void RectInitialation()
        {
            IntRect r = IntRect.FromExtents(0, 0, 10, 10);

            Assert.AreEqual(new int2(0, 0), r.Position);
            Assert.AreEqual(new int2(10, 10), r.Max);
            Assert.AreEqual(new int2(10, 10), r.Size);

            r = IntRect.FromPositionSize(5, 5, 5, 5);

            Assert.IsFalse(r.Intersect(new int2(0, 0)));
            Assert.AreEqual(new int2(5, 5), r.Position);
            Assert.AreEqual(new int2(5, 5), r.Size);
        }

        [Test]
        public void IEnumerableVisitsExpectedPoints()
        {
            int size = 10;
            IntRect r = IntRect.FromExtents(0, 0, size, size);

            var points =
                (from p in r
                 select p).ToList();

            for (int x = 0; x < size; ++x)
                for (int y = 0; y < size; ++y)
                    Assert.Contains(new int2(x, y), points);
        }

        [Test]
        public void Add()
        {
            var r1 = IntRect.FromExtents(0, 0, 10, 10);
            var r2 = IntRect.FromExtents(1, 1, 1, 1);

            r1 += r2;

            Assert.AreEqual(new int2(1, 1), r1.Position);
            Assert.AreEqual(new int2(11, 11), r1.Max);
        }

        [Test]
        public void Intersect()
        {
            var r1 = IntRect.FromExtents(0, 0, 10, 10);
            var r2 = IntRect.FromExtents(5, 5, 10, 10);
            var r3 = IntRect.FromExtents(100, 100, 5, 5);

            Assert.IsTrue(r1.Intersect(r2));
            Assert.IsFalse(r1.Intersect(r3));
        }

        [Test]
        public void Center()
        {
            var r1 = IntRect.FromExtents(0, 0, 10, 10);
            Assert.AreEqual(new int2(5, 5), r1.Center);
        }
    }

}