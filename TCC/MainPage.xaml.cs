using System;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
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
        private readonly Sim800Driver.Sim800Driver _sim800Driver = new Sim800Driver.Sim800Driver(); 
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
                    var engineTemp = await _obdDriver.GetEngineTemperature();
                    var intakeTemp = await _obdDriver.GetIntakeAirTemperature();
                    var throtlePos = await _obdDriver.GetThrottlePosition();
                    var fuelPres = await _obdDriver.GetFuelPressure();

                    GaugeSpeed.Value = speed;
                    GaugeRpm.Value = rpm;
                    TxtTempEngine.Text = engineTemp + " °C";
                    TxtTempIntake.Text = intakeTemp + " °C";
                    TxtFuelPressure.Text = fuelPres + " kPa";
                    TxtThrotlePosition.Text = (int)throtlePos + " %";

                    try
                    {
                        var dataFeed = new ThingSpeakFeed { Field1 = speed.ToString(), Field2 = rpm.ToString() };
                        await _thingSpeakClient.UpdateFeedAsync(WriteApiKey, dataFeed);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var xxx = await _sim800Driver.InitializeConnection();

            //var xxx = new xxx();
            //await xxx.Initialise(9600);

            //xxx.SendBytes("AT+CMGF=?\r");

            //InitSerial();
            //try
            //{
            //    if (await _obdDriver.InitializeConnection("xxx"))
            //    {
            //        DispatcherTimerSetup();
            //        IsClosed = false;
            //    }
            //}
            //catch (Exception)
            //{
            //    // ignored
            //}
        }

        ~MainPage()
        {
            _obdDriver.Close().Wait();
        }

        //private static int value = 0;

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            await _obdDriver.Close();
            IsClosed = true;

            //TxtTempEngine.Text = value + " °C";
            //TxtTempIntake.Text = value + " °C";
            //GaugeRpm.Value = value;
            //GaugeSpeed.Value = value;
            //TxtFuelPressure.Text = value + " KpA";
            //TxtThrotlePosition.Text = value + " %";

            //value += 2;
        }
        
    }
}
