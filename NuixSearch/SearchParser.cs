using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reductech.EDR.Connectors.Nuix.Search.SearchProperties;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Reductech.EDR.Connectors.Nuix.Search
{
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
                return (false, tokensResult.ToString(), null);

            //If we get to here the search term is made of valid tokens, but they still may be in an invalid order
            var errors = new List<string>();
            var results = new List<ISearchTerm>();
            var tokens = tokensResult.Value;

            do
            {
                var result = Boolean.TryParse(tokens); //All searches should ultimately be booleans

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
            .Match(Span.Regex(CompoundPropertySearchTerm.CompoundPropertyRegex.ToString(), CompoundPropertySearchTerm.CompoundPropertyRegex.Options), SearchToken.Property)
            .Match(Span.Regex(RegularPropertySearchTerm.PropertyRegex.ToString(), RegularPropertySearchTerm.PropertyRegex.Options), SearchToken.Property)
            
            .Match(Span.Regex("[a-z0-9-_]+/[a-z0-9-_]+(?:\\.[a-z0-9-_]+){0,3}", RegexOptions.Compiled | RegexOptions.IgnoreCase), SearchToken.FileType)
            .Match(Span.Regex(Range.RangeRegex.ToString(), Range.RangeRegex.Options), SearchToken.Range)
            .Match(Span.EqualToIgnoreCase("and"), SearchToken.And, true)
            .Match(Span.EqualToIgnoreCase("or"), SearchToken.Or, true)
            .Match(Span.EqualToIgnoreCase("not"), SearchToken.Not, true)
            .Match(Span.Regex("[a-z0-9-'_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), SearchToken.Text, true)
            .Build();


        private static readonly TokenListParser<SearchToken, PropertyValue> SinglePropertyValue =
            Token.EqualTo(SearchToken.Text)
                .Or(Token.EqualTo(SearchToken.FileType))
                .Or(Token.EqualTo(SearchToken.Range))
                .Select(x=> new SimplePropertyValue(x.ToStringValue()) as PropertyValue);

        private static readonly TokenListParser<SearchToken, PropertyValue> ComplexPropertyValue =
            from lp in Token.EqualTo(SearchToken.LParen)
            from b in Parse.Chain(
                Token.EqualTo(SearchToken.Or),
                SinglePropertyValue,
                (op, lhs, rhs) =>
                {
                    var leftTerms = lhs is ComplexPropertyValue lct ? lct.Disjunction : new[] { (SimplePropertyValue) lhs };
                    var rightTerms = rhs is ComplexPropertyValue rct ? rct.Disjunction : new[] { (SimplePropertyValue) rhs };

                    return new ComplexPropertyValue(leftTerms.Concat(rightTerms));
                })
            from rp in Token.EqualTo(SearchToken.RParen)
            select  b;


        private static readonly TokenListParser<SearchToken, ISearchTerm> FullProperty =
            from pToken in Token.EqualTo(SearchToken.Property)
            from vToken in SinglePropertyValue.Or(ComplexPropertyValue)
            select PropertySearchTerm.TryCreate(pToken.ToStringValue(), vToken);
        
        private static readonly TokenListParser<SearchToken, ISearchTerm> CompoundProperty =
            from pToken in Token.EqualTo(SearchToken.CompoundProperty)
            from vToken in SinglePropertyValue.Or(ComplexPropertyValue)
            select  PropertySearchTerm.TryCreate(pToken.ToStringValue(), vToken);

        private static readonly TokenListParser<SearchToken, ISearchTerm> TextParser =
            Token.EqualTo(SearchToken.Text).Select(b => new TextTerm(b.ToStringValue()) as ISearchTerm);


        private static readonly TokenListParser<SearchToken, ISearchTerm> Bracket =
            from lp in Token.EqualTo(SearchToken.LParen)
            from b in Parse.Ref(() => Boolean)
            from rp in Token.EqualTo(SearchToken.RParen)
            select  b;


        private static readonly TokenListParser<SearchToken, ISearchTerm> And = 
            Parse.Chain(
                Token.EqualTo(SearchToken.And),
                
                    Parse.Ref(()=>Or)
                    .Or(Parse.Ref(() => Not))
                    .Or(Bracket)
                    .Or(FullProperty)
                    .Or(CompoundProperty)
                    .Or(TextParser),
                (op, lhs, rhs) =>
                {
                    var leftTerms = lhs is ConjunctionTerm lct ? lct.Terms : new[] {lhs};
                    var rightTerms = rhs is ConjunctionTerm rct ? rct.Terms : new[] {rhs};

                    return new ConjunctionTerm(leftTerms.Concat(rightTerms));
                });

        private static readonly TokenListParser<SearchToken, ISearchTerm> Or =
            Parse.Chain(
                Token.EqualTo(SearchToken.Or),
                Bracket
                    .Or(Parse.Ref(()=> Not))
                    .Or(FullProperty)
                    .Or(CompoundProperty)
                    .Or(TextParser),
                (op, lhs, rhs) =>
                {
                    var leftTerms = lhs is DisjunctionTerm lct ? lct.Terms : new[] { lhs };
                    var rightTerms = rhs is DisjunctionTerm rct ? rct.Terms : new[] { rhs };

                    return new DisjunctionTerm(leftTerms.Concat(rightTerms));
                });

        private static readonly TokenListParser<SearchToken, ISearchTerm> Not =
            from pToken in Token.EqualTo(SearchToken.Not)
            from vToken in
                Bracket
                .Or(FullProperty)
                .Or(CompoundProperty)
                .Or(TextParser)
            select (ISearchTerm) new NotTerm(vToken);

        private static readonly TokenListParser<SearchToken, ISearchTerm> Boolean =
            And
            .Or(Or)
            .Or(Not)
            .Or(FullProperty)
            .Or(CompoundProperty)
            .Or(TextParser)
            .Or(Bracket);

    }
}
