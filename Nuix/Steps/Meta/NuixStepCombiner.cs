using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Combines multiple nuix steps into a single step.
    /// </summary>
    public sealed class NuixStepCombiner : IStepCombiner
    {
        private NuixStepCombiner() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IStepCombiner Instance { get; } = new NuixStepCombiner();

        /// <inheritdoc />
        public Result<IStep<Unit>> TryCombine(IStep<Unit> p1, IStep<Unit> p2)
        {
            var pair = RubyBlockConversion.TryConvert(p1, p1.Name)
                .BindCast<IRubyBlock, IUnitRubyBlock>()
                .Compose(()=> RubyBlockConversion.TryConvert(p2, p2.Name).BindCast<IRubyBlock, IUnitRubyBlock>());

            if (pair.IsFailure)
                return pair.ConvertFailure<IStep<Unit>>();

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

            var configuration = Configuration.Combine(p1.Configuration, p2.Configuration);

            var name = $"{p1.Name}; {p2.Name}";

            var requirements = GetRequirements(p1).Concat(GetRequirements(p2)).ToList();

            var blockStep = new BlockStep(name, sequenceRubyBlock, configuration, requirements);

            return blockStep;
        }

        private static IEnumerable<Requirement> GetRequirements(IStep step)
        {
            return step switch
            {
                ICompoundStep compoundStep => compoundStep.StepFactory.Requirements,
                BlockStep blockStep => blockStep.Requirements,
                _ => Enumerable.Empty<Requirement>()
            };
        }
    }

    internal sealed class BlockStep : IStep<Unit>
    {
        /// <summary>
        /// Create a new block step.
        /// </summary>
        public BlockStep(string name,
            IUnitRubyBlock block,
            Configuration? configuration,
            IReadOnlyCollection<Requirement> requirements)
        {
            Block = block;
            Name = name;
            Configuration = configuration;
            Requirements = requirements;
        }

        public IUnitRubyBlock Block { get; }


        /// <inheritdoc />
        public async Task<Result<Unit, IRunErrors>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var settingsResult = stateMonad.GetSettings<INuixSettings>(Name);
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<Unit>();


            var r = await RubyBlockRunner.RunAsync(Name, Block, stateMonad, settingsResult.Value);

            return r;
        }


        /// <inheritdoc />
        public IFreezableStep Unfreeze()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Result<T, IRunErrors>> Run<T>(StateMonad stateMonad, CancellationToken cancellationToken) =>
            Run(stateMonad, cancellationToken).BindCast<Unit, T, IRunErrors>(
                new RunError($"Could not cast {typeof(T)} to {typeof(Unit)}", Name, null, ErrorCode.InvalidCast));

        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(ISettings settings) =>
            Requirements.Select(x => settings.CheckRequirement(Name, x))
                .Combine(_ => Unit.Default, RunErrorList.Combine);

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Configuration? Configuration { get; set; }

        /// <inheritdoc />
        public IEnumerable<IStepCombiner> StepCombiners { get; } = new[] {NuixStepCombiner.Instance};

        public IEnumerable<Requirement> Requirements { get; }
    }


}