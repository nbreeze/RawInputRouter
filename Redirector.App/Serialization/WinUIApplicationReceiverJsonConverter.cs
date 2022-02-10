﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Redirector.App.Serialization
{
    [JsonConverter(typeof(WinUIApplicationReceiver))]
    public class WinUIApplicationReceiverJsonConverter : JsonConverter<WinUIApplicationReceiver>
    {
        public override WinUIApplicationReceiver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIApplicationReceiver source = new();
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
                            case "ExecutableName":
                                source.ExecutableName = reader.GetString();
                                break;
                            case "WindowTextSearchQuery":
                                source.WindowTextSearchQuery = reader.GetString();
                                break;
                            case "WindowTextSearch":
                                source.WindowTextSearch = (WindowTextSearch)reader.GetInt32();
                                break;
                            case "WindowTextSearchCaseSensitive":
                                source.WindowTextSearchCaseSensitive = reader.GetBoolean();
                                break;
                        }

                        break;
                }
            }

            return source;
        }

        public override void Write(Utf8JsonWriter writer, WinUIApplicationReceiver value, JsonSerializerOptions options)
        {
            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();

            writer.WriteStartObject();

            if (resolver != null)
            {
                bool alreadyExists;
                writer.WriteString("$id", resolver.GetReference(value, out alreadyExists));
            }

            writer.WriteString("Name", value.Name);
            writer.WriteString("ExecutableName", value.ExecutableName);
            writer.WriteString("WindowTextSearchQuery", value.WindowTextSearchQuery);
            writer.WriteNumber("WindowTextSearch", (int)value.WindowTextSearch);
            writer.WriteBoolean("WindowTextSearchCaseSensitive", value.WindowTextSearchCaseSensitive);

            writer.WriteEndObject();
        }
    }
}
