using Redirector.App.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization.Triggers
{
    public class WinUIKeyboardInputRouteTriggerJsonConverter : JsonConverter<WinUIKeyboardInputRouteTrigger>
    {
        public override WinUIKeyboardInputRouteTrigger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var value = new WinUIKeyboardInputRouteTrigger();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return value;

                    case JsonTokenType.PropertyName:
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "VKey":
                                value.VKey = reader.GetInt32();
                                break;
                            case "KeyDown":
                                value.KeyDown = reader.GetBoolean();
                                break;
                        }

                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, WinUIKeyboardInputRouteTrigger value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("VKey", value.VKey);
            writer.WriteBoolean("KeyDown", value.KeyDown);

            writer.WriteEndObject();
        }
    }
}
