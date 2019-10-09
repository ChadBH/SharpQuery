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

        [TestMethod]
        public void SimpleTypes()
        {
            var helloWorld = new MyClass {
              String1 = "Hello",
              String2 = "World"
            };

            var halloWorld = new Query<MyClass>(helloWorld)
                .Where("String1 == \"Hello\"")
                .Set("String1", "Hallo")
                .Result;

            Assert.AreEqual("Hallo", halloWorld.String1);
        }

        public class MyClass
        {
            public string String1 { get; set; }
            public string String2 { get; set; }
        }
    }
}