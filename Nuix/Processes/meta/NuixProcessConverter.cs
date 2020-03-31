using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class NuixProcessConverter : IProcessConverter
    {
        public static NuixProcessConverter Instance = new NuixProcessConverter();

        private NuixProcessConverter()
        {
        }


        /// <inheritdoc />
        public Result<ImmutableProcess<Unit>> TryConvert(ImmutableProcess<Unit> immutableProcess, IProcessSettings processSettings)
        {
            return Result.Failure<ImmutableProcess<Unit>>("Could not convert");
        }
    }
}
