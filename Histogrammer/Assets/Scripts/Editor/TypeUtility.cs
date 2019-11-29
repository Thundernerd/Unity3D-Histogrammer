using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNRD.Histogrammer
{
    public class TypeUtility
    {
        public static List<FieldInfo> GetFieldsUpTo(Type type, BindingFlags bindingAttr, Type stopWhenReached)
        {
            List<FieldInfo> fields = new List<FieldInfo>();

            while (type != null && type != stopWhenReached)
            {
                fields.AddRange(type.GetFields(bindingAttr));
                type = type.BaseType;
            }

            return fields;
        }

        public static bool IsEnumerable(object value)
        {
            if (value == null)
                return false;

            Type type = value.GetType();
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool HasCustomAttribute<T>(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(typeof(T), false).Any();
        }
    }
}
