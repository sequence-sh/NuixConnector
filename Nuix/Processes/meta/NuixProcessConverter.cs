using System;
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


            return null;

            if (process is DoNothing)
            {
                return new ImmutableRubyScriptProcess(ns, new List<IUnitRubyBlock>());
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
                            //return new ImmutableRubyScriptProcess?(ns,
                            //    new List<IRubyBlock>()
                            //    {
                            //        nuixIf.
                            //    }
                                
                                
                            //    );
                        }
                    }
                }
            }
        }


        private static ImmutableRubyScriptProcessTyped<T>? AsImmutableRubyScriptProcessTyped<T>(ImmutableProcess<T> p,
            INuixProcessSettings ns)
        {
            return null;
        }
    }
}
