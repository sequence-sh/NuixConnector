using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Extract Entities from a Nuix Case.
    /// </summary>
    public sealed class NuixExtractEntitiesProcessFactory : RubyScriptProcessFactory<NuixExtractEntities, Unit>
    {
        private NuixExtractEntitiesProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixExtractEntities, Unit> Instance { get; } = new NuixExtractEntitiesProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(4, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        public override string FunctionName => "ExtractEntities";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(casePathArg)

    puts ""Extracting Entities:""

    entityTypes = the_case.getAllEntityTypes()

    results = Hash.new { |h, k| h[k] = Hash.new { [] } }

    entitiesText = ""TypeDescription\tValue\tCount"" #The headers for the entities file

    if entityTypes.length > 0
        allItems = the_case.searchUnsorted(""named-entities:*"")

        allItems.each do |i|
            entityTypes.each do |et|
                entities = i.getEntities(et)
                entities.each do |e|
                   results[et][e] =  results[et][e].push(i.getGuid())
                end
            end
        end

        puts ""Found entities for #{allItems.length} items""

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
        puts ""Case has no entities""
    end

    File.write(File.join(outputFolderPathArg, 'Entities.txt'), entitiesText) #For consistency, file is written even if there are no entities

    the_case.close";



    }

    /// <summary>
    /// Extract Entities from a Nuix Case.
    /// </summary>
    public sealed class NuixExtractEntities : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixExtractEntitiesProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("casePathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The path to the folder to put the output files in.
        /// </summary>
        [Required]
        [Example("C:/Output")]
        [RunnableProcessProperty]
        [RubyArgument("outputFolderPathArg", 2)]
        public IRunnableProcess<string> OutputFolder { get; set; } = null!;
    }
}