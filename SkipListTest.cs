using System;
using NUnit.Framework;
using SkipList;

namespace SkipListTest
{
    [TestFixture]
    public class SkipListTest
    {
        SkipList<String, String> sl;

        [SetUp]
        protected void Setup()
        {
            sl = new SkipList<String, String>();
        }

        [Test]
        public void HoldsValue()
        {
            sl.add("foo", "fie");
            Assert.AreEqual(sl.get("foo"), "fie");
        }
    }
}
