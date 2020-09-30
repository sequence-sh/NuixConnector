using System;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class StepSettingsCombo
    {
        public StepSettingsCombo(string name, CompoundStep<Unit> step, INuixSettings settings)
        {
            Name = name;
            Step = step;
            Settings = settings;
        }

        public string Name { get; }

        public readonly CompoundStep<Unit> Step;

        public readonly INuixSettings Settings;

        public override string ToString() => (Name, Settings.NuixVersion.ToString(2)).ToString();

        public bool IsStepCompatible => IsVersionCompatible(Step, Settings.NuixVersion);

        private static bool IsVersionCompatible(IStep step, Version nuixVersion)
        {
            var features = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToHashSet();

            var settings = new NuixSettings(false, "", nuixVersion, features);

            var r = step.Verify(settings);

            return r.IsSuccess;
        }

    }
}