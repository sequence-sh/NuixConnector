using System;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Apply arbitrary unique deterministic suffixes to terms within scripts
    /// </summary>
    public sealed class Suffixer
    {
        /// <summary>
        /// Create a new suffixer starting with 1.
        /// </summary>
        public Suffixer() : this(string.Empty, Numbering.Number)
        {

        }

        private Suffixer(string prefix, Numbering myNumbering)
        {
            Prefix = prefix;
            MyNumbering = myNumbering;
        }

        /// <summary>
        /// Gets a suffixer with this as the prefix and then increments.
        /// </summary>
        /// <returns></returns>
        public Suffixer GetNextChild()
        {
            var prefix = GetNext();
            var newNumbering = GetNextNumbering(MyNumbering);

            var child = new Suffixer(prefix, newNumbering);

            return child;
        }

        /// <summary>
        /// Gets the current value and then increments.
        /// </summary>
        /// <returns></returns>
        public string GetNext()
        {
            var v =  CurrentSuffix;
            CurrentValue++;

            return v;

        }

        /// <summary>
        /// Gets the current string. Using this will not increment the suffixer.
        /// </summary>
        public string CurrentSuffix => Prefix + ConvertToString(MyNumbering, CurrentValue);

        /// <summary>
        /// The prefix of this numberer.
        /// </summary>
        private string Prefix { get; }

        private int CurrentValue { get; set; } = 1;

        private Numbering MyNumbering { get; }

        private static string ConvertToString(Numbering numbering, int value)
        {
            return numbering switch
            {
                Numbering.Number => value.ToString(),
                Numbering.LowerCaseLetter => ((char)('a' + value - 1)) .ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(numbering), numbering, null)
            };
        }

        private static  Numbering GetNextNumbering(Numbering n)
        {
            return n switch
            {
                Numbering.Number => Numbering.LowerCaseLetter,
                Numbering.LowerCaseLetter => Numbering.Number,
                _ => throw new ArgumentOutOfRangeException(nameof(n), n, null)
            };
        }

        private enum Numbering
        {
            Number,
            LowerCaseLetter
        }
    }
}