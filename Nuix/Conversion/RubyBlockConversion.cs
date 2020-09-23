using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    /// <summary>
    /// Converts processes to ruby blocks
    /// </summary>
    public static class RubyBlockConversion
    {
        /// <summary>
        /// Tries to convert a process into a collection of ruby blocks
        /// </summary>
        public static Result<IRubyBlock> TryConvert(IRunnableProcess process, string parameterName)
        {
            if (process is IConstantRunnableProcess)
            {
                var dynamicConstant = ConvertConstant(process as dynamic, parameterName);
                var constantRubyBlock = (IRubyBlock)dynamicConstant;

                return Result.Success(constantRubyBlock);
            }
            if (process is BlockProcess blockProcess)
                return Result.Success<IRubyBlock>(blockProcess.Block);
            if (process is IRubyScriptProcess rubyScriptProcess)
                return rubyScriptProcess.TryConvert();

            foreach (var coreMethodConverter in CoreMethodConverters.Value)
            {
                var r = coreMethodConverter.Convert(process);
                if (r.IsSuccess)
                    return r;
            }


            return Result.Failure<IRubyBlock>($"Could not convert '{process.Name}' to ruby block.");
        }

        //TODO many special cases

        private static readonly Lazy<IReadOnlyCollection<ICoreMethodConverter>> CoreMethodConverters =
            new Lazy<IReadOnlyCollection<ICoreMethodConverter>>(()=>
                Assembly.GetAssembly(typeof(ICoreMethodConverter))!
                    .GetTypes()
                    .Where(x=>x.IsClass && !x.IsAbstract)
                    .Where(x=>typeof(ICoreMethodConverter).IsAssignableFrom(x))
                    .Select(Activator.CreateInstance)
                    .Cast<ICoreMethodConverter>().ToList());

        private static ConstantRubyBlock<T> ConvertConstant<T>(Constant<T> constant, string parameterName)
        {
            return new ConstantRubyBlock<T>(constant.Value, parameterName);
        }
    }
}