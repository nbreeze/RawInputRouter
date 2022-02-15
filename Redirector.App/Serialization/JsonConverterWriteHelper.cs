using System;
using System.Text.Json;

namespace Redirector.App.Serialization
{
    internal abstract class JsonConverterWriteHelper
    {
        public abstract void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options);
    }

    internal class JsonConverterWriteHelper<T> : JsonConverterWriteHelper
        where T : class
    {
        private readonly WriteDelegate _delegate;

        private delegate void WriteDelegate(Utf8JsonWriter reader, T value, JsonSerializerOptions options);

        public JsonConverterWriteHelper(object converter)
        {
            _delegate = Delegate.CreateDelegate(typeof(WriteDelegate), converter, "Write") as WriteDelegate;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            => _delegate.Invoke(writer, (T)value, options);
    }
}
