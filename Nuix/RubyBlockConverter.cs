using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix
{
    /// <summary>
    /// Converts processes to ruby blocks
    /// </summary>
    public static class RubyBlockConverter
    {
        /// <summary>
        /// Tries to convert a process into a collection of ruby blocks
        /// </summary>
        public static Result<IRubyBlock> TryConvert(IRunnableProcess process)
        {
            if (process is Constant<object>)
            {
                var dynamicConstant = ConvertConstant(process as dynamic);
                var constantRubyBlock = (IRubyBlock) dynamicConstant;

                return Result.Success(constantRubyBlock) ;
            }
            else if (process is IRubyScriptProcess rubyScriptProcess)
                return rubyScriptProcess.TryConvert();


            return Result.Failure<IRubyBlock>($"Could not convert '{process.Name}' to ruby block.");
        }

        //TODO many special cases

        private static ConstantRubyBlock<T> ConvertConstant<T>(Constant<T> constant) => new ConstantRubyBlock<T>(constant.Value);
    }
}