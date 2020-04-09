using System.Collections.Generic;
using System.IO;
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
                if (r != null)
                {
                    var result = Result.Success<ImmutableProcess<Unit>>(r);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse //ReSharper basically gets this wrong
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if(result is Result<ImmutableProcess<T>> castResult)
                        // ReSharper disable once HeuristicUnreachableCode
                        return castResult;
                }
                     
            }
            else
            {
                var r = AsImmutableRubyScriptProcessTyped(immutableProcess, ns);
                if(r != null)
                    return Result.Success<ImmutableProcess<T>>(r);
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

            if (process is Utilities.Processes.immutable.AssertBool assertBool)
            {
                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(assertBool.SubProcess, ns);

                if (nuixSubprocess != null)
                    return new ImmutableRubyScriptProcess(
                        new List<IUnitRubyBlock> { new AssertBoolRubyBlock(nuixSubprocess.RubyBlock, assertBool.ExpectedResult) }, ns);
            }

            if (process is Utilities.Processes.immutable.AssertError assertError)
            {
                var nuixSubProcess = AsImmutableRubyScriptProcess(assertError.SubProcess, ns);

                if (nuixSubProcess != null)
                    return new ImmutableRubyScriptProcess(
                            new List<IUnitRubyBlock> { new AssertErrorRubyBlock(nuixSubProcess) }, ns);
            }

            if (process is Utilities.Processes.immutable.WriteFile writeFile)
            {
                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(writeFile.TextProcess, ns);

                if (nuixSubprocess != null)
                {
                    var fullPath = Path.Combine(writeFile.Folder, writeFile.FileName);
                    var writeBlock = new WriteFileRubyBlock(fullPath, nuixSubprocess.RubyBlock);

                    var p = new ImmutableRubyScriptProcess(new []{writeBlock}, ns);

                    return p;
                }
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
            if (process is ImmutableRubyScriptProcessTyped<T> immutableRubyScriptProcessTyped) return immutableRubyScriptProcessTyped;

            if (process is Utilities.Processes.immutable.CheckNumber checkNumber)
            {
                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(checkNumber.CountProcess, ns);

                if (nuixSubprocess != null)
                {
                    var checkNumberBlock = new CheckNumberRubyBlock(nuixSubprocess.RubyBlock, checkNumber.Minimum, checkNumber.Maximum);

                    var p = new ImmutableRubyScriptProcessTyped<bool>(checkNumberBlock, ns, RubyScriptProcess.TryParseBool);

                    if (p is ImmutableRubyScriptProcessTyped<T> resultProcess)
                        return resultProcess;
                }
            }

            if (process is Conditional<T> conditional)
            {
                var nuixIf = AsImmutableRubyScriptProcessTyped(conditional.If, ns);
                if (nuixIf != null)
                {
                    var nuixThen = AsImmutableRubyScriptProcessTyped(conditional.Then, ns);
                    if (nuixThen != null)
                    {
                        var nuixElse = AsImmutableRubyScriptProcessTyped(conditional.Else, ns);
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
