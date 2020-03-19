using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Superpower.Model;

namespace Reductech.EDR.Connectors.Nuix.steps
{
    public class ComposedNuixProcess
    {

        public async IAsyncEnumerable<Result<string>> Execute(INuixProcessSettings nuixProcessSettings)
        {
            var allLines = Steps.SelectMany(x => x.Lines).ToList();
        }

        private IReadOnlyCollection<NuixStep> Steps { get; }
    }



    internal abstract class NuixStep
    {
        public abstract IEnumerable<string> Lines { get; }


    }
}
