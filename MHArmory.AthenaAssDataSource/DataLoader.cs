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
        private readonly ILogger logger;
        private readonly IDictionary<string, int> header = new Dictionary<string, int>();
        private readonly IList<MemberInfo> members;

        public DataLoader(string[] header, ILogger logger)
        {
            this.logger = logger;

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

                string[] names = nameAttribute?.Names;

                if (names == null || names.Length == 0)
                    names = new[] { member.Name };

                int index = -1;

                foreach (string name in names)
                {
                    if (header.TryGetValue(name, out index) == false)
                    {
                        index = -1;
                        continue;
                    }
                    break;
                }

                if (index < 0)
                {
                    logger?.LogError($"could not find header with '{string.Join(", ", names)}'");
                    break;
                }

                if (index >= lineData.Length)
                {
                    logger?.LogError($"line {lineNum}: data is shorter ({lineData.Length} columns) than {string.Join("/", names)} index ({index})");
                    continue;
                }

                string strValue = lineData[index];

                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    if (field.FieldType == typeof(int))
                    {
                        if (strValue == string.Empty)
                            field.SetValue(result, 0);
                        else
                        {
                            if (int.TryParse(strValue, out int numValue) == false)
                            {
                                logger?.LogError($"line {lineNum}: data of property {string.Join("/", names)} is integer but could not parse '{strValue}'");
                                continue;
                            }

                            field.SetValue(result, numValue);
                        }
                    }
                    else
                    {
                        field.SetValue(result, strValue);
                    }
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    if (property.PropertyType == typeof(int))
                    {
                        if (strValue == string.Empty)
                            property.SetValue(result, 0);
                        else
                        {
                            if (int.TryParse(strValue, out int numValue) == false)
                            {
                                logger?.LogError($"line {lineNum}: data of property {string.Join("/", names)} is integer but could not parse '{strValue}'");
                                continue;
                            }

                            property.SetValue(result, numValue);
                        }
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
