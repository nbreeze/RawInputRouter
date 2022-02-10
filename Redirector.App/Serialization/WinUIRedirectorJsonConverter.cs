﻿using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App.Serialization
{
    public class WinUIRedirectorSerializedDataJsonConverter : JsonConverter<WinUIRedirectorSerializedData>
    {
        public override WinUIRedirectorSerializedData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter<WinUIDeviceSource> deviceConverter = options.GetConverter(typeof(WinUIDeviceSource)) as JsonConverter<WinUIDeviceSource>;
            JsonConverter<WinUIApplicationReceiver> appConverter = options.GetConverter(typeof(WinUIApplicationReceiver)) as JsonConverter<WinUIApplicationReceiver>;
            JsonConverter<WinUIRoute> routeConverter = options.GetConverter(typeof(WinUIRoute)) as JsonConverter<WinUIRoute>;

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            WinUIRedirectorSerializedData data = new();
            string propertyName;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return data;

                    case JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Devices":
                                if (reader.TokenType != JsonTokenType.StartArray)
                                    throw new JsonException();

                                while (reader.Read())
                                {
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.EndArray:
                                            goto FinishDevices;
                                        case JsonTokenType.StartObject:
                                            WinUIDeviceSource device = deviceConverter.Read(ref reader, typeof(WinUIDeviceSource), options);
                                            if (device != null)
                                            {
                                                data.Devices.Add(device);
                                            }
                                            break;
                                    }
                                }

                                FinishDevices:

                                break;

                            case "Applications":
                                if (reader.TokenType != JsonTokenType.StartArray)
                                    throw new JsonException();

                                while (reader.Read())
                                {
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.EndArray:
                                            goto FinishApps;
                                        case JsonTokenType.StartObject:
                                            WinUIApplicationReceiver app = appConverter.Read(ref reader, typeof(WinUIApplicationReceiver), options);
                                            if (app != null)
                                            {
                                                data.Applications.Add(app);
                                            }
                                            break;
                                    }
                                }

                                FinishApps:

                                break;

                            case "Routes":
                                if (reader.TokenType != JsonTokenType.StartArray)
                                    throw new JsonException();
                                
                                while (reader.Read())
                                {
                                    switch (reader.TokenType)
                                    {
                                        case JsonTokenType.EndArray:
                                            goto FinishRoutes;
                                        case JsonTokenType.StartObject:
                                            WinUIRoute route = routeConverter.Read(ref reader, typeof(WinUIRoute), options);
                                            if (route != null)
                                            {
                                                data.Routes.Add(route);
                                            }
                                            break;
                                    }
                                }

                                FinishRoutes:

                                break;
                        }

                        break;
                }
            }

            return data;
        }

        public override void Write(Utf8JsonWriter writer, WinUIRedirectorSerializedData value, JsonSerializerOptions options)
        {
            JsonConverter<WinUIDeviceSource> deviceConverter = options.GetConverter(typeof(WinUIDeviceSource)) as JsonConverter<WinUIDeviceSource>;
            JsonConverter<WinUIApplicationReceiver> appConverter = options.GetConverter(typeof(WinUIApplicationReceiver)) as JsonConverter<WinUIApplicationReceiver>;
            JsonConverter<WinUIRoute> routeConverter = options.GetConverter(typeof(WinUIRoute)) as JsonConverter<WinUIRoute>;

            writer.WriteStartObject();

            writer.WriteStartArray("Devices");

            foreach (WinUIDeviceSource source in value.Devices)
            {
                deviceConverter.Write(writer, source, options);
            }

            writer.WriteEndArray();

            writer.WriteStartArray("Applications");

            foreach (WinUIApplicationReceiver app in value.Applications)
            {
                appConverter.Write(writer, app, options);
            }

            writer.WriteEndArray();

            writer.WriteStartArray("Routes");

            foreach (WinUIRoute route in value.Routes)
            {
                routeConverter.Write(writer, route, options);
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}