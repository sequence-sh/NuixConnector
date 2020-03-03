using System;
using System.Linq;

namespace NuixClientConsole
{
    public interface IParameter
    {
        string Name { get; }

        string Summary { get; }

        Type Type { get; }

        public string TypeName => GetFullName(Type);

        bool Required { get; }


        private static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var typeName = t.Name.Split("`")[0];

            var arguments = $"<{string.Join(",", t.GetGenericArguments().Select(GetFullName))}>";

            return typeName + arguments;
        }

    }
}