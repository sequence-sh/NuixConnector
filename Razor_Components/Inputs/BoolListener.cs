using System.Collections.Concurrent;

namespace Razor_Components.Inputs
{
    internal class BoolListener
    {
        // ReSharper disable once UnusedMember.Global - this property is bound to by the UI
        public bool? Property
        {
            get
            {
                var o = Dictionary[PropertyName];

                return o switch
                {
                    true => true,
                    false => false,
                    _ => _isNullable? null as bool? : false
                };
            }
            set
            {
                Dictionary.AddOrUpdate(PropertyName, value, (_, __) => value);
            }
        }

        public readonly string PropertyName;

        public readonly ConcurrentDictionary<string, object?> Dictionary;
        private readonly bool _isNullable;

        public BoolListener(string propertyName, ConcurrentDictionary<string, object?> dictionary, bool isNullable)
        {
            PropertyName = propertyName;
            Dictionary = dictionary;
            _isNullable = isNullable;
        }
    }
}