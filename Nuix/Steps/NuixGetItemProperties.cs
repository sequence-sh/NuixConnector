using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// A step that the searches a case for items and outputs the values of item properties.
/// The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid'
/// Use this inside a WriteFile step to write it to a file.
/// </summary>
public sealed class
    NuixGetItemPropertiesStepFactory : RubyScriptStepFactory<NuixGetItemProperties, StringStream>
{
    private NuixGetItemPropertiesStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGetItemProperties, StringStream> Instance { get; } =
        new NuixGetItemPropertiesStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(6, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "GetParticularProperties";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log ""Finding Entities""
    items = currentCase.search(searchArg, {})
    log ""#{items.length} items found""
    propertyRegex = Regexp.new(propertyRegexArg)
    valueRegex = nil
    valueRegex = Regexp.new(valueRegexArg) if valueRegexArg != nil

    text = ""Key\tValue\tPath\tGuid""

    items.each do |i|
        i.getProperties().each do |k,v|
            begin
                if propertyRegex =~ k
                    if valueRegex != nil
                        if match = valueRegex.match(k) #Only output if the value regex actually matches
                            valueString = match.captures[0]
                            text << ""\n#{k}\t#{valueString}\t#{i.getPathNames().join(""/"")}\t#{i.getGuid()}""
                        end
                    else #output the entire value
                        text << ""\n#{k}\t#{v}\t#{i.getPathNames().join(""/"")}\t#{i.getGuid()}""
                    end
                end
            rescue
            end
        end
    end
    return text";
}

/// <summary>
/// A step that the searches a case for items and outputs the values of item properties.
/// The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid'
/// Use this inside a WriteFile step to write it to a file.
/// </summary>
public sealed class NuixGetItemProperties : RubyScriptStepBase<StringStream>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<StringStream> RubyScriptStepFactory =>
        NuixGetItemPropertiesStepFactory.Instance;

    /// <summary>
    /// The term to search for.
    /// </summary>
    [Required]
    [Example("*.txt")]
    [StepProperty(1)]
    [RubyArgument("searchArg", 1)]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The regex to search the property for.
    /// </summary>
    [Example("Date")]
    [Required]
    [StepProperty(2)]
    [RubyArgument("propertyRegexArg", 2)]
    [Alias("Filter")]
    public IStep<StringStream> PropertyRegex { get; set; } = null!;

    /// <summary>
    /// An optional regex to check the value.
    /// If this is set, only values which match this regex will be returned, and only the contents of the first capture group.
    /// </summary>
    [Example(@"(199\d)")]
    [StepProperty(3)]
    [RubyArgument("valueRegexArg", 3)]
    [DefaultValueExplanation("All values will be returned")]
    [Alias("ValueFilter")]
    public IStep<StringStream>? ValueRegex { get; set; }
}

}
