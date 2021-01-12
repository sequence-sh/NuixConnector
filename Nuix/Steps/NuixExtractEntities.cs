using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Extract Entities from a Nuix Case.
/// </summary>
public sealed class
    NuixExtractEntitiesStepFactory : RubyScriptStepFactory<NuixExtractEntities, Unit>
{
    private NuixExtractEntitiesStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixExtractEntities, Unit> Instance { get; } =
        new NuixExtractEntitiesStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "ExtractEntities";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log ""Extracting Entities:""

    entityTypes = $currentCase.getAllEntityTypes()

    results = Hash.new { |h, k| h[k] = Hash.new { [] } }

    entitiesText = ""TypeDescription\tValue\tCount"" #The headers for the entities file

    if entityTypes.length > 0
        allItems = $currentCase.searchUnsorted(""named-entities:*"")

        allItems.each do |i|
            entityTypes.each do |et|
                entities = i.getEntities(et)
                entities.each do |e|
                   results[et][e] =  results[et][e].push(i.getGuid())
                end
            end
        end

        log ""Found entities for #{allItems.length} items""

        results.each do |et, values|
            totalCount = values.map{|x,y| y.length}.reduce(:+)
            entitiesText << ""#{et}\t*\t#{totalCount}"" #The total count for entities of this type
            currentText = ""Value\tGuid"" #The header for this types' file
            values.each do |value, guids|
                entitiesText << ""#{et}\t#{value}\t#{guids.length}"" #The row in the entities file
                guids.each do |guid|
                    currentText << ""#{value}\t#{guid}"" #The row in this entity type file
                end
            end
            File.write(File.join(outputFolderPathArg, et + '.txt'), currentText)
        end
    else
        log ""Case has no entities""
    end

    File.write(File.join(outputFolderPathArg, 'Entities.txt'), entitiesText) #For consistency, file is written even if there are no entities";
}

/// <summary>
/// Extract Entities from a Nuix Case.
/// </summary>
public sealed class NuixExtractEntities : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixExtractEntitiesStepFactory.Instance;

    /// <summary>
    /// The path to the folder to put the output files in.
    /// </summary>
    [Required]
    [Example("C:/Output")]
    [StepProperty(1)]
    [RubyArgument("outputFolderPathArg", 1)]
    [Alias("ToDirectory")]
    public IStep<StringStream> OutputFolder { get; set; } = null!;
}

}
