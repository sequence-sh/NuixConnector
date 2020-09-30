using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    /// <summary>
    /// Converts a core method to a ruby block
    /// </summary>
    internal interface ICoreMethodConverter
    {
        Result<IRubyBlock> Convert(IStep step);
    }
}