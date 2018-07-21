using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MHArmory.AthenaAssDataSource
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HiddenAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NameAttribute : Attribute
    {
        public string Name { get; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }

    public class DataLoader<T> where T : new()
    {
        private readonly IDictionary<string, int> header = new Dictionary<string, int>();
        private readonly IList<MemberInfo> members;

        public DataLoader(string[] header)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (this.header.ContainsKey(header[i]) == false)
                    this.header.Add(header[i], i);
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            members = typeof(T).GetFields(flags)
                .Cast<MemberInfo>()
                .Concat(typeof(T).GetProperties(flags))
                .Where(m => m.GetCustomAttribute<HiddenAttribute>() == null)
                .ToList();
        }

        public T CreateObject(string[] lineData, int lineNum)
        {
            var result = new T();

            foreach (MemberInfo member in members)
            {
                NameAttribute nameAttribute = member.GetCustomAttribute<NameAttribute>();

                string name = nameAttribute?.Name ?? member.Name;

                if (header.TryGetValue(name, out int index) == false)
                {
                    Console.WriteLine($"[WARN] property '{name}' not in header");
                    continue;
                }

                if (index >= lineData.Length)
                {
                    Console.WriteLine($"[ERROR] line {lineNum}: data is shorter ({lineData.Length} columns) than {name} index ({index})");
                    continue;
                }

                string strValue = lineData[index];

                if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    if (field.FieldType == typeof(int))
                    {
                        if (int.TryParse(strValue, out int numValue) == false)
                        {
                            Console.WriteLine($"[ERROR] line {lineNum}: data of property {name} is integer but could not parse '{strValue}'");
                            continue;
                        }

                        field.SetValue(result, numValue);
                    }
                    else
                    {
                        field.SetValue(result, strValue);
                    }
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.PropertyType == typeof(int))
                    {
                        if (int.TryParse(strValue, out int numValue) == false)
                        {
                            Console.WriteLine($"[ERROR] line {lineNum}: data of property {name} is integer but could not parse '{strValue}'");
                            continue;
                        }

                        property.SetValue(result, numValue);
                    }
                    else
                    {
                        property.SetValue(result, strValue);
                    }
                }
            }

            return result;
        }
    }
}
