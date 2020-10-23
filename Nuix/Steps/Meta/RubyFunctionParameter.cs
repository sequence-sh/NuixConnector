using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// The argument to a ruby function.
    /// </summary>
    public readonly struct RubyFunctionParameter : IEquatable<RubyFunctionParameter>
    {
        /// <summary>
        /// Creates a new RubyFunctionParameter.
        /// </summary>
        public RubyFunctionParameter(string parameterName, bool isOptional, Version? requiredNuixVersion)
        {
            ParameterName = parameterName;
            IsOptional = isOptional;
            RequiredNuixVersion = requiredNuixVersion;
        }

        /// <inheritdoc />
        public override string ToString() => ParameterName;

        /// <summary>
        /// The name of the argument.
        /// Should be lower case as per style guidelines.
        /// The arguments to a function should have unique names.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// False if this argument is required.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The required Nuix version for this parameter.
        /// </summary>
        public Version? RequiredNuixVersion { get; }

        /// <inheritdoc />
        public bool Equals(RubyFunctionParameter other) => ParameterName == other.ParameterName;


        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is RubyFunctionParameter other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => ParameterName.GetHashCode();

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(RubyFunctionParameter left, RubyFunctionParameter right) => left.Equals(right);

        /// <summary>
        /// Not equals operator
        /// </summary>
        public static bool operator !=(RubyFunctionParameter left, RubyFunctionParameter right) => !left.Equals(right);


        /// <summary>
        /// Gets a dictionary mapping ruby script function parameters to their arguments.
        /// </summary>
        public static IReadOnlyDictionary<RubyFunctionParameter, IStep?> GetRubyFunctionArguments<TStep>(TStep process)
            where TStep : IRubyScriptStep
        {
            var dict = new Dictionary<RubyFunctionParameter, IStep?>();

            foreach (var p in process.GetType().GetProperties())
            {
                var argumentAttribute = p.GetCustomAttribute<RubyArgumentAttribute>();

                if (argumentAttribute != null)
                {
                    var (isRunnableProcess, isNullable) = CheckType(p);

                    if (isRunnableProcess)
                    {
                        var version = p.GetCustomAttributes<RequiredVersionAttribute>()
                        .Where(x => x.SoftwareName.Equals("Nuix", StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.RequiredVersion)
                        .FirstOrDefault();

                        var parameter = new RubyFunctionParameter(argumentAttribute.RubyName, isNullable, version);

                        var value = p.GetValue(process);

                        if (value is IStep rp)
                            dict.Add(parameter, rp);
                        else
                            dict.Add(parameter, null);
                    }
                }
            }

            return dict;
        }


        /// <summary>
        /// Gets the function parameters of a ruby script step in the correct order.
        /// </summary>
        public static IReadOnlyCollection<RubyFunctionParameter> GetRubyFunctionParameters<TStep>() where TStep : IRubyScriptStep
        {
            var list = new List<(RubyFunctionParameter argument, int order)>();

            foreach (var p in typeof(TStep).GetProperties())
            {
                var argumentAttribute = p.GetCustomAttribute<RubyArgumentAttribute>();

                var (isRunnableProcess, isNullable) = CheckType(p);

                if (argumentAttribute != null)
                {
                    var version = p.GetCustomAttributes<RequiredVersionAttribute>()
                        .Where(x => x.SoftwareName.Equals("Nuix", StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.RequiredVersion)
                        .FirstOrDefault();


                    if (isRunnableProcess)
                        list.Add((new RubyFunctionParameter(argumentAttribute.RubyName, isNullable, version),
                            argumentAttribute.Order));
                    else

                        throw new ApplicationException($"{p.Name} in {typeof(TStep).Name} is not assignable to IStep");
                }
                else if (isRunnableProcess)
                {
                    throw new ApplicationException($"{p.Name} in {typeof(TStep).Name} is missing RubyArgumentAttribute");
                }
            }

            return list.OrderBy(x => x.order).Select(x => x.argument).ToList();

        }

        private static (bool isRunnableProcess, bool isOptional) CheckType(PropertyInfo propertyInfo)
        {
            var isOptional = propertyInfo.GetCustomAttribute<RequiredAttribute>() == null;

            if (typeof(IStep).IsAssignableFrom(propertyInfo.PropertyType))
                return (true, isOptional);

            return (false, isOptional);
        }
    }
}