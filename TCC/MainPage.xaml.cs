using System;
using System.Collections.Generic;
using Windows.Foundation;
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
        
        public class Speed
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
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

                    try
                    {
                        var dataFeed = new ThingSpeakFeed { Field1 = speed.ToString(), Field2 = rpm.ToString() };
                        await _thingSpeakClient.UpdateFeedAsync(WriteApiKey, dataFeed);
                    }
                    catch (Exception exception)
                    {
                        var xuxuxuxu = exception;
                    }
                });
            }
            catch (Exception exception)
            {
                //TxtTeste.Text = exception.Message;
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
                //TxtTeste.Text = exception.Message;
            }
        }

        ~MainPage()
        {
            _obdDriver.Close().Wait();
        }

        private static int value = 0;

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
           //await _obdDriver.Close();
           //IsClosed = true;

            TxtTempEngine.Text = value.ToString();
            TxtTempIntake.Text = value.ToString();
            GaugeRpm.Value = value;
            GaugeSpeed.Value = value;
            TxtFuelPressure.Text = value + " KpA";
            TxtThrotlePosition.Text = value + " %";

            value += 2;
        }
    }
}
