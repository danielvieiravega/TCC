using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace Sim800Driver
{

    public class xxx
    {
        private SerialDevice UartPort;
        private DataReader DataReaderObject = null;
        private DataWriter DataWriterObject;
        private CancellationTokenSource ReadCancellationTokenSource;


        //********************************
        //********************************
        //********** INITIALISE **********
        //********************************
        //********************************
        public async Task Initialise(uint BaudRate) //NOTE - THIS IS AN ASYNC METHOD!
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector("UART0");
                var dis = await DeviceInformation.FindAllAsync(aqs);
                UartPort = await SerialDevice.FromIdAsync(dis[0].Id);

                //Configure serial settings
                UartPort.WriteTimeout =
                    TimeSpan.FromMilliseconds(
                        1000); //mS before a time-out occurs when a write operation does not finish (default=InfiniteTimeout).
                UartPort.ReadTimeout =
                    TimeSpan.FromMilliseconds(
                        1000); //mS before a time-out occurs when a read operation does not finish (default=InfiniteTimeout).
                UartPort.BaudRate = 9600;
                UartPort.Parity = SerialParity.None;
                UartPort.StopBits = SerialStopBitCount.One;
                UartPort.DataBits = 8;

                DataReaderObject = new DataReader(UartPort.InputStream);
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                DataWriterObject = new DataWriter(UartPort.OutputStream);

                StartReceive();
            }
            catch (Exception ex)
            {
                throw new Exception("Uart Initialise Error", ex);
            }
        }




        //***********************************
        //***********************************
        //********** RECEIVE BYTES **********
        //***********************************
        //***********************************
        //This is all a bit complex....but necessary if you want receive to happen asynchrously and for your app to be notified instead of your code having to poll it (this code is basically polling it to create a receive event)

        //ASYNC METHOD TO CREATE THE LISTEN LOOP
        public async void StartReceive()
        {

            ReadCancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                await Listen();
                if ((ReadCancellationTokenSource.Token.IsCancellationRequested) || (UartPort == null))
                    break;
            }
        }

        //LISTEN FOR NEXT RECEIVE
        private async Task Listen()
        {
            const int NUMBER_OF_BYTES_TO_RECEIVE = 1; //<<<<<SET THE NUMBER OF BYTES YOU WANT TO WAIT FOR

            Task<UInt32> loadAsyncTask;
            byte[] ReceiveData;
            UInt32 bytesRead;

            try
            {
                if (UartPort != null)
                {
                    while (true)
                    {
                        //###### WINDOWS IoT MEMORY LEAK BUG 2017-03 - USING CancellationToken WITH LoadAsync() CAUSES A BAD MEMORY LEAK.  WORKAROUND IS
                        //TO BUILD RELEASE WITHOUT USING THE .NET NATIVE TOOLCHAIN OR TO NOT USE A CancellationToken IN THE CALL #####
                        //bytesRead = await DataReaderObject.LoadAsync(NUMBER_OF_BYTES_TO_RECEIVE).AsTask(ReadCancellationTokenSource.Token);	//Wait until buffer is full
                        bytesRead = await DataReaderObject.LoadAsync(NUMBER_OF_BYTES_TO_RECEIVE)
                            .AsTask(); //Wait until buffer is full

                        if ((ReadCancellationTokenSource.Token.IsCancellationRequested) || (UartPort == null))
                            break;

                        if (bytesRead > 0)
                        {
                            ReceiveData = new byte[NUMBER_OF_BYTES_TO_RECEIVE];
                            DataReaderObject.ReadBytes(ReceiveData);

                            foreach (byte Data in ReceiveData)
                            {
                                //-------------------------------
                                //-------------------------------
                                //----- RECEIVED NEXT BYTE ------
                                //-------------------------------
                                //-------------------------------

                                //Do something with it

                            } //foreach (byte Data in ReceiveData)

                        }

                    }
                }
            }
            catch (Exception e)
            {
                //We will get here often if the USB serial cable is removed so reset ready for a new connection (otherwise a never ending error occurs)
                if (ReadCancellationTokenSource != null)
                    ReadCancellationTokenSource.Cancel();
                System.Diagnostics.Debug.WriteLine("UART ReadAsync Exception: {0}", e.Message);
            }
        }


        //********************************
        //********************************
        //********** SEND BYTES **********
        //********************************
        //********************************
        public async void SendBytes(string command)
        {
            try
            {
                //Send data to UART
                DataWriterObject.WriteString(command);
                await DataWriterObject.StoreAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Uart Tx Error", ex);
            }
        }
    }
}