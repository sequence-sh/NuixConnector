using System;
using System.ComponentModel;

namespace Orchestration
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    public static class EnumMethods
    {
        /// <summary>
        /// Gets the description of an enum value from the Description Attribute.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return value.ToString();
            var field = type.GetField(name);
            if (field == null) return value.ToString();
            var attr =
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (attr is DescriptionAttribute da)
            {
                return da.Description;
            }
            return value.ToString();
        }
    }
}
