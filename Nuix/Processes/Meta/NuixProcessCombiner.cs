using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    /// <summary>
    /// Combines multiple nuix processes into a single process.
    /// </summary>
    public sealed class NuixProcessCombiner : IProcessCombiner
    {
        private NuixProcessCombiner() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IProcessCombiner Instance { get; } = new NuixProcessCombiner();

        /// <inheritdoc />
        public Result<IRunnableProcess<Unit>> TryCombine(IRunnableProcess<Unit> p1, IRunnableProcess<Unit> p2)
        {
            var pair = RubyBlockConversion.TryConvert(p1, p1.Name)
                .BindCast<IRubyBlock, IUnitRubyBlock>()
                .Compose(()=> RubyBlockConversion.TryConvert(p2, p2.Name).BindCast<IRubyBlock, IUnitRubyBlock>());

            if (pair.IsFailure)
                return pair.ConvertFailure<IRunnableProcess<Unit>>();

            var blocks = new List<IUnitRubyBlock>();

            if(pair.Value.Item1 is SequenceRubyBlock srb1)
                blocks.AddRange(srb1.Blocks);
            else
                blocks.Add(pair.Value.Item1);

            if (pair.Value.Item2 is SequenceRubyBlock srb2)
                blocks.AddRange(srb2.Blocks);
            else
                blocks.Add(pair.Value.Item2);


            var sequenceRubyBlock = new SequenceRubyBlock(blocks);

            var configuration = ProcessConfiguration.Combine(p1.ProcessConfiguration, p2.ProcessConfiguration);

            var name = $"{p1.Name}; {p2.Name}";

            var requirements = GetRequirements(p1).Concat(GetRequirements(p2)).ToList();

            var blockProcess = new BlockProcess(name, sequenceRubyBlock, configuration, requirements);

            return blockProcess;
        }

        private static IEnumerable<Requirement> GetRequirements(IRunnableProcess process)
        {
            if (process is ICompoundRunnableProcess compoundRunnableProcess)
                return compoundRunnableProcess.RunnableProcessFactory.Requirements;
            else if (process is BlockProcess blockProcess)
                return blockProcess.Requirements;

            return Enumerable.Empty<Requirement>();
        }
    }

    internal sealed class BlockProcess : IRunnableProcess<Unit>
    {
        /// <summary>
        /// Create a new block process.
        /// </summary>
        public BlockProcess(string name,
            IUnitRubyBlock block,
            ProcessConfiguration? configuration,
            IReadOnlyCollection<Requirement> requirements)
        {
            Block = block;
            Name = name;
            ProcessConfiguration = configuration;
            Requirements = requirements;
        }

        public IUnitRubyBlock Block { get; }


        /// <inheritdoc />
        public Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<Unit>();


            var r = RubyProcessRunner.RunAsync(Name, Block, processState, settingsResult.Value).Result;

            return r;
        }


        /// <inheritdoc />
        public IFreezableProcess Unfreeze()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Result<T, IRunErrors> Run<T>(ProcessState processState) =>
            Run(processState).BindCast<Unit, T, IRunErrors>(
                new RunError($"Could not cast {typeof(T)} to {typeof(Unit)}", Name, null, ErrorCode.InvalidCast));

        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(IProcessSettings settings) =>
            Requirements.Select(x => settings.CheckRequirement(Name, x))
                .Combine(_ => Unit.Default, RunErrorList.Combine);

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ProcessConfiguration? ProcessConfiguration { get; set; }

        /// <inheritdoc />
        public IEnumerable<IProcessCombiner> ProcessCombiners { get; } = new[] {NuixProcessCombiner.Instance};

        public IEnumerable<Requirement> Requirements { get; }
    }


}