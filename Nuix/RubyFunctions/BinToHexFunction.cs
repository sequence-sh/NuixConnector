using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reductech.EDR.Connectors.Nuix.processes.meta;

namespace Reductech.EDR.Connectors.Nuix.RubyFunctions
{
    /// <summary>
    /// A function for converting values to hex for returning them as outputs from Nuix.
    /// </summary>
    public sealed class BinToHexFunction : IRubyFunction<string>
    {
        private BinToHexFunction() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IRubyFunction<string> Instance { get; } = new BinToHexFunction();

        /// <inheritdoc />
        public string FunctionName => "BinToHex";

        /// <inheritdoc />
        public string FunctionText { get; } =
            @"suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
'0x' + suffix";

        /// <inheritdoc />
        public bool RequireUtilities => false;

        /// <inheritdoc />
        public IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } =
            new[] {new RubyFunctionParameter("s", false)};

        /// <inheritdoc />
        public Version RequiredNuixVersion { get; }= new Version(5, 0);

        /// <inheritdoc />
        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => ImmutableList<NuixFeature>.Empty;
    }

}