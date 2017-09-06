using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TCC.ODBDriver;
using ThingSpeakWinRT;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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

            //Random rand = new Random();
            //List<FinancialStuff> financialStuffList = new List<FinancialStuff>
            //{
            //    new FinancialStuff() {Name = "MSFT", Amount = rand.Next(0, 100)},
            //    new FinancialStuff() {Name = "AAPL", Amount = rand.Next(0, 100)},
            //    new FinancialStuff() {Name = "GOOG", Amount = rand.Next(0, 100)},
            //    new FinancialStuff() {Name = "BBRY", Amount = rand.Next(0, 100)}
            //};

            //var lineSeries = SpeedChart.Series[0] as LineSeries;
            //if (lineSeries != null)
            //    lineSeries.ItemsSource = financialStuffList;
        }

        public class FinancialStuff
        {
            public string Name { get; set; }
            public int Amount { get; set; }
        }

        public class Speed
        {
            public string Name { get; set; }
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
                        var dsafsfd = 1;
                    }

                    var lineSeries = SpeedChart.Series[0] as LineSeries;
                    (SpeedChart.Series[0] as LineSeries).ItemsSource = new List<Speed> {new Speed { Name = "a", Value = speed }, new Speed { Name = "v", Value = speed -10 }, new Speed { Name = "c", Value = speed - 20 }, new Speed { Name = "a", Value = speed - 30} };
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

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
           await _obdDriver.Close();
            IsClosed = true;
        }
    }
}
