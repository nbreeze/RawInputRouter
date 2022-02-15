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
    public class WinUIRouteJsonConverter : JsonConverter<WinUIRoute>
    {
        public override WinUIRoute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIRoute route = new WinUIRoute();
            JsonConverter<IWinUIRouteTrigger> triggerConverter = new WinUIRouteTriggerJsonConverter();
            JsonConverter<IWinUIRouteOutputAction> actionConverter = new WinUIRouteOutputActionJsonConverter();

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

                            case "Triggers":
                                if (reader.TokenType != JsonTokenType.StartArray)
                                {
                                    throw new JsonException();
                                }

                                while (reader.Read())
                                {
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.EndArray:
                                            goto TriggerArrayEnd;
                                        case JsonTokenType.StartObject:
                                            var trigger = triggerConverter.Read(ref reader, typeof(IWinUIRouteTrigger), options);
                                            if (trigger != null)
                                            {
                                                route.Triggers.Add(trigger);
                                            }
                                            break;
                                    }
                                }

                                TriggerArrayEnd:

                                break;

                            case "Actions":
                                if (reader.TokenType != JsonTokenType.StartArray)
                                {
                                    throw new JsonException();
                                }

                                while (reader.Read())
                                {
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.EndArray:
                                            goto ActionArrayEnd;
                                        case JsonTokenType.StartObject:
                                            var action = actionConverter.Read(ref reader, typeof(IWinUIRouteOutputAction), options);
                                            if (action != null)
                                            {
                                                route.Actions.Add(action);
                                            }
                                            break;
                                    }
                                }

                                ActionArrayEnd:

                                break;
                        }

                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, WinUIRoute value, JsonSerializerOptions options)
        {
            ReferenceResolver resolver = options.ReferenceHandler?.CreateResolver();
            JsonConverter<IWinUIRouteTrigger> triggerConverter = new WinUIRouteTriggerJsonConverter();
            JsonConverter<IWinUIRouteOutputAction> actionConverter = new WinUIRouteOutputActionJsonConverter();

            writer.WriteStartObject();

            if (resolver != null)
            {
                bool alreadyExists;

                string reference = resolver.GetReference(value.Source, out alreadyExists);
                writer.WriteString("Source", reference);

                reference = resolver.GetReference(value.Destination, out alreadyExists);
                writer.WriteString("Destination", reference);
            }

            writer.WriteStartArray("Triggers");

            foreach (IWinUIRouteTrigger trigger in value.Triggers)
            {
                triggerConverter.Write(writer, trigger, options);
            }

            writer.WriteEndArray();

            writer.WriteStartArray("Actions");

            foreach (IWinUIRouteOutputAction action in value.Actions)
            {
                actionConverter.Write(writer, action, options);
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
