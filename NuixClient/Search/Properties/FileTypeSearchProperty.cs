using Orchestration;

namespace NuixClient.Search.Properties
{

    
    
        internal class FileTypeSearchProperty : GenericSearchProperty<FileType>
        {
            public FileTypeSearchProperty(string propertyName) : base(propertyName)
            {
            }

            protected override Result<FileType> TryParse(string s)
            {
                return FileType.TryParse(s);
            }
        }
}

    