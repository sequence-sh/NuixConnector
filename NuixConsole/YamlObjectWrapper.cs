﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using Reductech.Utilities.InstantConsole;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    public class YamlObjectWrapper : IDocumented
    {
        private readonly Type _processType;

        public YamlObjectWrapper(Type processType, DocumentationCategory category)
        {
            DocumentationCategory = category;
            _processType = processType;

            RelevantProperties = processType.GetProperties()
                .Select(p=> (p, p.GetCustomAttribute<YamlMemberAttribute>()))
                .Where(x=>x.Item2 != null)
                .OrderBy(x=>x.Item2?.Order)
                .ThenBy(x=>x.p.Name).Select(x=>x.p).ToList();


            var instance = Activator.CreateInstance(processType);
            Parameters = RelevantProperties.Select(propertyInfo => 
                new PropertyWrapper(propertyInfo, propertyInfo.GetValue(instance)?.ToString()  )).ToList();
        }
        
        /// <inheritdoc />
        public DocumentationCategory DocumentationCategory { get; }

        /// <inheritdoc />
        public string Name => _processType.Name;

        /// <inheritdoc />
        public string Summary => _processType.GetXmlDocsSummary();


        protected IEnumerable<PropertyInfo> RelevantProperties { get; }

        /// <inheritdoc />
        public IEnumerable<IParameter> Parameters { get; }


        protected class PropertyWrapper : IParameter
        {
            private readonly PropertyInfo _propertyInfo;

            public PropertyWrapper(PropertyInfo propertyInfo, string? defaultValueString)
            {
                _propertyInfo = propertyInfo;
                Required = _propertyInfo.CustomAttributes.Any(att => att.AttributeType == typeof(RequiredAttribute)) && defaultValueString == null;
                DefaultValueString = defaultValueString;
            }

            public string Name => _propertyInfo.Name;
            public string Summary => _propertyInfo.GetXmlDocsSummary();
            public Type Type => _propertyInfo.PropertyType;
            public bool Required { get; }
            public string? DefaultValueString { get; }
        }
    }
}