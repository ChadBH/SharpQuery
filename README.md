SharpQuery is a simple library for querying and setting values on an object using string inputs. Given the following object:

           var helloWorld = new MyClass {
              String1 = "Hello",
              String2 = "World"
            };

            var halloWorld = new Query<MyClass>(helloWorld)
                .If("String1 == \"Hello\"")
                .Set("String1", "Hallo")
                .Result;

            Assert.AreEqual("Hallo", halloWorld.String1);

It's useful when your business rules tend to be a barrage of different types of conditional logic on a particular object, and you'd benefit from a runtime way to do that rather than hardcoding a ton of if/else statements that are very customer-specific. You can store these strings in a database, for example, and do basic logic that you'd normally have to hardcode.

Under the hood, `.If()` is just dynamic Linq, so anything that works in dynamic Linq will work here.

You can exclude the `.If()` if you want to use `Set()` without conditions.

You can also exclude the `Set()` if you just care about checking some condition on an object. `.Result` will be null or you can use `.HasMatch` in place of it to get a boolean for whether the object meets the condition.

.Set supports dot notation if your fields are nested, and your second parameter can be JSON to set more complex objects.

If your object implements ICloneable, the original object will remain unchanged and the `.Result` will be a clone with the relevant fields changed. Otherwise, `.Result` is a reference to the original object, so it will be changed. If you need to preserve the old fields on the input object make it implement ICloneable.