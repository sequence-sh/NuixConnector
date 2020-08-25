//using System.Collections.Generic;
//using System.IO;
//using CSharpFunctionalExtensions;
//using Reductech.EDR.Processes;
//using Reductech.EDR.Processes.Immutable;
//using Reductech.EDR.Processes.Mutable;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta //TODO redo this
//{
//    internal sealed class NuixProcessConverter : IProcessConverter
//    {
//        public static IProcessConverter Instance = new NuixProcessConverter();

//        private NuixProcessConverter()
//        {
//        }

//        /// <inheritdoc />
//        Result<IImmutableProcess<T>> IProcessConverter.TryConvert<T>(IImmutableProcess<T> immutableProcess, IProcessSettings processSettings)
//        {
//            if (!(processSettings is INuixProcessSettings ns))
//            {
//                return Result.Failure<IImmutableProcess<T>>("Process settings are not Nuix Process Settings");
//            }

//            if (immutableProcess is IImmutableProcess<Unit> ipu)
//            {
//                var r = AsImmutableRubyScriptProcess(ipu, ns);
//                if (r != null)
//                {
//                    var result = Result.Success<IImmutableProcess<Unit>>(r);
//                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse //ReSharper basically gets this wrong
//                    // ReSharper disable once SuspiciousTypeConversion.Global
//                    if(result is Result<IImmutableProcess<T>> castResult)
//                        // ReSharper disable once HeuristicUnreachableCode
//                        return castResult;
//                }
//            }
//            else
//            {
//                var r = AsImmutableRubyScriptProcessTyped(immutableProcess, ns);
//                if(r != null)
//                    return Result.Success<IImmutableProcess<T>>(r);
//            }

//            return Result.Failure<IImmutableProcess<T>>("Could not convert");
//        }

//        private static RubyScriptProcess? AsImmutableRubyScriptProcess(IImmutableProcess<Unit> process,
//            INuixProcessSettings ns)
//        {
//            if (process is RubyScriptProcess immutableRubyScriptProcess) return immutableRubyScriptProcess;

//            if (process is DoNothing)
//            {
//                return new RubyScriptProcess(new List<IUnitRubyBlock>(), ns);
//            }

//            if (process is Processes.Immutable.AssertBool assertBool)
//            {
//                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(assertBool.SubProcess, ns);

//                if (nuixSubprocess != null)
//                    return new RubyScriptProcess(
//                        new List<IUnitRubyBlock> { new AssertBoolRubyBlock(nuixSubprocess.RubyBlock, assertBool.ExpectedResult) }, ns);
//            }

//            if (process is Processes.Immutable.AssertError assertError)
//            {
//                var nuixSubProcess = AsImmutableRubyScriptProcess(assertError.SubProcess, ns);

//                if (nuixSubProcess != null)
//                    return new RubyScriptProcess(
//                            new List<IUnitRubyBlock> { new AssertErrorRubyBlock(nuixSubProcess) }, ns);
//            }

//            if (process is Processes.Immutable.WriteFile writeFile)
//            {
//                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(writeFile.TextProcess, ns);

//                if (nuixSubprocess != null)
//                {
//                    var fullPath = Path.Combine(writeFile.Folder, writeFile.FileName);
//                    var writeBlock = new WriteFileRubyBlock(fullPath, nuixSubprocess.RubyBlock);

//                    var p = new RubyScriptProcess(new []{writeBlock}, ns);

//                    return p;
//                }
//            }


//            if (process is Conditional<Unit> conditional)
//            {
//                var nuixIf = AsImmutableRubyScriptProcessTyped(conditional.If, ns);
//                if (nuixIf != null)
//                {
//                    var nuixThen = AsImmutableRubyScriptProcess(conditional.Then, ns);
//                    if (nuixThen != null)
//                    {
//                        var nuixElse = AsImmutableRubyScriptProcess(conditional.Else, ns);
//                        if(nuixElse != null)
//                        {
//                            var r = new ConditionalRubyBlock(nuixIf.RubyBlock, nuixThen, nuixElse);
//                            return new RubyScriptProcess(new List<IUnitRubyBlock> {r}, ns);
//                        }
//                    }
//                }
//            }

//            return null;
//        }


//        private static RubyScriptProcessTyped<T>? AsImmutableRubyScriptProcessTyped<T>(IImmutableProcess<T> process,
//            INuixProcessSettings ns)
//        {
//            if (process is RubyScriptProcessTyped<T> immutableRubyScriptProcessTyped) return immutableRubyScriptProcessTyped;

//            if (process is Processes.Immutable.CheckNumber checkNumber)
//            {
//                var nuixSubprocess = AsImmutableRubyScriptProcessTyped(checkNumber.CountProcess, ns);

//                if (nuixSubprocess != null)
//                {
//                    var checkNumberBlock = new CheckNumberRubyBlock(nuixSubprocess.RubyBlock, checkNumber.Minimum, checkNumber.Maximum);

//                    var p = new RubyScriptProcessTyped<bool>(checkNumberBlock, ns, RubyScriptProcess.TryParseBool);

//                    if (p is RubyScriptProcessTyped<T> resultProcess)
//                        return resultProcess;
//                }
//            }

//            if (process is Conditional<T> conditional)
//            {
//                var nuixIf = AsImmutableRubyScriptProcessTyped(conditional.If, ns);
//                if (nuixIf != null)
//                {
//                    var nuixThen = AsImmutableRubyScriptProcessTyped(conditional.Then, ns);
//                    if (nuixThen != null)
//                    {
//                        var nuixElse = AsImmutableRubyScriptProcessTyped(conditional.Else, ns);
//                        if(nuixElse != null)
//                        {
//                            var r = new TypedConditionalRubyBlock<T>(nuixIf.RubyBlock, nuixThen, nuixElse);

//                            return new RubyScriptProcessTyped<T>(r, ns, nuixThen.TryParseFunc);
//                        }
//                    }
//                }
//            }

//            return null;
//        }
//    }
//}
