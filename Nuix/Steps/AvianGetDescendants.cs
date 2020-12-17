using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    public sealed class AvianGetDescendantsStepFactory : RubyScriptStepFactory<AvianGetDescendants, Unit>
    {
        private AvianGetDescendantsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<AvianGetDescendants, Unit> Instance { get; } =
            new AvianGetDescendantsStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(4, 0);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
            = new List<NuixFeature>
            {
                NuixFeature.ANALYSIS
            };

        /// <inheritdoc />
        public override string FunctionName => "AvianGetDescendants";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
require 'set'

nuix_case = $utilities.case_factory.open(pathArg)

log ""Searching for '#{searchArg}'""

searchOptions = {}
items = nuix_case.search(searchArg, searchOptions)

log ""#{items.length} items found""

if (items.length > 0)

    @hash = {}

    items.each_with_index do |item, item_index|
        desc = item.descendants.length
        unless @hash.key? (desc)
             @hash[desc] = Set[]
        end
        @hash[desc].add(item)
    end

    num_items = @hash.values.map(&:size).reduce(0, :+)
    bulk_annotater = $utilities.get_bulk_annotater

    for num_descendants, item_set in @hash
        log ""Updating metadata for #{item_set.size} items with #{num_descendants} descendants""
        if item_set.size< 5
            # If the item set is too small, add metadata individually. This should maybe be removed.
            for item in item_set
                item.custom_metadata.put_integer(metadataKeyArg, num_descendants.to_s)
            end
        else
            bulk_annotater.put_custom_metadata(metadataKeyArg, num_descendants, item_set) do |item|
                nil # counter?
            end
        end
        log ""Done""
    end

end

nuix_case.close";

    }

    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    [Alias("AddDescendantsMetadata")]
    public sealed class AvianGetDescendants : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => AvianGetDescendantsStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:\\Cases\\MyCase")]
        [RubyArgument("pathArg", 1)]
        [Alias("Case")]
        public IStep<StringStream> CasePath { get; set; } = null!;

        /// <summary>
        /// Descendants metadata will be appended to the items responsive to this search.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [Example("item-set:TaggedItems")]
        [RubyArgument("searchArg", 2)]
        public IStep<StringStream> SearchTerm { get; set; } = null!;

        /// <summary>
        /// The custom metadata property/key name
        /// </summary>
        [Required]
        [StepProperty(3)]
        [Example("item-set:TaggedItems")]
        [RubyArgument("metadataKeyArg", 3)]
        [Alias("PropertyName")]
        public IStep<StringStream> MetadataKey { get; set; } = null!;
    }
}
