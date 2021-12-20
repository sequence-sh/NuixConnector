namespace Reductech.Sequence.Connectors.Nuix.Steps
{

/// <summary>
/// Creates a list of all terms appearing in the case and their frequencies.
/// The report is in CSV format. The headers are 'Term' and 'Count'
/// Use this inside a WriteFile step to write it to a file.
/// </summary>
public sealed class
    NuixCreateTermListStepFactory : RubyScriptStepFactory<NuixCreateTermList, StringStream>
{
    private NuixCreateTermListStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixCreateTermList, StringStream> Instance { get; } =
        new NuixCreateTermListStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "CreateTermList";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Generating Report:""
    caseStatistics = $current_case.getStatistics()
    termStatistics = caseStatistics.getTermStatistics("""", {""sort"" => ""on"", ""deduplicate"" => ""md5""}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    log ""#{termStatistics.length} terms""

    text = ""Term\tCount""

    termStatistics.each do |term, count|
        text << ""\n#{term}\t#{count}""
    end
    return text";
}

/// <summary>
/// Creates a list of all terms appearing in the case and their frequencies.
/// The report is in CSV format. The headers are 'Term' and 'Count'
/// Use this inside a WriteFile step to write it to a file.
/// </summary>
public sealed class NuixCreateTermList : RubyCaseScriptStepBase<StringStream>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<StringStream> RubyScriptStepFactory =>
        NuixCreateTermListStepFactory.Instance;
}

}
