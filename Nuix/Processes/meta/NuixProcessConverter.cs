using System.Collections.Generic;
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
        public Result<ImmutableProcess<T>> TryConvert<T>(ImmutableProcess<T> immutableProcess, IProcessSettings processSettings)
        {
            if (!(processSettings is INuixProcessSettings ns))
            {
                return Result.Failure<ImmutableProcess<T>>("Process settings are not Nuix Process Settings");
            }

            if (immutableProcess is ImmutableProcess<Unit> ipu)
            {
                var r = AsImmutableRubyScriptProcess(ipu, ns);
                if(r != null)
                    return Result.Success(immutableProcess);
            }
            else
            {
                var r = AsImmutableRubyScriptProcessTyped(immutableProcess, ns);
                if(r != null)
                    return Result.Success(immutableProcess);
            }
            

            return Result.Failure<ImmutableProcess<T>>("Could not convert");
        }

        private static ImmutableRubyScriptProcess? AsImmutableRubyScriptProcess(ImmutableProcess<Unit> process,
            INuixProcessSettings ns)
        {
            if (process is ImmutableRubyScriptProcess immutableRubyScriptProcess) return immutableRubyScriptProcess;

            if (process is DoNothing)
            {
                return new ImmutableRubyScriptProcess(new List<IUnitRubyBlock>(), ns);
            }

            if (process is Conditional<Unit> conditional)
            {
                var nuixIf = AsImmutableRubyScriptProcessTyped(conditional.If, ns);
                if (nuixIf != null)
                {
                    var nuixThen = AsImmutableRubyScriptProcess(conditional.Then, ns);
                    if (nuixThen != null)
                    {
                        var nuixElse = AsImmutableRubyScriptProcess(conditional.Else, ns);
                        if(nuixElse != null)
                        {
                            var r = new ConditionalRubyBlock(nuixIf.RubyBlock, nuixThen, nuixElse);

                            return new ImmutableRubyScriptProcess(new List<IUnitRubyBlock> {r}, ns);
                        }
                    }
                }
            }

            return null;
        }


        private static ImmutableRubyScriptProcessTyped<T>? AsImmutableRubyScriptProcessTyped<T>(ImmutableProcess<T> process,
            INuixProcessSettings ns)
        {
            if (process is Conditional<T> conditional)
            {
                var nuixIf = AsImmutableRubyScriptProcessTyped(conditional.If, ns);
                if (nuixIf != null)
                {
                    var nuixThen = AsImmutableRubyScriptProcessTyped<T>(conditional.Then, ns);
                    if (nuixThen != null)
                    {
                        var nuixElse = AsImmutableRubyScriptProcessTyped<T>(conditional.Else, ns);
                        if(nuixElse != null)
                        {
                            var r = new TypedConditionalRubyBlock<T>(nuixIf.RubyBlock, nuixThen, nuixElse);

                            return new ImmutableRubyScriptProcessTyped<T>(r, ns, nuixThen.TryParseFunc);
                        }
                    }
                }
            }

            return null;
        }
    }
}
