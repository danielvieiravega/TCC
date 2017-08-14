using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace TCC.ODBDriver
{
    public class ObdDriver
    {
        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        private DataReader _reader;
        private DataWriter _writer;

        private string DeviceName { get; }

        private StreamSocket _streamSocket;

        public  ObdDriver()
        {
            _streamSocket = new StreamSocket();
        }

        private async Task<bool> ConfigureConnectionToElmAdapter()
        {
            var result = false;
            var device = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            _deviceCollection = await DeviceInformation.FindAllAsync(device);

            if (_deviceCollection.Count > 0)
            {
                _selectedDevice = _deviceCollection[0];
                _deviceService = await RfcommDeviceService.FromIdAsync(_selectedDevice.Id);

                if (_deviceService != null)
                {
                    _streamSocket = new StreamSocket();

                    await _streamSocket.ConnectAsync(_deviceService.ConnectionHostName,
                        _deviceService.ConnectionServiceName);

                    SetupStreams();

                    try
                    {
                        await SendInitializationCommands();
                    }
                    catch (Exception e)
                    {
                        // await _logger.Log(e.Message);
                    }

                    result = true;
                }
            }

            return result;
        }

        private void SetupStreams()
        {
            _reader = new DataReader(_streamSocket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };
            _writer = new DataWriter(_streamSocket.OutputStream);
        }

        /// <summary>
        /// Initializes the communication with the ELM327
        /// </summary>
        private async Task SendInitializationCommands()
        {
            //await _logger.Log("Streams Setup!");
            //await _logger.Log("Sending Commands!");
           // await _logger.Log("ATZ\r");
            var res = await SendCommand("ATZ\r");
            // await _logger.Log(res);

            //await _logger.Log("ATSP6\r");
            res = await SendCommand("ATSP6\r");
            // await _logger.Log(res);

            // await _logger.Log("ATH0\r");
            res = await SendCommand("ATH0\r");
            // await _logger.Log(res);

            //await _logger.Log("ATCAF1\r");
            res = await SendCommand("ATCAF1\r");
            // await _logger.Log(res);
        }

        private async Task<string> SendCommand(string command)
        {
            //await _logger.Log($"Sending Command: {command}");
            _writer.WriteString(command);
            await _writer.StoreAsync();
            await _writer.FlushAsync();
            var count = await _reader.LoadAsync(512);
            return _reader.ReadString(count).Trim('>');
        }

        public async Task<double> GetSpeed()
        {
            var result = 0.0;
            try
            {
                var response = await SendCommand("010D\r");
                if (HasValidLength(response) && response.Contains("410D"))
                {
                    response = NormalizeResponse(response);
                    result = ParseSpeed(response);
                }
            }
            catch (Exception e)
            {
                //await _logger.Log(e.Message);
            }

            return result;
        }

        public async Task<double> GetRpm()
        {
            var result = 0.0;
            try
            {
                var response = await SendCommand("010C\r");
                if (HasValidLength(response) && response.Contains("410C"))
                {
                    response = NormalizeResponse(response);
                    result = ParseRpm(response);
                }
            }
            catch (Exception e)
            {
                //await _logger.Log(e.Message);
            }

            return result;
        }

        private double ParseSpeed(string response)
        {
            var result = 0.0;
            try
            {
                var speedHexString = response.Substring(4);
                var speed = Convert.ToInt32(speedHexString, 16);

                result = speed;
            }
            catch (Exception e)
            {
                // _logger.Log(e.Message);
            }

            return result;
        }

        private double ParseRpm(string response)
        {
            var result = 0.0;
            try
            {
                var rpmInHex = response.Substring(4);

                var rpmA = rpmInHex.Substring(0, 2);
                var rpmB = rpmInHex.Substring(2);
                
                var rpmAInt = Convert.ToInt32(rpmA, 16);
                var rpmBInt = Convert.ToInt32(rpmB, 16);

                var rpm = ((256 * rpmAInt) + rpmBInt) / 4;

                result = Convert.ToDouble(rpm);
            }
            catch (Exception e)
            {
                // _logger.Log(e.Message);
            }

            return result;
        }

        private static string NormalizeResponse(string res)
        {
            return res.Replace("\r", "");
        }

        private static bool HasValidLength(string res)
        {
            return res.Length < 10;
        }
    }
}
    