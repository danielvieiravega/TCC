using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;

namespace TCC
{
    public class ObdProvider
    {
        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        
        private string DeviceName { get; }

        public StreamSocket StreamSocket { get; private set; }

        public  ObdProvider(string deviceName)
        {
            DeviceName = deviceName;
            StreamSocket = new StreamSocket();
        }

        public async Task ConfigureConnectionToElmAdapter()
        {
            var device = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            _deviceCollection = await DeviceInformation.FindAllAsync(device);

            if (_deviceCollection.Count > 0)
            {
                _selectedDevice = _deviceCollection[0];
                _deviceService = await RfcommDeviceService.FromIdAsync(_selectedDevice.Id);

                if (_deviceService == null)
                {
                    throw new Exception("Não foi possível se conectar no dispositivo");
                }
                
                await StreamSocket.ConnectAsync(_deviceService.ConnectionHostName,
                    _deviceService.ConnectionServiceName);

                var xuxu = "";
            }
        }

      }
}
