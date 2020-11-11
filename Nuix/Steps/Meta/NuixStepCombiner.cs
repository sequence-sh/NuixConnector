using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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

            var name = $"{GetName(p1)}; {GetName(p2)}";

            var requirements = GetRequirements(p1).Concat(GetRequirements(p2)).ToList();

            var blockStep = new BlockStep(name, sequenceRubyBlock, configuration, requirements);

            return blockStep;

            static string GetName(IStep step)
            {
                if (step is ICompoundStep cs) return cs.StepFactory.TypeName;
                return step.Name;
            }
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
        public async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var settingsResult = stateMonad.GetSettings<INuixSettings>().MapError(x=>x.WithLocation(this));
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<Unit>();


            var r = await RubyBlockRunner.RunAsync(Name, Block, stateMonad, settingsResult.Value, cancellationToken).
                MapError(x=>x.WithLocation(this));

            return r;
        }


        /// <inheritdoc />
        public IFreezableStep Unfreeze()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result<T, IError>> Run<T>(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var runResult = await Run(stateMonad, cancellationToken);

            if (runResult.IsFailure)
                return runResult.ConvertFailure<T>();

            if (runResult.Value is T t)
                return t;

            return new SingleError($"Could not cast {typeof(T)} to {typeof(Unit)}", ErrorCode.InvalidCast, new StepErrorLocation(this));
        }

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings) =>
            Requirements.Select(settings.CheckRequirement)
                .Combine(_ => Unit.Default, ErrorBuilderList.Combine)
                .MapError(x=>x.WithLocation(this));

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Configuration? Configuration { get; set; }

        /// <inheritdoc />
        public IEnumerable<IStepCombiner> StepCombiners { get; } = new[] {NuixStepCombiner.Instance};

        /// <inheritdoc />
        public Type OutputType => typeof(Unit);

        public IEnumerable<Requirement> Requirements { get; }
    }


}