using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a list of all irregular items in a case.
    /// The report is in CSV format. The headers are 'Reason', 'Path' and 'Guid'
    /// Reasons include 'NonSearchablePDF','BadExtension','Unrecognised','Unsupported','TextNotIndexed','ImagesNotProcessed','Poisoned','Record','UnrecognisedDeleted','NeedManualExamination', and 'CodeTextFiles'
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixCreateIrregularItemsReport : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.String;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Create Irregular Items report";
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        //TODO change how this works - at the moment it creates multiple files 
        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(casePathArg)

    puts ""Generating Report:""   
    fields = {    
        encrypted: ""flag:encrypted"",
        NonSearchablePDF: ""mime-type:application/pdf AND NOT content:*"",
        BadExtension: ""flag:irregular_file_extension"",
        Unrecognised: ""kind:unrecognised"",
        Unsupported: ""NOT flag:encrypted AND has-embedded-data:0 AND ( ( has-text:0 AND has-image:0 AND NOT flag:not_processed AND NOT kind:multimedia AND NOT mime-type:application/vnd.ms-shortcut AND NOT mime-type:application/x-contact AND NOT kind:system AND NOT mime-type:( application/vnd.apache-error-log-entry OR application/vnd.git-logstash-log-entry OR application/vnd.linux-syslog-entry OR application/vnd.logstash-log-entry OR application/vnd.ms-iis-log-entry OR application/vnd.ms-windows-event-log-record OR application/vnd.ms-windows-event-logx-record OR application/vnd.ms-windows-setup-api-win7-win8-log-boot-entry OR application/vnd.ms-windows-setup-api-win7-win8-log-section-entry OR application/vnd.ms-windows-setup-api-xp-log-entry OR application/vnd.squid-access-log-entry OR application/vnd.tcpdump.record OR application/vnd.tcpdump.tcp.stream OR application/vnd.tcpdump.udp.stream OR application/vnd.twitter-logstash-log-entry OR application/x-pcapng-entry OR filesystem/x-linux-login-logfile-record OR filesystem/x-ntfs-logfile-record OR server/dropbox-log-event OR text/x-common-log-entry OR text/x-log-entry ) AND NOT kind:log AND NOT mime-type:application/vnd.ms-exchange-stm ) OR mime-type:application/vnd.lotus-notes )"",
        TextNotIndexed: ""flag:text_not_indexed"",
        ImagesNotProcessed: ""flag:images_not_processed"",
        Poisoned: ""flag:poison"",
        Record: ""mime-type:( filesystem/x-ntfs-mft OR filesystem/x-ntfs-logfile OR filesystem/x-ntfs-file-record OR filesystem/x-ntfs-index-record OR filesystem/x-ntfs-logfile-record OR filesystem/x-ntfs-usnjrnl OR filesystem/x-ntfs-usnjrnl-record OR filesystem/x-ntfs-vss-catalog OR filesystem/x-ntfs-vss-store ) OR ( path-mime-type:filesystem/drive AND ( ( path-name:\""[File System Root]\"" AND name:( \""$AttrDef\"" OR \""$Bitmap\"" OR \""$BadClus\"" OR \""$BadClus:$Bad\"" OR \""$Boot\"" OR \""$Extend\"" OR \""$Secure\"" OR \""$Secure:$SDS\"" OR \""$UpCase\"" OR \""$UpCase:$Info\"" OR \""$Volume\"" ) ) OR path-name:\""[File System Root]/$Extend\"" ) )"",
        UnrecognisedDeleted: ""kind:unrecognised AND flag:deleted"",
        NeedManualExamination: ""kind:unrecognised AND NOT ( flag:deleted OR mime-type:( filesystem/x-ntfs-mft OR filesystem/x-ntfs-logfile OR filesystem/x-ntfs-file-record OR filesystem/x-ntfs-index-record OR filesystem/x-ntfs-logfile-record OR filesystem/x-ntfs-usnjrnl OR filesystem/x-ntfs-usnjrnl-record OR filesystem/x-ntfs-vss-catalog OR filesystem/x-ntfs-vss-store ) OR ( path-mime-type:filesystem/drive AND ( ( path-name:\""[File System Root]\"" AND name:( \""$AttrDef\"" OR \""$Bitmap\"" OR \""$BadClus\"" OR \""$BadClus:$Bad\"" OR \""$Boot\"" OR \""$Extend\"" OR \""$Secure\"" OR \""$Secure:$SDS\"" OR \""$UpCase\"" OR \""$UpCase:$Info\"" OR \""$Volume\"" ) ) OR path-name:\""[File System Root]/$Extend\"" ) ) )"",
        CodeTextFiles: ""kind:unrecognised AND (content:(function OR def) AND IF)""
    }
    
    irregularText = ""Reason\tPath\tGuid""

    fields.each do |key, value|
        items = the_case.search(value)
        
        items.each do |i|
            path = i.getPathNames().join(""/"")
            guid = i.getGuid()
            irregularText << ""\n#{key.to_s}\t#{path}\t#{guid}""
        end
    end

    the_case.close
    return irregularText;";

        /// <inheritdoc />
        internal override string MethodName => "CreateIrregularItemsReport";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(2,16);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("casePathArg", CasePath, false);
        }
    }
}