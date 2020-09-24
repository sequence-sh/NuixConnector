using System;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class ProcessSettingsCombo
    {
        public ProcessSettingsCombo(string name, CompoundRunnableProcess<Unit> process, INuixProcessSettings settings)
        {
            Name = name;
            Process = process;
            Settings = settings;
        }

        public string Name { get; }

        public readonly CompoundRunnableProcess<Unit> Process;

        public readonly INuixProcessSettings Settings;

        public override string ToString() => (Name, Settings.NuixVersion.ToString(2)).ToString();

        public bool IsProcessCompatible => IsVersionCompatible(Process, Settings.NuixVersion);

        private static bool IsVersionCompatible(CompoundRunnableProcess<Unit> process, Version nuixVersion)
        {
            var features = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToHashSet();

            var settings = new NuixProcessSettings(false, "", nuixVersion, features);

            var r = process.Verify(settings);

            return r.IsSuccess;
        }

    }
}