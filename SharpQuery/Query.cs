using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Newtonsoft.Json;

namespace SharpQuery
{
    public class Query<T>
    {

        public IQueryable<T> Results;

        private readonly bool _throwExceptions;

        public T Result
        {
            get
            {
                return Results.FirstOrDefault();
            }
        }

        public bool HasMatch
        {
            get
            {
                return Result != null;
            }
        }

        private Query()
        {
            Results = new List<T>().AsQueryable();
        }

        private Query(bool throwExceptions) : this()
        {
            _throwExceptions = throwExceptions;
        }

        public Query(T obj, bool throwExceptions = false) : this(throwExceptions)
        {
            Results = new List<T> { obj }.AsQueryable();
        }

        public Query(
            IQueryable<T> objects,
            bool throwExceptions = false
        ) : this(throwExceptions)
        {
            Results = objects;
        }

        private Query(Query<T> prior) : this()
        {
            if (prior == null) return;
            _throwExceptions = prior._throwExceptions;
            Results = prior.Results;
        }

        private Query(Query<T> prior, IQueryable<T> results) : this(prior)
        {
            Results = results;
        }

        public Query<T> If(string expression)
        {
            //if no expression provided, match nothing
            if (expression == null) return new Query<T>(this);

            if (_throwExceptions)
            {
                return new Query<T>(this, Clones().Where(expression));
            }
            try
            {
                return new Query<T>(this, Clones().Where(expression));
            }
            catch (Exception e)
            {
                return new Query<T>(this, Clones());
            }
        }

        public Query<T> If(List<string> expressions)
        {
            //if no expressions provided, match nothing
            if (expressions == null || !expressions.Any()) return new Query<T>(this);

            Query<T> toReturn = this;

            expressions.ForEach(expression => {
                toReturn = toReturn.If(expression);
            });

            return toReturn;
        }

        public Query<T> Set(string property, string value)
        {
            var clones = Clones()
                .ToList()
                .Select(clone => {
                    var exception = Set(clone, property, value);
                    if (exception != null)
                    {
                        if (_throwExceptions)
                        {
                            throw exception;
                        }
                    }
                    return clone;
                }).AsQueryable();

            return new Query<T>(this, clones);
        }

        public Query<T> Set<TValue>(string property, TValue value)
        {
            var clones = Clones()
                .ToList()
                .Select(clone => {
                    var exception = Set(clone, property, value);
                    if (exception != null)
                    {
                        if (_throwExceptions)
                        {
                            throw exception;
                        }
                    }
                    return clone;
                }).AsQueryable();

            return new Query<T>(this, clones);
        }

        public Query<T> Set(ICollection<KeyValuePair<string, object>> propertyValues)
        {
            if (propertyValues == null || !propertyValues.Any()) return this;

            var toReturn = this;

            propertyValues
                .ToList()
                .ForEach(pv => {
                    toReturn = toReturn.Set(pv.Key, pv.Value);
                });

            return toReturn;
        }

        public Query<T> SetIf(
            List<string> whereClauses,
            ICollection<KeyValuePair<string, object>> propertyValues
        )
        {
            return If(whereClauses)
                .Set(propertyValues);
        }

        private IQueryable<T> Clones()
        {
            return Clones(Results);
        }

        private static IQueryable<T> Clones(IQueryable<T> objects)
        {
            if (typeof(T).IsAssignableFrom(typeof(ICloneable)))
            {
                //make clones so the original isn't mutated
                return objects.Select(o => (T)(o as ICloneable).Clone());
            }
            //fall back on pass by reference
            //todo: warn that you're doing this
            return objects;
        }

        private static Exception Set<TValue>(T target, string compoundProperty, TValue value)
        {
            try
            {
                if (target == null || string.IsNullOrWhiteSpace(compoundProperty)) return null;

                var targetObject = (object)target;

                string[] bits = compoundProperty.Split('.');
                for (int i = 0; i < bits.Length - 1; i++)
                {
                    PropertyInfo propertyToGet = targetObject.GetType().GetProperty(bits[i]);

                    if (propertyToGet == null)
                    {
                        throw new ArgumentException($"No {bits[i]} exists in {typeof(T).Name}.{compoundProperty}, check your spelling", "compoundProperty");
                    }

                    targetObject = propertyToGet.GetValue(targetObject, null);
                }

                PropertyInfo propertyToSet = targetObject.GetType().GetProperty(bits.Last());

                if (typeof(TValue) != propertyToSet.PropertyType)
                {
                    if (value is string && propertyToSet.PropertyType != typeof(string))
                    {
                        if (IsJson(value.ToString()))
                        {
                            var deserialized = JsonConvert.DeserializeObject(value.ToString(), propertyToSet.PropertyType);
                            propertyToSet.SetValue(targetObject, deserialized);
                            return null;
                        }
                    }
                }

                propertyToSet.SetValue(targetObject, value);

            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        private static bool IsJson(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && (input.Contains("{") || input.Contains("["));
        }
    }
}