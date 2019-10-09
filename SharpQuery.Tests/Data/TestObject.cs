using System.Collections.Generic;
using System.Linq;

namespace SharpQuery.Tests.Data
{
    public class TestObject
    {
        public NestedObject Nested { get; set; }
        public TestObject()
        {
            Nested = new NestedObject();
        }

        public TestObject(List<string> nestedStrings):this()
        {
            Nested = new NestedObject(nestedStrings);
        }

        public TestObject(List<int> nestedInts) : this()
        {
            Nested = new NestedObject(nestedInts);
        }

        public TestObject(params string[] nestedStrings):this(nestedStrings.ToList())
        {
        }

        public TestObject(params int[] nestedInts) : this(nestedInts.ToList()) { }
    }

    public class NestedObject
    {
        public List<string> NestedStrings { get; set; }
        public List<int> NestedInts { get; set; }

        public NestedObject()
        {
            NestedStrings = new List<string>();
            NestedInts = new List<int>();
        }

        public NestedObject(List<string> nestedStrings) : this()
        {
            if (nestedStrings != null) NestedStrings = nestedStrings;
        }

        public NestedObject(List<int> nestedInts) : this()
        {
            if (nestedInts != null) NestedInts = nestedInts;
        }
    }
}

