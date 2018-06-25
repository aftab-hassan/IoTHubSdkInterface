List of APIs
------------
 
	1. Add a device

		async Task<DeviceInfo> AddDeviceAsync (Endpoint endpoint)
	
	2. Send message from device to cloud
	
		async Task<Result>SendMessageD2CAsync(DeviceInfo deviceInfo, List<Telemetry> data, TransportType type)

	3. Receive desired property change from cloud to device
	
	async Task ReceiveC2DDesiredPropertyChangeAsync (DeviceInfo deviceInfo, DesiredPropertyUpdateCallback callback)
	
	
API Description
---------------
	
	Serial	API use case	API Signature	API Description
	1	Add a device	async Task<DeviceInfo> AddDeviceAsync (Endpoint endpoint)	Add a device given the IoT hub connection string and device id.
				
				Input arguments
				An EndPoint object containing the following
				        - IoT hub connection string (this can be obtained by going to your IoT Hub page on the azure portal -> Shared Access Policies -> iothubower -> copy the value Connection string—primary key)This values contains the following
				                ? HostName
				                ? SharedAccessKeyName
				                ? SharedAccessKey
				        - A device id, which could be a string/GUID
				
				Return value
				An object containing
				        - The Primary key connecting string of the newly created device. 

	2	Send message from device to cloud	async Task<Result>SendMessageD2CAsync(DeviceInfo deviceInfo, List<Telemetry> data, TransportType type)	Send message(s) from device to cloud

				Input arguments
				        1. The DeviceInfo obtained from the API call to add a device above.
				        2. List of key value pairs to be transmitted from the device
				
				Return value
				        - A Result object containing the following
				                ? A boolean value indicating whether the send message call succeeded or failed
				                ? A string indicating the reason for failure, in case of failure
				                
	3	Receive desired property change from cloud to device	async Task ReceiveC2DDesiredPropertyChangeAsync (DeviceInfo deviceInfo, DesiredPropertyUpdateCallback callback)	Receives any change in desired property, and performs an action

				Input arguments
				        1. The DeviceInfo obtained from the API call to add a device above.
				        2. A function callback of the following signature to be run upon change in desired properties
				                Async Task OnDesiredPropertyChanged(Microsoft.Azure.Devices.Shared.TwinCollection desiredProperties, object userContext);
				                
				Return value
				        None

Steps for a client application to use the dll
---------------------------------------------

	1. Create a console application
	2. Right click project -> Add -> Reference -> choose the IoTHubSdkInterface.dll
	3. Install the nuget package, Microsoft.Azure.Devices.Client
	4. Add the following using directives
		a. using IoTHubSdkInterface;
	5. A sample client that uses the IoTHubSdkInterface.dll library can be found at : https://github.com/aftab-hassan/SampleClientApplicationForIoTHubSdkInterface
