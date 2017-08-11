using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace TCC
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private DeviceInformationCollection _deviceCollection;
        //private DeviceInformation _selectedDevice;
        //private RfcommDeviceService _deviceService;
        ////CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        //public string _deviceName = "danielvv";

        //private StreamSocket _streamSocket;

        DataReader _reader;
        DataWriter _writer;

        DispatcherTimer _dispatcherTimer;
        private readonly ObdProvider _obdProvider;

        public MainPage()
        {
            this.InitializeComponent();
            //DispatcherTimerSetup();

            _obdProvider = new ObdProvider("danielvv");
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        private int x = 1;
        void dispatcherTimer_Tick(object sender, object e)
        {
            x++;
            TxtTeste.Text = x.ToString();
        }

        //private async Task ConfigureConnectionToElmAdapter()
        //{
        //    var device = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
        //    _deviceCollection = await DeviceInformation.FindAllAsync(device);
            
        //    _selectedDevice = _deviceCollection[0];
        //    _deviceService = await RfcommDeviceService.FromIdAsync(_selectedDevice.Id);

        //    if (_deviceService == null)
        //    {
        //        throw new Exception("Não foi possível se conectar no dispositivo");
        //    }

        //    await _streamSocket.ConnectAsync(_deviceService.ConnectionHostName,
        //        _deviceService.ConnectionServiceName);

        //    TxtTeste.Text = "Conexão estabelecida!";

        //    DispatcherTimerSetup();
        //}

        private void SetupStreams()
        {
            _reader = new DataReader(_obdProvider.StreamSocket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };
            _writer = new DataWriter(_obdProvider.StreamSocket.OutputStream);
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            await _obdProvider.ConfigureConnectionToElmAdapter();
        }
    }
}
