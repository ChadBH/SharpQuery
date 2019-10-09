using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpQuery.Tests.Data;

namespace SharpQuery.Tests
{
    [TestClass]
    public class Basics
    {
        [TestMethod]
        public void Set()
        {
            var result = new Query<TestObject>(new TestObject())
               .Set("Nested.NestedStrings", "[\"abc\", \"123\"]")
               .Result;

            Assert.IsTrue(result.Nested.NestedStrings.Any());

            var nested = new Query<NestedObject>(new NestedObject())
                .Set("NestedStrings", "[\"abc\", \"123\"]")
                .Result;

            Assert.IsTrue(nested.NestedStrings.Any());

            var ints = new Query<TestObject>(new TestObject())
                .Set("Nested.NestedInts", "[1, 2]")
                .Result;

            Assert.IsTrue(ints.Nested.NestedInts.Any());

            var list = new Query<TestObject>(new TestObject())
                .Set(new Dictionary<string, string> {
                    {"Nested.NestedStrings", "[\"hello\", \"world\"]" },
                    {"Nested.NestedInts", "[1, 2]" }
                })
                .Result;

            Assert.IsTrue(list.Nested.NestedStrings.Any());
            Assert.IsTrue(list.Nested.NestedInts.Any());

            var query4 = new Query<TestObject>(new TestObject());
            var fromJson = query4.Set("Nested", "{\"NestedStrings\": [\"1\", \"2\"]}").Result;
            Assert.IsTrue(fromJson.Nested.NestedStrings.Any());
        }

        [TestMethod]
        public void Query()
        {
            var testObj = new TestObject("1", "2", "3");
            var query = new Query<TestObject>(testObj);
            var match = query.Where("Nested.NestedStrings.Any(ns => ns == \"1\")").Result;
            Assert.IsNotNull(match);

            var query2 = new Query<TestObject>(testObj);
            var noMatch = query2.Where("Nested.NestedStrings.Any(ns => ns == \"4\")").Result;
            Assert.IsNull(noMatch);

            var query3 = new Query<TestObject>(new TestObject(1, 2, 3, 4));
            var intMatch = query3.Where("Nested.NestedInts.Any(i => i < 4)").Result;
            Assert.IsNotNull(intMatch);
            var noIntMatch = query3.Where("Nested.NestedInts.Any(i => i > 4)").Result;
            Assert.IsNull(noIntMatch);

        }
    }
}