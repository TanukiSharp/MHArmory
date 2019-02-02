using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MHArmory.AthenaAssDataSource
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HiddenAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class NameAttribute : Attribute
    {
        public string[] Names { get; }

        public NameAttribute(params string[] names)
        {
            Names = names;
        }
    }

    public class DataLoader<T> where T : new()
    {
        private delegate bool Setter(object instance, string value);

        private readonly ILogger logger;
        private readonly IDictionary<int, Setter> members = new Dictionary<int, Setter>();

        public DataLoader(string[] header, ILogger logger)
        {
            this.logger = logger;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            IList<(MemberInfo member, string[] names)> localMembers = typeof(T).GetMembers(flags)
                .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                .Where(m => m.GetCustomAttribute<HiddenAttribute>() == null)
                .Select(m => (member: m, names: m.GetCustomAttribute<NameAttribute>(true)?.Names ?? new string[] { m.Name }))
                .ToList();

            var headerUniqueness = new HashSet<string>();

            for (int i = 0; i < header.Length; i++)
            {
                (MemberInfo member, string[] names) = localMembers.FirstOrDefault(x => x.names.Contains(header[i]));

                if (member == null)
                    continue;

                if (headerUniqueness.Add(member.Name) == false)
                    continue;

                if (member is FieldInfo f)
                {
                    if (f.FieldType == typeof(int))
                        members.Add(i, (x, y) => IntFieldSetter(f, x, y));
                    else
                        members.Add(i, (x, y) => StringFieldSetter(f, x, y));
                }
                else if (member is PropertyInfo p)
                {
                    if (p.PropertyType == typeof(int))
                        members.Add(i, (x, y) => IntPropertySetter(p, x, y));
                    else
                        members.Add(i, (x, y) =>StringPropertySetter(p, x, y));
                }
            }
        }

        private bool StringFieldSetter(FieldInfo fieldInfo, object instance, string value)
        {
            try
            {
                fieldInfo.SetValue(instance, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IntFieldSetter(FieldInfo fieldInfo, object instance, string value)
        {
            try
            {
                if (value == string.Empty)
                    fieldInfo.SetValue(instance, 0);
                else
                {
                    if (int.TryParse(value, out int numValue) == false)
                        return false;

                    fieldInfo.SetValue(instance, numValue);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool StringPropertySetter(PropertyInfo propertyInfo, object instance, string value)
        {
            try
            {
                propertyInfo.SetValue(instance, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IntPropertySetter(PropertyInfo propertyInfo, object instance, string value)
        {
            try
            {
                if (value == string.Empty)
                    propertyInfo.SetValue(instance, 0);
                else
                {
                    if (int.TryParse(value, out int numValue) == false)
                        return false;

                    propertyInfo.SetValue(instance, numValue);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public T CreateObject(string[] lineData, int lineNum)
        {
            var result = new T();

            foreach (KeyValuePair<int, Setter> kv in members)
            {
                if (kv.Key >= lineData.Length)
                    continue;

                if (kv.Value(result, lineData[kv.Key]) == false)
                    logger?.LogError($"line {lineNum}: failed to parse data at column {kv.Key + 1}.");
            }

            return result;
        }
    }
}
