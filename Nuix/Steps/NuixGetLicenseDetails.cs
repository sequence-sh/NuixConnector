namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Returns an Entity with information about the Nuix license currently in use.
/// Includes license type, description, exprity, number of available workers,
/// and the available features.
/// The features list can be used when configuring the Nuix Connector.
/// </summary>
public sealed class
    NuixGetLicenseDetailsFactory : RubyScriptStepFactory<NuixGetLicenseDetails, Entity>
{
    private NuixGetLicenseDetailsFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGetLicenseDetails, Entity> Instance { get; } =
        new NuixGetLicenseDetailsFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(8, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "GetLicenseDetails";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    license = $utilities.get_licence
    info = {
      :Name => license.get_short_name,
      :Description => license.get_description,
      :IsValid => license.is_valid,
      :Expiry => license.get_expiry.to_s,
      :Workers => license.get_workers,
      :Audited => license.is_audited,
      :AuditThreshold => license.get_audit_threshold.to_f,
      :AuditVerifiable => license.is_audit_verifiable,
      :Features => license.get_all_enabled_features.to_a
    }
    return info
";
}

/// <summary>
/// Returns an Entity with information about the Nuix license currently in use.
/// Includes license type, description, exprity, number of available workers,
/// and the available features.
/// The features list can be used when configuring the Nuix Connector.
/// </summary>
public sealed class NuixGetLicenseDetails : RubyCaseScriptStepBase<Entity>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Entity> RubyScriptStepFactory =>
        NuixGetLicenseDetailsFactory.Instance;
}

}
