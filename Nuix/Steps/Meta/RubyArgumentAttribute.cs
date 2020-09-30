using System;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Indicates that this argument is to be passed to a ruby function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RubyArgumentAttribute : Attribute
    {
        /// <inheritdoc />
        public RubyArgumentAttribute(string rubyName, int order)
        {
            RubyName = rubyName;
            Order = order;
        }

        /// <summary>
        /// The position in the order where this function argument will occur.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The name of the argument in Ruby.
        /// </summary>
        public string RubyName { get; }
    }
}