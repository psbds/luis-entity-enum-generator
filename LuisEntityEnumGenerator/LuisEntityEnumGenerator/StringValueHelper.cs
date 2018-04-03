using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LuisEntityEnumGenerator
{
    public static class StringValueHelper
    {
        public static string GetStringValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            EntityKeyAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(EntityKeyAttribute), false) as EntityKeyAttribute[];
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }

    public class EntityKeyAttribute : Attribute
    {
        public string StringValue { get; protected set; }

        public EntityKeyAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
