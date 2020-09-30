using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    /// <summary>
    /// Converts processes to ruby blocks
    /// </summary>
    public static class RubyBlockConversion
    {
        /// <summary>
        /// Tries to convert a step into a collection of ruby blocks
        /// </summary>
        public static Result<IRubyBlock> TryConvert(IStep step, string parameterName)
        {
            if (step is IConstantStep)
            {
                var dynamicConstant = ConvertConstant(step as dynamic, parameterName);
                var constantRubyBlock = (IRubyBlock)dynamicConstant;

                return Result.Success(constantRubyBlock);
            }
            if (step is BlockStep blockProcess)
                return Result.Success<IRubyBlock>(blockProcess.Block);
            if (step is IRubyScriptStep rubyScriptProcess)
                return rubyScriptProcess.TryConvert();

            foreach (var coreMethodConverter in CoreMethodConverters.Value)
            {
                var r = coreMethodConverter.Convert(step);
                if (r.IsSuccess)
                    return r;
            }


            return Result.Failure<IRubyBlock>($"Could not convert '{step.Name}' to ruby block.");
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