using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization
{
    public class WinUIRouteTriggerJsonConverter : JsonConverter<IWinUIRouteTrigger>
    {
        public override IWinUIRouteTrigger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            IWinUIRouteTrigger value = null;
            Type actionType = null;
            string propertyName;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return value;

                    case JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Type":
                                if (actionType == null)
                                {
                                    string actionTypeName = reader.GetString();
                                    actionType = Type.GetType(actionTypeName);
                                }

                                break;

                            case "Data":
                                if (actionType == null)
                                    throw new JsonException("Tried to read Data property of unresolved Trigger type!");

                                JsonConverter converter = options.GetConverter(actionType);
                                Type readHelperType = typeof(JsonConverterReadHelper<>).MakeGenericType(actionType);
                                JsonConverterReadHelper readHelper = (JsonConverterReadHelper)Activator.CreateInstance(readHelperType, converter);

                                value = (IWinUIRouteTrigger)readHelper.Read(ref reader, actionType, options);

                                break;
                        }

                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IWinUIRouteTrigger value, JsonSerializerOptions options)
        {
            Type valueType = value.GetType();
            JsonConverter converter = options.GetConverter(valueType);
            Type writeHelperType = typeof(JsonConverterWriteHelper<>).MakeGenericType(valueType);
            JsonConverterWriteHelper writeHelper = (JsonConverterWriteHelper)Activator.CreateInstance(writeHelperType, converter);

            writer.WriteStartObject();
            writer.WriteString("Type", valueType.FullName);

            writer.WritePropertyName("Data");
            writeHelper.Write(writer, value, options);

            writer.WriteEndObject();
        }
    }
}
