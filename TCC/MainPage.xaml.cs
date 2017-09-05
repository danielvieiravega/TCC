using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TCC.ODBDriver;
using ThingSpeakWinRT;

namespace TCC
{
    public sealed partial class MainPage : Page
    {
        private readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimer;
        
        public bool IsClosed { get; set; }

        private readonly ObdDriver _obdDriver = new ObdDriver();
        private readonly ThingSpeakClient _thingSpeakClient = new ThingSpeakClient(false);

        private const string WriteApiKey = "DM63F2BD1CS70GJC";

        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }
        
        private async void DispatcherTimer_Tick(object sender, object e)
        {
            if (IsClosed)
            {
                _dispatcherTimer.Stop();
                return;
            }

            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var speed = await _obdDriver.GetSpeed();
                    var rpm = await _obdDriver.GetRpm();

                    GaugeSpeed.Value = speed;
                    GaugeRpm.Value = rpm;
                        
                    var dataFeed = new ThingSpeakFeed { Field1 = speed.ToString(), Field2 = rpm.ToString() };
                    await _thingSpeakClient.UpdateFeedAsync(WriteApiKey, dataFeed);
                });
            }
            catch (Exception exception)
            {
                TxtTeste.Text = exception.Message;
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await _obdDriver.InitializeConnection())
                {
                    DispatcherTimerSetup();
                    IsClosed = false;
                }
            }
            catch (Exception exception)
            {
                TxtTeste.Text = exception.Message;
            }
        }

        ~MainPage()
        {
            _obdDriver.Close().Wait();
        }

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
           await _obdDriver.Close();
            IsClosed = true;
        }
    }
}
