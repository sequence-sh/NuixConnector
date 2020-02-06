using System.Collections.Concurrent;

namespace Razor_Components.Inputs
{
    internal class Listener
    {
        // ReSharper disable once UnusedMember.Global - this property is bound to by the UI
        public string Property
        {
            get
            {
                var o = Dictionary[PropertyName];

                return _converter.FromObject(o);
            }
            set
            {
                var (success, asObject) = _converter.FromString(value);

                if (success)
                {
                    Dictionary.AddOrUpdate(PropertyName, asObject, (_, __) => asObject);
                }
            }
        }

        public readonly string PropertyName;

        public readonly ConcurrentDictionary<string, object?> Dictionary;
        private readonly IConverter _converter;

        public Listener(string propertyName, ConcurrentDictionary<string, object?> dictionary, IConverter converter)
        {
            PropertyName = propertyName;
            Dictionary = dictionary;
            _converter = converter;
        }
    }
}