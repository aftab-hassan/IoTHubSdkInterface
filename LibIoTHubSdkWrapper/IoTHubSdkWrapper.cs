﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;

namespace LibIoTHubSdkWrapper
{
    public class IoTHubSdkWrapper : IDisposable
    {
        public delegate Task LibCallback(Dictionary<string, string> desiredProperties);

        private DeviceClient Client = null;
        public enum TransportType
        {
            Amqp = 0, Http1 = 1, Amqp_WebSocket_Only = 2, Amqp_Tcp_Only = 3, Mqtt = 4, Mqtt_WebSocket_Only = 5, Mqtt_Tcp_Only = 6
        };
        private Dictionary<TransportType, Microsoft.Azure.Devices.Client.TransportType> typeMapping = new Dictionary<TransportType, Microsoft.Azure.Devices.Client.TransportType>();

        public IoTHubSdkWrapper()
        {
            typeMapping.Add(TransportType.Amqp, Microsoft.Azure.Devices.Client.TransportType.Amqp);
            typeMapping.Add(TransportType.Http1, Microsoft.Azure.Devices.Client.TransportType.Http1);
            typeMapping.Add(TransportType.Amqp_WebSocket_Only, Microsoft.Azure.Devices.Client.TransportType.Amqp_WebSocket_Only);
            typeMapping.Add(TransportType.Amqp_Tcp_Only, Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only);
            typeMapping.Add(TransportType.Mqtt, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            typeMapping.Add(TransportType.Mqtt_WebSocket_Only, Microsoft.Azure.Devices.Client.TransportType.Mqtt_WebSocket_Only);
            typeMapping.Add(TransportType.Mqtt_Tcp_Only, Microsoft.Azure.Devices.Client.TransportType.Mqtt_Tcp_Only);
        }

        // Public facing APIs
        // 1. Add a device
        public async Task<DeviceInfo> AddDeviceAsync(Endpoint endpoint)
        {
            string iotHubConnectionString = endpoint.ConnectionString;
            string deviceId = endpoint.DeviceId;
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            List<string> iotHubConnectionStringComponents = endpoint.ConnectionString.Split(';').ToList<string>();
            string primaryKeyConnectionString = iotHubConnectionStringComponents[0] + ";DeviceId=" + endpoint.DeviceId + ";SharedAccessKey=" + device.Authentication.SymmetricKey.PrimaryKey;

            return new DeviceInfo(primaryKeyConnectionString);
        }

        // 2. Send message from device to cloud
        public async Task<Result> SendMessageD2CAsync(DeviceInfo deviceInfo, List<TelemetryData> data, TransportType type)
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceInfo.PrimaryKeyConnectionString, typeMapping[type]);

            if (deviceClient == null)
            {
                return new Result(false, "Failed to create DeviceClient!");
            }
            else
            {
                try
                {
                    await SendEvent(deviceClient, data);
                    return new Result(true, "Message sending successful!");
                }

                catch (Exception e)
                {
                    return new Result(false, e.ToString());
                }
            }
        }

        // 3. Receive desired property change from cloud to device
        public async Task ReceiveC2DDesiredPropertyChangeAsync(DeviceInfo deviceInfo, LibCallback callback, TransportType type)
        {
            Console.WriteLine("Connecting to hub");
            //Client = DeviceClient.CreateFromConnectionString(deviceInfo.PrimaryKeyConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            Client = DeviceClient.CreateFromConnectionString(deviceInfo.PrimaryKeyConnectionString, typeMapping[type]);
            await Client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, callback);
        }

        // Supporting methods
        private static async Task OnDesiredPropertyChanged(Microsoft.Azure.Devices.Shared.TwinCollection desiredProperties, object userContext)
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();

            Console.WriteLine("desired property change:");
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(desiredProperties));
            desiredProperties.OfType<KeyValuePair<string, object>>().All(s =>
            {
                returnDict.Add(s.Key, s.Value.ToString());
                return true;
            });
            
            await Task.Delay(100);
            await ((LibCallback)userContext)(returnDict);
            return;
        }

        private async Task SendEvent(DeviceClient deviceClient, List<TelemetryData> data)
        {
            string dataBuffer = "[";

            for (int i = 0; i < data.Count; i++)
            {
                string dataBufferHere = "{";
                int keyValuePairCount = 0;
                foreach (KeyValuePair<string, string> entry in data[i].KeyValuePair)
                {
                    dataBufferHere += addProperty(entry.Key, entry.Value);

                    if (keyValuePairCount != (data[i].KeyValuePair.Count - 1))
                        dataBufferHere += ",";

                    keyValuePairCount++;
                }

                dataBufferHere += "}";
                dataBuffer += dataBufferHere;

                if (i != (data.Count - 1))
                    dataBuffer += ",";

                // Sending each JSON message in loop
                Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBufferHere));
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), 0, dataBufferHere);
                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
            dataBuffer += "]";

            //// Sending the JSON array
            //Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBuffer));
            //Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), 0, dataBuffer);
            //await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        private string addProperty(string key, string value)
        {
            return "\"" + key + "\":" + "\"" + value + "\"";
        }

        public void Dispose()
        {
            Client?.CloseAsync().Wait();
        }
    }

    // Complex types
    public class DeviceInfo
    {
        public string PrimaryKeyConnectionString;

        public DeviceInfo(string primaryKeyConnectionString)
        {
            this.PrimaryKeyConnectionString = primaryKeyConnectionString;
        }
    }

    public class Endpoint
    {
        public string ConnectionString;
        public string DeviceId;

        public Endpoint(string connectionString, string deviceId)
        {
            this.ConnectionString = connectionString;
            this.DeviceId = deviceId;
        }
    }

    public class TelemetryData
    {
        public Dictionary<string, string> KeyValuePair;

        public TelemetryData(Dictionary<string, string> keyValuePair)
        {
            this.KeyValuePair = keyValuePair;
        }
    }

    public class Result
    {
        public Boolean IsSuccessful;
        public string Reason;

        public Result(Boolean isSuccessful, string reason)
        {
            this.IsSuccessful = isSuccessful;
            this.Reason = reason;
        }
    }

}
