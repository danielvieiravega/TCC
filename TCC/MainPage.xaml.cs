using System;
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
        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        //CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        public string _deviceName = "danielvv";

        private StreamSocket _streamSocket;

        DataReader _reader;
        DataWriter _writer;


        public MainPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer dispatcherTimer;
       
        int timesTicked = 1;
        int timesToTick = 10;


        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private int x = 1;
        void dispatcherTimer_Tick(object sender, object e)
        {
            x++;

        }


    }
}
