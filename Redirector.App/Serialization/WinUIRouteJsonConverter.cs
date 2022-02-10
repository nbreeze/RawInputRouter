using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization
{
    [JsonConverter(typeof(WinUIRoute))]
    public class WinUIRouteJsonConverter : JsonConverter<WinUIRoute>
    {
        public override WinUIRoute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIRoute route = new WinUIRoute();

            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();
            string reference;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return route;

                    case JsonTokenType.PropertyName:
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Source":
                                reference = reader.GetString();
                                if (resolver != null)
                                {
                                    if (!string.IsNullOrEmpty(reference))
                                    {
                                        route.Source = resolver.ResolveReference(reference) as IDeviceSource;
                                    }
                                }

                                break;

                            case "Destination":
                                reference = reader.GetString();
                                if (resolver != null)
                                {
                                    if (!string.IsNullOrEmpty(reference))
                                    {
                                        route.Destination = resolver.ResolveReference(reference) as IApplicationReceiver;
                                    }
                                }

                                break;
                        }

                        break;
                }
            }

            return route;
        }

        public override void Write(Utf8JsonWriter writer, WinUIRoute value, JsonSerializerOptions options)
        {
            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();

            writer.WriteStartObject();

            if (resolver != null)
            {
                bool alreadyExists;

                string reference = resolver.GetReference(value.Source, out alreadyExists);
                writer.WriteString("Source", reference);

                reference = resolver.GetReference(value.Destination, out alreadyExists);
                writer.WriteString("Destination", reference);
            }

            writer.WriteEndObject();
        }
    }
}
