using System;
using System.Text.Json;

namespace Redirector.App.Serialization
{
    internal abstract class JsonConverterReadHelper
    {
        public abstract object Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options);
    }

    internal class JsonConverterReadHelper<T> : JsonConverterReadHelper
    {
        private readonly ReadDelegate _readDelegate;

        private delegate T ReadDelegate(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options);

        public JsonConverterReadHelper(object converter)
        {
            _readDelegate = Delegate.CreateDelegate(typeof(ReadDelegate), converter, "Read") as ReadDelegate;
        }

        public override object Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            => _readDelegate.Invoke(ref reader, type, options);
    }
}
