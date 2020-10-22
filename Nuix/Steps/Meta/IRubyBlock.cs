using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// A function that can be run within Ruby
    /// </summary>
    public interface IRubyFunction
    {
        /// <summary>
        /// The name of this function. Should be unique.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        /// The text of this function. Does not include the header.
        /// </summary>
        string FunctionText { get; }


        /// <summary>
        /// Whether this function requires the utilities argument.
        /// </summary>
        bool RequireUtilities { get; }

        /// <summary>
        /// Arguments other than utilities.
        /// </summary>
        IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

        /// <summary>
        /// Required nuix version.
        /// </summary>
        public Version RequiredNuixVersion { get; }

        /// <summary>
        /// Required nuix features.
        /// </summary>
        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }
    }

    /// <summary>
    /// A ruby function that returns a particular type or a unit.
    /// </summary>
    public interface IRubyFunction<T> : IRubyFunction
    { }

    /// <summary>
    /// A ruby function that returns a particular type or a unit.
    /// </summary>
    public class RubyFunction<T> : IRubyFunction<T>
    {
        /// <summary>
        /// Create a new RubyFunction
        /// </summary>
        public RubyFunction(string functionName, string functionText, bool requireUtilities, IReadOnlyCollection<RubyFunctionParameter> arguments)
        {
            FunctionName = functionName;
            FunctionText = functionText;
            RequireUtilities = requireUtilities;
            Arguments = arguments;
        }

        /// <inheritdoc />
        public string FunctionName { get; }

        /// <inheritdoc />
        public string FunctionText { get; }

        /// <inheritdoc />
        public bool RequireUtilities { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

        /// <inheritdoc />
        public Version RequiredNuixVersion { get; set; } = NuixVersionHelper.DefaultRequiredVersion;

        /// <inheritdoc />
        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; set; } = new List<NuixFeature>();
    }


    /// <summary>
    /// A block of ruby code that will be called by this script.
    /// </summary>
    public interface IRubyBlock
    {
        /// <summary>
        /// The name of this ruby block.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The functions that this block of code is dependent on.
        /// </summary>
        IEnumerable<IRubyFunction> FunctionDefinitions { get; }

        /// <summary>
        /// Gets the arguments that need to be passed to the nuix step.
        /// Will generally include the argument name (prefixed by '--' and the argument).
        /// Will fail if some required arguments are missing.
        /// </summary>
        Result<IReadOnlyCollection<string>, IErrorBuilder> TryGetArguments(Suffixer suffixer);

        /// <summary>
        /// Gets the lines needed in the options parser to provide the arguments.
        /// </summary>
        void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer);
    }

    /// <summary>
    /// A block of ruby code with no return type.
    /// </summary>
    public interface IUnitRubyBlock : IRubyBlock
    {
        /// <summary>
        /// Writes the main block of ruby code.
        /// </summary>
        public Result<Unit, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder);
    }

    /// <summary>
    /// A block of ruby code that creates a variable.
    /// </summary>
    public interface ITypedRubyBlock : IRubyBlock
    {
        /// <summary>
        /// Writes the main block of ruby code.
        /// Returns the result variable name.
        /// </summary>
        public Result<string, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder);
    }


    /// <summary>
    /// A block of ruby code that creates a variable with a particular return type.
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public interface ITypedRubyBlock<T> : ITypedRubyBlock { }
}