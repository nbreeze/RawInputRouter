using Redirector.App.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization.Actions
{
    public class WinUIAcceleratorOutputActionJsonConverter : JsonConverter<WinUIAcceleratorOutputAction>
    {
        public override WinUIAcceleratorOutputAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIAcceleratorOutputAction action = new();
            string propertyName;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return action;

                    case JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Code":
                                action.Accelerator = reader.GetInt32();
                                break;
                        }

                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, WinUIAcceleratorOutputAction value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("Code", value.Accelerator);

            writer.WriteEndObject();
        }
    }
}
