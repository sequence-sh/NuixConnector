using Superpower.Display;

namespace Reductech.EDR.Connectors.Nuix.Search
{
    internal enum SearchToken
    {
        None = 0,
        [Token(Example = "raptor")]
        Text,
        [Token(Example = "AND")]
        And,
        [Token(Example = "OR")]
        Or,
        [Token(Example = "NOT")]
        Not,
        [Token(Example = "MimeType:")]
        Property,
        [Token(Example = "date-properties:\"File Modified\":")]
        CompoundProperty,
        [Token(Example = "Emails/Email")]
        FileType,

        [Token(Example = "[50 to *]")]
        Range,

        [Token(Example = "(")]
        LParen,

        [Token(Example = ")")]
        RParen

    }
}