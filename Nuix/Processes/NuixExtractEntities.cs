﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Extract Entities from a Nuix Case.
    /// </summary>
    public sealed class NuixExtractEntities : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Extract Entities";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The path to the folder to put the output files in.
        /// </summary>
        [Required]
        [ExampleValue("C:/Output")]
        [YamlMember(Order = 4)]
        public string OutputFolder { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        
        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(casePathArg)

    puts ""Extracting Entities:""

    entityTypes = the_case.getAllEntityTypes()

    results = Hash.new { |h, k| h[k] = Hash.new { [] } }

    entitiesText = ""Type\tValue\tCount"" #The headers for the entities file

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

        /// <inheritdoc />
        internal override string MethodName => "ExtractEntities";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("casePathArg", CasePath, false);
            yield return ("outputFolderPathArg", OutputFolder, false);
        }
    }
}