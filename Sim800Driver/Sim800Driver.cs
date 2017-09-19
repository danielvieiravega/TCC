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
            var filter = SerialDevice.GetDeviceSelector("COM5");
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

                    _reader = new DataReader(serialDevice.InputStream);
                    _writer = new DataWriter(serialDevice.OutputStream);

                    result = true;
                }
            }

            return result;
        }

        private async Task WriteAsync(string content)
        {
            //Task<UInt32> storeAsyncTask;

            //// ...

            //// Load the text from the sendText input text box to the dataWriter object
            //_writer.WriteString(content);

            //// Launch an async task to complete the write operation
            //storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

            //// ...    
        }

    }
}
