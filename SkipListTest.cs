using System;
using NUnit.Framework;
using SkipList;

namespace SkipListTest
{
    [TestFixture]
    public class SkipListTest
    {
        SkipList<String, String> sl;
        SkipList<String, int> sl2;

        [SetUp]
        protected void Setup()
        {
            sl = new SkipList<String, String>();
            sl2 = new SkipList<String, int>();
        }

        [Test]
        public void HoldsValues()
        {
            sl["foo"] = "fie";
            Assert.AreEqual(sl["foo"], "fie");
            sl2["fum"] = 2343;
            Assert.AreEqual(sl2["fum"], 2343);
        }
    }
}
