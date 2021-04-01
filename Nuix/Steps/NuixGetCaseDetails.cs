using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Returns an Entity with information about the current case.
/// Includes Name, Description, GUID, Investigator, InvestigationTimeZone,
/// Users, Custodians, Tags, Languages, Location (path), and if available
/// the earliest and latest dates.
/// </summary>
public sealed class NuixGetCaseDetailsFactory : RubyScriptStepFactory<NuixGetCaseDetails, Entity>
{
    private NuixGetCaseDetailsFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGetCaseDetails, Entity> Instance { get; } =
        new NuixGetCaseDetailsFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 4);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "GetLicenseDetails";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    info = {
      :Name => $current_case.get_name,
      :Description => $current_case.get_description,
      :GUID => $current_case.get_guid,
      :Investigator => $current_case.get_investigator,
      :InvestigationTimeZone => $current_case.get_investigation_time_zone,
      :Users => $current_case.get_all_users.map {|u| u.get_short_name },
      :Custodians => $current_case.get_all_custodians.to_a,
      :Tags => $current_case.get_all_tags.to_a,
      :Languages => $current_case.get_languages.to_a,
      :Location => $current_case.get_location.get_path,
      :EarliestDate => '',
      :LatestDate => '',
      :IsCompound => $current_case.is_compound
    }
    case_stats = $current_case.get_statistics
    date_ranges = case_stats.get_case_date_range
    unless date_ranges.nil?
      info[:EarliestDate] = date_ranges.get_earliest.to_s
      info[:LatestDate] = date_ranges.get_latest.to_s
    end
    return info
";
}

/// <summary>
/// Returns an Entity with information about the current case.
/// Includes Name, Description, GUID, Investigator, InvestigationTimeZone,
/// Users, Custodians, Tags, Languages, Location (path), and if available
/// the earliest and latest dates.
/// </summary>
public sealed class NuixGetCaseDetails : RubyCaseScriptStepBase<Entity>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Entity> RubyScriptStepFactory =>
        NuixGetCaseDetailsFactory.Instance;
}

}
