using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Performs optical character recognition on files in a NUIX case.
    /// </summary>
    public sealed class NuixPerformOCRProcessFactory : RubyScriptProcessFactory<NuixPerformOCR, Unit>
    {
        private NuixPerformOCRProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixPerformOCR, Unit> Instance { get; } = new NuixPerformOCRProcessFactory();

        /// <inheritdoc />`,
        public override Version RequiredNuixVersion => new Version(7, 6);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.OCR_PROCESSING
        };

        /// <inheritdoc />
        public override string FunctionName => "RunOCR";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(pathArg)

    searchTerm = searchTermArg
    items = the_case.searchUnsorted(searchTerm).to_a

    puts ""Running OCR on #{items.length} items""

    processor = utilities.createOcrProcessor() #since Nuix 7.0 but seems to work with earlier versions anyway

    if ocrProfileArg != nil
        ocrOptions = {:ocrProfileName => ocrProfileArg}
        processor.process(items, ocrOptions)
        puts ""Items Processed""
    elsif ocrProfilePathArg != nil
        profileBuilder = utilities.getOcrProfileBuilder()
        profile = profileBuilder.load(ocrProfilePathArg)

        if profile == nil
            puts ""Could not find processing profile at #{ocrProfilePathArg}""
            exit
        end

        processor.setOcrProfileObject(profile)
    else
        processor.process(items)
        puts ""Items Processed""
    end
    the_case.close";
    }


    /// <summary>
    /// Performs optical character recognition on files in a NUIX case.
    /// </summary>
    public sealed class NuixPerformOCR : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixPerformOCRProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to use for searching for files to OCR.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [DefaultValueExplanation("NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))")]
        [RubyArgument("searchTermArg", 2)]
        public IRunnableProcess<string> SearchTerm { get; set; } =
            new Constant<string>("NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))");

        /// <summary>
        /// The name of the OCR profile to use.
        /// This cannot be set at the same time as OCRProfilePath.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [DefaultValueExplanation("The default profile will be used.")]
        [Example("MyOcrProfile")]
        [RubyArgument("ocrProfileArg", 3)]
        public IRunnableProcess<string>? OCRProfileName { get; set; }

        /// <summary>
        /// Path to the OCR profile to use.
        /// This cannot be set at the same times as OCRProfileName.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [RequiredVersion("Nuix", "7.6")]
        [DefaultValueExplanation("The default profile will be used.")]
        [Example("C:\\Profiles\\MyProfile.xml")]
        [RubyArgument("ocrProfilePathArg", 4)]
        public IRunnableProcess<string>? OCRProfilePath { get; set; }

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> VerifyThis(IProcessSettings settings)
        {
            if (OCRProfileName != null && OCRProfilePath != null)
                return new RunError(
                    $"Only one of {nameof(OCRProfileName)} and {nameof(OCRProfilePath)} may be set.",
                    Name,
                    null,
                    ErrorCode.ConflictingParameters);

            return base.VerifyThis(settings);
        }
    }
}