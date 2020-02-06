using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Namotion.Reflection;
using Razor_Components.Inputs;

namespace Razor_Components
{
    internal static class ParameterExtensions
    {
        internal static IParameterInput GetParameterInfo(ParameterInfo parameterInfo)
        {
            //TODO handle other parameters
            //TODO collections
            
            var remarks = parameterInfo.GetXmlDocs(); //Note: for remarks to work, you may need to modify the csproj file

            var type = parameterInfo.ParameterType;

            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            if (isNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                type = underlyingType ?? throw new ArgumentException("Problem converting isParameterNullable type.");
            }
            if (parameterInfo.Name == null)
                return new UnhandledInput("Unknown Parameter", remarks, "Parameter has no name" );

            if (TypesToIgnore.Contains(type))
            {
                return new NoInput(parameterInfo.Name, remarks);
            }
            else if (type == typeof(bool))
            {
                return new CheckboxInput(parameterInfo.Name, remarks, isNullable);
            }
            else if (ParameterConverterDictionary.TryGetValue(type, out var converter))
            {
                return new InputInput(parameterInfo.Name, remarks, converter, isNullable);
            }
            else if (type.BaseType != null && type.BaseType == typeof(Enum))
            {
                var enumConverter = new EnumConverter(type);
                var enumValues = Enum.GetNames(type);

                return new DropdownInput(parameterInfo.Name, remarks, enumConverter, enumValues, isNullable);
            }
            else if (type.IsAssignableFrom(typeof(List<string>)))
            {
                return new TextAreaInput(parameterInfo.Name, remarks, new StringListConverter());
            }

            return new UnhandledInput(parameterInfo.Name, remarks, $"Could not handle parameter of type '{type}'");
        }

        private static readonly ISet<Type> TypesToIgnore = new HashSet<Type>()
        {
            typeof(CancellationToken)
        };

        private static readonly IReadOnlyDictionary<Type, IConverter> ParameterConverterDictionary =

            new ReadOnlyDictionary<Type, IConverter>(
                new IConverter[]
                {
                    new GenericConverter<int>(i=> (int.TryParse(i, out var r), r), "number", null ), 
                    new GenericConverter<uint>(i=> (uint.TryParse(i, out var r), r), "number", null), 
                    new GenericConverter<long>(i=> (long.TryParse(i, out var r), r), "number", null), 
                    new GenericConverter<decimal>(i=> (decimal.TryParse(i, out var r), r), "text", @"[\d]*\.?[\d]*"), 
                    new GenericConverter<double>(i=> (double.TryParse(i, out var r), r), "text", @"[\d]*\.?[\d]*"), 
                    new GenericConverter<float>(i=> (float.TryParse(i, out var r), r), "text", @"[\d]*\.?[\d]*"), 
                    new GenericConverter<Guid>(i=> (Guid.TryParse(i, out var r), r), "text", @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}"), 
                    
                    new GenericConverter<char>(i=> (char.TryParse(i, out var r), r), "text", @"[A_Za-z]"),
                    new StringConverter(), 
                }.ToDictionary(x=>x.ParameterType)
            );
    }
}