using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Performs optical character recognition on files in a NUIX case.
/// </summary>
public sealed class NuixPerformOCRStepFactory : RubyScriptStepFactory<NuixPerformOCR, Unit>
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

    searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
    log(""Search options: #{searchOptions}"", severity: :trace)

    if sortArg.nil? || !sortArg
      log('Search results will be unsorted', severity: :trace)
      items = $current_case.search_unsorted(searchArg, searchOptions)
    else
      log('Search results will be sorted', severity: :trace)
      items = $current_case.search(searchArg, searchOptions)
    end

    if items.length == 0
      log 'No items found to OCR.'
      return
    end

    log ""Items found: #{items.length}""

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

    log ""OCR starting using profile: #{ocr_profile.get_name}""
    ocr_processor.process(items.to_a, ocr_profile)
    log ""OCR finished. OK: #{ocr_count} Failed: #{ocr_fail}""
";
}

/// <summary>
/// Performs optical character recognition on files in a NUIX case.
/// </summary>
public sealed class NuixPerformOCR : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixPerformOCRStepFactory.Instance;

    private const string DefaultSearchTerm =
        "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

    /// <summary>
    /// The term to use for searching for files to OCR.
    /// </summary>
    [StepProperty(1)]
    [DefaultValueExplanation(DefaultSearchTerm)]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } =
        new StringConstant(DefaultSearchTerm);

    /// <summary>
    /// The name of the OCR profile to use.
    /// This cannot be set at the same time as OCRProfilePath.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("The Default profile will be used.")]
    [Example("MyOcrProfile")]
    [RubyArgument("ocrProfileArg")]
    [Alias("Profile")]
    public IStep<StringStream>? OCRProfileName { get; set; }

    /// <summary>
    /// Path to the OCR profile to use.
    /// This cannot be set at the same times as OCRProfileName.
    /// </summary>
    [StepProperty(3)]
    [RequiredVersion("Nuix", "7.6")]
    [DefaultValueExplanation("The Default profile will be used.")]
    [Example("C:\\Profiles\\MyProfile.xml")]
    [RubyArgument("ocrProfilePathArg")]
    [Alias("ProfilePath")]
    public IStep<StringStream>? OCRProfilePath { get; set; }

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Core.Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(SCLSettings settings)
    {
        if (OCRProfileName != null && OCRProfilePath != null)
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(OCRProfileName),
                nameof(OCRProfilePath)
            );

        return base.VerifyThis(settings);
    }
}

}
