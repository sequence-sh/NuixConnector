using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
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
        public override Version RequiredVersion => new Version(7, 6);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.OCR_PROCESSING
        };
    }


    /// <summary>
    /// Performs optical character recognition on files in a NUIX case.
    /// </summary>
    public sealed class NuixPerformOCR : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixPerformOCRProcessFactory.Instance;

        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "RunOCR";

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]

        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> CasePath { get; set; }

        /// <summary>
        /// The term to use for searching for files to OCR.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [DefaultValueExplanation("NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))")]
        public IRunnableProcess<string> SearchTerm { get; set; } =
            new Constant<string>("NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))");

        /// <summary>
        /// The name of the OCR profile to use.
        /// This cannot be set at the same time as OCRProfilePath.
        /// </summary>
        [RunnableProcessProperty]
        [DefaultValueExplanation("The default profile will be used.")]
        [Example("MyOcrProfile")]
        public IRunnableProcess<string>? OCRProfileName { get; set; }

        /// <summary>
        /// Path to the OCR profile to use.
        /// This cannot be set at the same times as OCRProfileName.
        /// </summary>
        [RunnableProcessProperty]
        [RequiredVersion("Nuix", "7.6")]
        [DefaultValueExplanation("The default profile will be used.")]
        [Example("C:\\Profiles\\MyProfile.xml")]
        public IRunnableProcess<string>? OCRProfilePath { get; set; }

        /// <inheritdoc />
        internal override string ScriptText => @"
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

        /// <inheritdoc />
        internal override string MethodName => "RunOCR";

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> VerifyThis
        {
            get
            {
                if (OCRProfileName != null && OCRProfilePath != null)
                    return new RunError(
                        $"Only one of {nameof(OCRProfileName)} and {nameof(OCRProfilePath)} may be set.",
                        Name,
                        null,
                        ErrorCode.ConflictingParameters);

                return Unit.Default;
            }
        }

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchTermArg", SearchTerm, false);
            yield return ("ocrProfileArg", OCRProfileName, true);
            yield return ("ocrProfilePathArg", OCRProfilePath, true);
        }
    }
}