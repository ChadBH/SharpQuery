using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

using Newtonsoft.Json;

namespace SharpQuery
{
    public class Query<T>{

        public IQueryable<T> Results;
        public T Result { get {
            return Results.FirstOrDefault();
        } }

        public bool HasMatch
        { get {
            return Result != null;
        } }

        private Query()
        {
            Results = new List<T>().AsQueryable();
        }

        public Query(T obj):this()
        {
            Results = new List<T> { obj }.AsQueryable();
        }

        public Query(IQueryable<T> objects)
        {
            Results = objects;
        }

        public Query<T> Where(string expression)
        {
            //if no expression provided, match nothing
            if (expression == null) return new Query<T>();
            return new Query<T>(Clones().Where(expression));
        }

        public Query<T> Where(List<string> expressions)
        {
            //if no expressions provided, match nothing
            if (expressions == null || !expressions.Any()) return new Query<T>();

            Query<T> toReturn = this;

            expressions.ForEach(expression => {
                toReturn = toReturn.Where(expression);
            });

            return toReturn;
        }

        public Query<T> Set(string property, string value)
        {
            var clones = Clones()
                .ToList()
                .Select(clone => {
                    Set(clone, property, value);
                    return clone;
                }).AsQueryable();

            return new Query<T>(clones);
        }

        public Query<T> Set(ICollection<KeyValuePair<string, string>> propertyValues)
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

        public Query<T> SetWhere(List<string> whereClauses, List<KeyValuePair<string, string>> propertyValues)
        {
            return Where(whereClauses)
                .Set(propertyValues);
        }

        private IQueryable<T> Clones()
        {
            return Clones(Results);
        }

        private static IQueryable<T> Clones(IQueryable<T> objects) {

            if( typeof(T).IsAssignableFrom(typeof(ICloneable)))
            {
                //make clones so the original isn't mutated
                return objects.Select(o => (T)(o as ICloneable).Clone());
            }
            //fall back on pass by reference
            //todo: warn that you're doing this
            return objects;
        }

        private static void Set(object target, string compoundProperty, string value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            var deserialized = JsonConvert.DeserializeObject(value, propertyToSet.PropertyType);
            propertyToSet.SetValue(target, deserialized, null);
        }
    }
}
