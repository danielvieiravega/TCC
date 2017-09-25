using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace Sim800Driver
{
    public class Sim800Driver
    {
        private DataReader _reader;
        private DataWriter _writer;

        public async Task<bool> InitializeConnection()
        {
            var result = false;
            var filter = SerialDevice.GetDeviceSelector("UART0");
            var devices = await DeviceInformation.FindAllAsync(filter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                var serialDevice = await SerialDevice.FromIdAsync(deviceId);

                if (serialDevice != null)
                {
                    serialDevice.BaudRate = 9600;
                    serialDevice.StopBits = SerialStopBitCount.One;
                    serialDevice.DataBits = 8;
                    serialDevice.Parity = SerialParity.None;
                    serialDevice.Handshake = SerialHandshake.None;

                    _reader = new DataReader(serialDevice.InputStream)
                    {
                        InputStreamOptions = InputStreamOptions.Partial
                    };
                    _writer = new DataWriter(serialDevice.OutputStream);

                    result = true;
                }
            }

            return result;
        }
        
        public async Task<string> ReadSms()
        {
            var result = "nothing";
            try
            {
                await WriteAsync("ATI\r");
                result =  await ReadAsync();
            }
            catch (Exception e)
            {
                var afasfdsfdsa = e;
            }

            return result;
        }

        public async Task<string> ReadAsync()
        {
            var result = "nothing";

            var loadAsyncTask = _reader.LoadAsync(1024).AsTask();
            
            var bytesRead = await loadAsyncTask;

            if (bytesRead > 0)
            {
                result = _reader.ReadString(bytesRead);
            }

            //var bytes = await _reader.LoadAsync(1024);
            //var buf = _reader.ReadString(bytes);

            return result;
        }

        private async Task<bool> WriteAsync(string command)
        {
            var result = false;
            _writer.WriteString(command);

            var storeAsyncTask = _writer.StoreAsync().AsTask();

            var bytesWritten = await storeAsyncTask;
            if (bytesWritten > 0)
            {
                result = true;
            }

            //var x =  _writer.WriteString(command);
            //var xxx = await _writer.StoreAsync();


            return result;
        }
    }
}
