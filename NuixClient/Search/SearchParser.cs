using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Superpower;
using Superpower.Display;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace NuixClient.Search
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

    /// <summary>
    /// Static class for parsing strings as Search terms
    /// </summary>
    public static class SearchParser
    {
        /// <summary>
        /// Tries to convert a string into a search term
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static (bool success, string? error, ISearchTerm? result) TryParse(string s)
        {
            //try to split the search term up into tokens
            var tokensResult = Tokenizer.TryTokenize(s);

            if (!tokensResult.HasValue)
                return (false, tokensResult.ErrorMessage, null);

            //If we get to here the search term is made of valid tokens, but they still may be in an invalid order
            
            var errors = new List<string>();
            var results = new List<ISearchTerm>();
            var tokens = tokensResult.Value;

            do
            {
                var result = BooleanParser.TryParse(tokens); //All searches should ultimately be 

                if (result.HasValue)
                {
                    if (result.Value.ErrorMessages.Any())
                        errors.AddRange(result.Value.ErrorMessages);
                    else results.Add(result.Value);
                }
                else
                    errors.Add(result.ToString());

                if(tokens.Count() == result.Remainder.Count())
                    break; //If the list has not got any shorter, then stop iterating

                tokens = result.Remainder;
            } while (tokens.Any());

            if (errors.Any())
            {
                var errorString = string.Join("\r\n", errors);

                return (false, errorString, null);
            }
            else
            {
                if (results.Count == 1)
                    return (true, null, results.Single());

                else
                {
                    var allTerms = results.SelectMany(x => x is ConjunctionTerm ct ? ct.Terms : new[] {x});

                    var finalResult = new ConjunctionTerm(allTerms);

                    return (true, null, finalResult);
                }
            }
        }

        private static readonly Tokenizer<SearchToken> Tokenizer = new TokenizerBuilder<SearchToken>()
            .Ignore(Span.WhiteSpace)
            .Ignore(Span.EqualTo("()"))
            .Match(Character.EqualTo('('), SearchToken.LParen)
            .Match(Character.EqualTo(')'), SearchToken.RParen)
            .Match(QuotedString.CStyle, SearchToken.Text)
            .Match(Span.Regex(PropertySearchTerm.CompoundPropertyRegex.ToString(), PropertySearchTerm.CompoundPropertyRegex.Options), SearchToken.Property)
            .Match(Span.Regex(PropertySearchTerm.PropertyRegex.ToString(), PropertySearchTerm.PropertyRegex.Options), SearchToken.Property)
            
            .Match(Span.Regex("[a-z0-9-]+/[a-z0-9-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), SearchToken.FileType)
            .Match(Span.Regex(Range.RangeRegex.ToString(), Range.RangeRegex.Options), SearchToken.Range)
            .Match(Span.EqualToIgnoreCase("and"), SearchToken.And, true)
            .Match(Span.EqualToIgnoreCase("or"), SearchToken.Or, true)
            .Match(Span.EqualToIgnoreCase("not"), SearchToken.Not, true)
            .Match(Span.Regex("[a-z0-9-']+", RegexOptions.Compiled | RegexOptions.IgnoreCase), SearchToken.Text, true)
            .Build();

        private static readonly TokenListParser<SearchToken, ISearchTerm> FullProperty =
            from pToken in Token.EqualTo(SearchToken.Property)
            from vToken in Token.EqualTo(SearchToken.Text)
                .Or(Token.EqualTo(SearchToken.FileType))
                .Or(Token.EqualTo(SearchToken.Range))
            select  PropertySearchTerm.TryCreate(pToken.ToStringValue(), vToken.ToStringValue());
        
        private static readonly TokenListParser<SearchToken, ISearchTerm> CompoundProperty =
            from pToken in Token.EqualTo(SearchToken.CompoundProperty)
            from vToken in Token.EqualTo(SearchToken.Text)
                .Or(Token.EqualTo(SearchToken.FileType))
                .Or(Token.EqualTo(SearchToken.Range))
            select  PropertySearchTerm.TryCreate(pToken.ToStringValue(), vToken.ToStringValue());

        private static readonly TokenListParser<SearchToken, ISearchTerm> TextParser =
            Token.EqualTo(SearchToken.Text).Select(b => new TextTerm(b.ToStringValue()) as ISearchTerm);


        private static readonly TokenListParser<SearchToken, ISearchTerm> BracketParser =
            from lp in Token.EqualTo(SearchToken.LParen)
            from b in Parse.Ref(() => BooleanParser)
            from rp in Token.EqualTo(SearchToken.RParen)
            select  b;


        private static readonly TokenListParser<SearchToken, ISearchTerm> AndParser = 
            Parse.Chain(
                Token.EqualTo(SearchToken.And),
                BracketParser
                    .Or(Parse.Ref(()=>OrParser))
                    .Or(Parse.Ref(() => NotParser))
                    .Or(FullProperty)
                    .Or(CompoundProperty)
                    .Or(TextParser),
                (op, lhs, rhs) =>
                {
                    var leftTerms = lhs is ConjunctionTerm lct ? lct.Terms : new[] {lhs};
                    var rightTerms = rhs is ConjunctionTerm rct ? rct.Terms : new[] {rhs};

                    return new ConjunctionTerm(leftTerms.Concat(rightTerms));
                });

        private static readonly TokenListParser<SearchToken, ISearchTerm> OrParser =
            Parse.Chain(
                Token.EqualTo(SearchToken.Or),
                BracketParser
                    .Or(Parse.Ref(()=> NotParser))
                    .Or(FullProperty)
                    .Or(CompoundProperty)
                    .Or(TextParser),
                (op, lhs, rhs) =>
                {
                    var leftTerms = lhs is DisjunctionTerm lct ? lct.Terms : new[] { lhs };
                    var rightTerms = rhs is DisjunctionTerm rct ? rct.Terms : new[] { rhs };

                    return new DisjunctionTerm(leftTerms.Concat(rightTerms));
                });

        private static readonly TokenListParser<SearchToken, ISearchTerm> NotParser =
            from pToken in Token.EqualTo(SearchToken.Not)
            from vToken in
                BracketParser
                .Or(FullProperty)
                .Or(CompoundProperty)
                .Or(TextParser)
            select (ISearchTerm) new NotTerm(vToken);

        private static readonly TokenListParser<SearchToken, ISearchTerm> BooleanParser =
            
            AndParser
            .Or(OrParser)
            .Or(NotParser)
            .Or(FullProperty)
            .Or(CompoundProperty)
            .Or(TextParser)
            .Or(BracketParser);
    }
}
