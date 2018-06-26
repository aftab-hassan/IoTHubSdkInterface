List of APIs
------------
 
	1. Add a device

	async Task<DeviceInfo> AddDeviceAsync (Endpoint endpoint)
	
	2. Send message from device to cloud
	
	async Task<Result>SendMessageD2CAsync(DeviceInfo deviceInfo, List<TelemetryData> data, TransportType type)

	3. Receive desired property change from cloud to device
	
	async Task ReceiveC2DDesiredPropertyChangeAsync (DeviceInfo deviceInfo, LibCallback callback, TransportType type)

	
	
API Description
---------------
	
		Serial	API use case	API Signature	API Description
	1	Add a device	


		async Task<DeviceInfo> AddDeviceAsync (Endpoint endpoint) - Add a device given the IoT hub connection string and device id.
				
				Input arguments
					An EndPoint object containing the following
				        - IoT hub connection string (this can be obtained by going to your IoT Hub page on the azure portal -> Shared Access Policies -> iothubower -> copy the value Connection string—primary key). This values contains the following
				                HostName
				                SharedAccessKeyName
				                SharedAccessKey
				        - A device id, which could be a string/GUID
				
				Return value
					An object containing
				        - The Primary key connecting string of the newly created device.

	2	Send message from device to cloud	

		async Task<Result>SendMessageD2CAsync(DeviceInfo deviceInfo, List<TelemetryData> data, TransportType type)	- Send message(s) from device to cloud
		
				Input arguments
				        1. The DeviceInfo obtained from the API call to add a device above.
				        2. List of TelemetryData to be transmitted from the device
				        TelemetryData contains a single member
				                a. public Dictionary<string, string> KeyValuePair;
				        It is initialized as
				                public TelemetryData(Dictionary<string, string> keyValuePair)
				                        {
				                            this.KeyValuePair = keyValuePair;
				                        }
				                
				        3. The transport type - use one of LibSdk.TransportType.Amqp, LibSdk.TransportType.Http1, LibSdk.TransportType.Amqp_WebSocket_Only, LibSdk.TransportType.Amqp_Tcp_Only, LibSdk.TransportType.Mqtt, LibSdk.TransportType.Mqtt_WebSocket_Only, LibSdk.TransportType.Mqtt_Tcp_Only
				
				Return value
				        - A Result object containing the following
				                - A boolean value indicating whether the send message call succeeded or failed
				                - A string indicating the reason for failure, in case of failure
				                
	3	Receive desired property change from cloud to device	async Task ReceiveC2DDesiredPropertyChangeAsync (DeviceInfo deviceInfo, LibCallback callback, TransportType type)	Receives any change in desired property, and performs an action
				Input arguments
				        1. The DeviceInfo obtained from the API call to add a device above.
				        2. A function callback of the following signature to be run upon change in desired properties
				                Task LibCallback(Dictionary<string, string> desiredProperties);
				                
				        3. The transport type - use one of LibSdk.TransportType.Amqp, LibSdk.TransportType.Http1, LibSdk.TransportType.Amqp_WebSocket_Only, LibSdk.TransportType.Amqp_Tcp_Only, LibSdk.TransportType.Mqtt, LibSdk.TransportType.Mqtt_WebSocket_Only, LibSdk.TransportType.Mqtt_Tcp_Only
				        
				Return value
				        None


Steps for a client application to use the dll
---------------------------------------------

	1. Create a console application
	2. Right click project -> Add -> Reference -> choose the IoTHubSdkInterface.dll
	3. Add the following using directives
		a. using IoTHubSdkInterface;
	4. A sample client that uses the IoTHubSdkInterface.dll library can be found here and could be used as a reference : https://github.com/aftab-hassan/SampleClientApplicationForIoTHubSdkInterface
