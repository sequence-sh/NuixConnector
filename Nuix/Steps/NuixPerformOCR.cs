using CSharpFunctionalExtensions;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps
{

/// <summary>
/// Performs optical character recognition on items in a NUIX case.
/// </summary>
[Alias("NuixRunOCR")]
public sealed class NuixPerformOCRStepFactory : RubySearchStepFactory<NuixPerformOCR, Unit>
{
    private NuixPerformOCRStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixPerformOCR, Unit> Instance { get; } =
        new NuixPerformOCRStepFactory();

    /// <summary>
    /// Version 12 of ABBYY is compatible with Nuix Engine 7.8 onwards.
    /// Prior version of the OCR plugin have not been tested.
    /// </summary>
    public override Version RequiredNuixVersion => new(7, 8);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.OCR_PROCESSING };

    /// <inheritdoc />
    public override string FunctionName => "PerformOCR";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    if ocrProfilePathArg != nil
      log(""Loading OCR profile from path: #{ocrProfilePathArg}"", severity: :debug)
      profile_builder = $utilities.get_ocr_profile_builder
      ocr_profile = profile_builder.load(ocrProfilePathArg)
      log(""Could not load OCR profile: #{ocrProfilePathArg}"", severity: :warn) if ocr_profile.nil?
    else
      ocr_profile_name = ocrProfileArg.nil? ? 'Default' : ocrProfileArg
      log(""Loading OCR profile by name: #{ocr_profile_name}"", severity: :debug)
      ocr_profile = $utilities.get_ocr_profile_store.get_profile(ocr_profile_name)
      log(""Could not load OCR profile: #{ocr_profile_name}"", severity: :warn) if ocr_profile.nil?
    end

    return if ocr_profile.nil?

    log ""Searching for items to OCR: #{searchArg}""

    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    ocr_processor = $utilities.create_ocr_processor

    sem_ocr = Mutex.new
    ocr_count = 0
    ocr_fail = 0

    ocr_processor.when_item_event_occurs do |item|
      if item.stage.eql? 'ocr'
        if item.state.eql? 'OK'
          sem_ocr.synchronize{ ocr_count += 1 }
        else
          sem_ocr.synchronize{ ocr_fail += 1 }
        end
      end
    end

    all_items = expand_search(items, searchTypeArg)

    log ""OCR starting using profile: #{ocr_profile.get_name}""
    ocr_processor.process(all_items.to_a, ocr_profile)
    log ""OCR finished. OK: #{ocr_count} Failed: #{ocr_fail}""
";
}

/// <summary>
/// Performs optical character recognition on items in a NUIX case.
/// </summary>
public sealed class NuixPerformOCR : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixPerformOCRStepFactory.Instance;

    private const string DefaultSearchTerm =
        "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

    /// <summary>
    /// The search query used for searching for items to OCR.
    /// </summary>
    [StepProperty(1)]
    [DefaultValueExplanation(DefaultSearchTerm)]
    [RubyArgument("searchArg")]
    public override IStep<StringStream> SearchTerm { get; set; } =
        new SCLConstant<StringStream>(DefaultSearchTerm);

    /// <summary>
    /// The name of the OCR profile to use.
    /// This cannot be set at the same time as OCRProfilePath.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("The Default profile will be used.")]
    [Example("MyOcrProfile")]
    [RubyArgument("ocrProfileArg")]
    [Alias("Profile")]
    public IStep<StringStream>? OCRProfileName { get; set; }

    /// <summary>
    /// Path to the OCR profile to use.
    /// This cannot be set at the same times as OCRProfileName.
    /// </summary>
    [StepProperty]
    [RequiredVersion(NuixVersionKey, "7.6")]
    [DefaultValueExplanation("The Default profile will be used.")]
    [Example("C:\\Profiles\\MyProfile.xml")]
    [RubyArgument("ocrProfilePathArg")]
    [Alias("ProfilePath")]
    public IStep<StringStream>? OCRProfilePath { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore)
    {
        if (OCRProfileName != null && OCRProfilePath != null)
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(OCRProfileName),
                nameof(OCRProfilePath)
            );

        return base.VerifyThis(stepFactoryStore);
    }
}

}
