using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization
{
    public class WinUIDeviceSourceJsonConverter : JsonConverter<WinUIDeviceSource>
    {
        public override WinUIDeviceSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIDeviceSource source = new();
            string propertyName;

            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return source;

                    case JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "$id":
                                string reference = reader.GetString();
                                if (resolver != null)
                                {
                                    resolver.AddReference(reference, source);
                                }
                                break;

                            case "Name":
                                source.Name = reader.GetString();
                                break;

                            case "Path":
                                source.Path = reader.GetString();
                                break;

                            case "BlockOriginalInput":
                                source.BlockOriginalInput = reader.GetBoolean();
                                break;
                        }

                        break;
                }
            }

            return source;
        }

        public override void Write(Utf8JsonWriter writer, WinUIDeviceSource value, JsonSerializerOptions options)
        {
            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();

            writer.WriteStartObject();

            if (resolver != null)
            {
                bool alreadyExists;
                writer.WriteString("$id", resolver.GetReference(value, out alreadyExists));
            }

            writer.WriteString("Name", value.Name);
            writer.WriteString("Path", value.Path);
            writer.WriteBoolean("BlockOriginalInput", value.BlockOriginalInput);

            writer.WriteEndObject();
        }
    }
}
