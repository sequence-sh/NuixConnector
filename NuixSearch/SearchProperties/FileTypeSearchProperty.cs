using CSharpFunctionalExtensions;

namespace Reductech.EDR.Connectors.Nuix.Search.SearchProperties
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

    