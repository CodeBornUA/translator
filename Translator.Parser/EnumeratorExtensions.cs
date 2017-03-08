using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parser
{
    public static class EnumeratorCloner
    {
        public static IEnumerator<T> Clone<T>(this IEnumerator<T> source) where T : class
        {
            var sourceType = source.GetType().UnderlyingSystemType;
            var sourceTypeConstructor = sourceType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(List<T>)}, null);

            var nonPublicFields = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var publicFields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var newInstance = sourceTypeConstructor.Invoke(new object[] { nonPublicFields.First(x => x.Name == "list").GetValue(source) }) as IEnumerator<T>;
            foreach (var field in nonPublicFields)
            {
                var value = field.GetValue(source);
                field.SetValue(newInstance, value);
            }
            foreach (var field in publicFields)
            {
                var value = field.GetValue(source);
                field.SetValue(newInstance, value);
            }
            return newInstance;
        }
    }
}
