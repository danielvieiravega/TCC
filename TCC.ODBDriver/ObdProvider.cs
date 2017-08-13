using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using TCC.ODBDriver;

namespace TCC
{
    public class ObdProvider
    {
        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        private DataReader _reader;
        private DataWriter _writer;

        private string DeviceName { get; }

        private StreamSocket _streamSocket;

        private readonly ILogger _logger;

        public  ObdProvider(string deviceName, CoreDispatcher coreDispatcher, TextBox txtBox)
        {
            _logger = new Logger(coreDispatcher, txtBox);
            DeviceName = deviceName;
            _streamSocket = new StreamSocket();
        }

        private async Task ConfigureConnectionToElmAdapter()
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

                _streamSocket = new StreamSocket();

                await _streamSocket.ConnectAsync(_deviceService.ConnectionHostName,
                    _deviceService.ConnectionServiceName);

                _reader = new DataReader(_streamSocket.InputStream) { InputStreamOptions = InputStreamOptions.Partial };
                _writer = new DataWriter(_streamSocket.OutputStream);

                try
                {
                    await SendInitializationCommands();
                }
                catch (Exception e)
                {
                    await _logger.Log(e.Message);
                }


                await _logger.Log("Conexão efetuada com sucesso!");
            }
            else
            {
                await _logger.Log("Nenhum dispositivo encontrado!");
            }
        }

        /// <summary>
        /// Initializes the communication with the ELM327
        /// </summary>
        private async Task SendInitializationCommands()
        {
            await _logger.Log("Streams Setup!");
            await _logger.Log("Sending Commands!");
            await _logger.Log("ATZ\r");
            var res = await SendCommand("ATZ\r");
            await _logger.Log(res);

            await _logger.Log("ATSP6\r");
            res = await SendCommand("ATSP6\r");
            await _logger.Log(res);

            await _logger.Log("ATH0\r");
            res = await SendCommand("ATH0\r");
            await _logger.Log(res);

            await _logger.Log("ATCAF1\r");
            res = await SendCommand("ATCAF1\r");
            await _logger.Log(res);
        }

        private async Task<string> SendCommand(string command)
        {
            await _logger.Log($"Sending Command: {command}");
            _writer.WriteString(command);
            await _writer.StoreAsync();
            await _writer.FlushAsync();
            var count = await _reader.LoadAsync(512);
            return _reader.ReadString(count).Trim('>');
        }

        private async Task<double> GetSpeed()
        {
            var result = 0.0;
            try
            {
                await _logger.Log("010D\r");
                var response = await SendCommand("010D\r");
                await _logger.Log(response);
                result = DecodeSpeed(response);
               // await LogStatus("Ready for command");
            }
            catch (Exception e)
            {
                await _logger.Log(e.Message);
            }

            return result;
        }

        private async Task<double> GetRpm()
        {
            var result = 0.0;
            try
            {
                await _logger.Log("010C\r");
                var response = await SendCommand("010C\r");
                await _logger.Log(response);
                result =  DecodeRpm(response);
                await _logger.Log("Ready for command");
            }
            catch (Exception e)
            {
                await _logger.Log(e.Message);
            }

            return result;
        }

        private double DecodeSpeed(string response)
        {
            var result = 0.0;
            try
            {
                var speedHexString = response.Substring(4);
                var speed = DecodeHexNumber(speedHexString);

                result = speed;
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }

            return result;
        }

        private double DecodeRpm(string response)
        {
            var result = 0.0;
            try
            {
                var rpmString = response.Substring(4);
                var rpm = (Convert.ToDouble(DecodeHexNumber(response.Substring(4))));
                result = rpm;

                if (response.Length > 6)
                {
                    response = response.Replace("\r", "");
                    var rpmInHex = response.Substring(4);

                    var rpmA = rpmInHex.Substring(0, 2);
                    var rpmB = rpmInHex.Substring(2);

                    var rpmAInt = DecodeHexNumber(rpmA);
                    var rpmBInt = DecodeHexNumber(rpmB);

                    var valueA = Convert.ToInt32(rpmA, 16);
                    var valueB = Convert.ToInt32(rpmB, 16);

                    var rpm1 = ((256 * rpmAInt) + rpmBInt) / 4;

                    var rpmDouble = Convert.ToDouble(rpm1);

                }
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }

            return result;
        }

        private static int DecodeHexNumber(string hexString)
        {
            var raw = new byte[hexString.Length / 2];

            for (var i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Convert.ToInt32(raw[0].ToString());
        }
    }
}
    