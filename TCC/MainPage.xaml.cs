﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TCC
{
    public sealed partial class MainPage : Page
    {
        private DataReader _reader;
        private DataWriter _writer;
        private readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimer;
        //private readonly ObdProvider _obdProvider;

        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        private StreamSocket _streamSocket;

        public bool IsClosed { get; set; }

        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();

            //_obdProvider = new ObdProvider("danielvv");
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            if (!IsClosed)
            {
                try
                {
                    GetSpeed();
                    GetRpm();
                }
                catch (Exception exception)
                {
                    TxtTeste.Text = exception.Message;
                }
            }
            else
            {
                _dispatcherTimer.Stop();
            }
        }

        public async Task ConfigureConnectionToElmAdapter()
        {
            var device = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            _deviceCollection = await DeviceInformation.FindAllAsync(device);

            if (_deviceCollection.Count > 0)
            {
                _selectedDevice = _deviceCollection.Single(x => x.Name == "danielvv");
                _deviceService = await RfcommDeviceService.FromIdAsync(_selectedDevice.Id);

                if (_deviceService == null)
                {
                    throw new Exception("Não foi possível se conectar no dispositivo");
                }

                _streamSocket = new StreamSocket();

                await _streamSocket.ConnectAsync(_deviceService.ConnectionHostName,
                    _deviceService.ConnectionServiceName);

                _reader = new DataReader(_streamSocket.InputStream) {InputStreamOptions = InputStreamOptions.Partial};
                _writer = new DataWriter(_streamSocket.OutputStream);

                try
                {
                    SendInitializationCommands();
                }
                catch (Exception e)
                {
                    await LogStatus(e.Message);
                }

                IsClosed = false;

                DispatcherTimerSetup();

                await LogStatus("Conexão efetuada com sucesso!");
            }
            else
            {
                await LogStatus("Nenhum dispositivo encontrado!");
            }
        }
        
        /// <summary>
        /// Initializes the communication with the ELM327
        /// </summary>
        private void SendInitializationCommands()
        {
            //await LogStatus("Streams Setup!");
            //await LogStatus("Sending Commands!");
            //await AddCommandLog("ATZ\r");
            SendCommand("ATZ\r").Wait();
            //await AddCommandLog(res, true);

           // await AddCommandLog("ATSP6\r");
            SendCommand("ATSP6\r").Wait();
            // await AddCommandLog(res, true);

            // await AddCommandLog("ATH0\r");
            SendCommand("ATH0\r").Wait();
           // await AddCommandLog(res, true);

           // await AddCommandLog("ATCAF1\r");
            SendCommand("ATCAF1\r").Wait();
          //  await AddCommandLog(res, true);
        }

        private async Task AddCommandLog(string value, bool isResponse = false)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtTeste.Text = string.Format("{0}: {1}", isResponse ? "Response" : "Request", value);
            });
        }

        private async Task LogStatus(string status)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtStatus.Text = string.Format("Status: {0}", status);
            });
        }

        private async Task<string> SendCommand(string command)
        {
            var result = "Failure sending command!";
            try
            {
                await LogStatus(string.Format("Sending Command - {0}", command));
                _writer.WriteString(command);
                await _writer.StoreAsync();
                await _writer.FlushAsync();
                var count = await _reader.LoadAsync(512);
                result = _reader.ReadString(count).Trim('>');
            }
            catch (Exception e)
            {
                TxtTeste.Text = e.Message;
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

        private async void GetSpeed()
        {
            try
            {
                //await AddCommandLog("010D\r");
                var response = await SendCommand("010D\r");
                //await AddCommandLog(response, true);
                if (HasValidLength(response))
                {
                    response = NormalizeResponse(response);
                    if (response.Contains("410D"))
                        await DecodeAndShowSpeed(response);
                }
                //await LogStatus("Ready for command");
            }
            catch (Exception e)
            {
                TxtTeste.Text = e.Message;
            }
        }

        private async void GetRpm()
        {
            try
            {
                //await AddCommandLog("010C\r");
                var response = await SendCommand("010C\r");
                //await AddCommandLog(response, true);
                if (HasValidLength(response))
                {
                    response = NormalizeResponse(response);
                    if (response.Contains("410C"))
                        await DecodeAndShowRpm(response);
                }
                //await LogStatus("Ready for command");
            }
            catch (Exception e)
            {
                TxtTeste.Text = e.Message;
            }
            
        }

        private async Task DecodeAndShowSpeed(string response)
        {
            var speedHexString = response.Substring(4);
            var speed = DecodeHexNumber(speedHexString);
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GaugeSpeed.Value = speed;
            });
        }

        private async Task DecodeAndShowRpm(string response)
        {
            try
            {
                //var rpmString = response.Substring(4);
                //var rpm = (Convert.ToDouble(DecodeHexNumber(response.Substring(4))));

                //await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                //    GaugeRpm.Value = rpm;
                //});

                //if (response.Length > 6)
                //{
                    response = response.Replace("\r", "");
                    var rpmInHex = response.Substring(4);

                    var rpmA = rpmInHex.Substring(0, 2);
                    var rpmB = rpmInHex.Substring(2);

                    //var rpmAInt = DecodeHexNumber(rpmA);
                    //var rpmBInt = DecodeHexNumber(rpmB);

                    var rpmAInt = Convert.ToInt32(rpmA, 16);
                    var rpmBInt = Convert.ToInt32(rpmB, 16);

                    var rpm1 = ((256 * rpmAInt) + rpmBInt) / 4;

                    var rpmDouble = Convert.ToDouble(rpm1);
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        GaugeRpm.Value = rpmDouble;
                    });
                //}
            }
            catch (Exception e)
            {
                TxtTeste.Text = e.Message;
            }
            
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

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ConfigureConnectionToElmAdapter();
                
            }
            catch (Exception exception)
            {
                TxtTeste.Text = exception.Message;
            }
        }

        ~MainPage()
        {
            //_streamSocket.OutputStream.FlushAsync().GetResults();
            _streamSocket.Dispose();
            _streamSocket = null;
            _deviceService.Dispose();
            _deviceService = null;
        }

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            await _streamSocket.CancelIOAsync();
            _streamSocket.Dispose();
            _streamSocket = null;
            _deviceService.Dispose();
            _deviceService = null;
            IsClosed = true;
        }


       

        //	Console.WriteLine(Convert.ToUInt32(Mode.CurrentData).ToString("X2") + Convert.ToUInt32(PID.Speed).ToString("X2") + "\r");
    }
}
