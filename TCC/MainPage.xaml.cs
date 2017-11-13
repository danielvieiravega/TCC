using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using TCC.ODBDriver;
using ThingSpeakWinRT;
using OpenWeatherMap;
using TCC.Sim800Driver;

namespace TCC
{
    public sealed partial class MainPage
    {
        public bool IsClosed { get; set; }

        private const int MaxSpeed = 200;
        private const string DefaulSmsMessage = "Nenhum SMS a exibir no momento!";
        private const string ThingSpeakWriteApiKey = "DM63F2BD1CS70GJC";
        private const string WeatherApi = "298782c5087abc5a14fe9d10d8fa46a4";

        private readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimerOdb;
        private DispatcherTimer _dispatcherTimerClock;
        private DispatcherTimer _dispatcherTimerForecastWheather;
        private DispatcherTimer _dispatcherTimerSms;

        private readonly ObdDriver _obdDriver = new ObdDriver();
        private readonly Sim800LDriver _sim800Driver = new Sim800LDriver(); 
        private readonly ThingSpeakClient _thingSpeakClient = new ThingSpeakClient(false);
        private readonly OpenWeatherMapClient _openWeatherMapClient = new OpenWeatherMapClient(WeatherApi);
        //private readonly Dictionary<string, string> _foreacastWheaterIconList;

        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();

            BtnClearSms.Visibility = Visibility.Collapsed;
            TxtSms.Text = DefaulSmsMessage;
            //_foreacastWheaterIconList = WheatherDescription.GetDictionary();
        }
        
        private async void BtnClearSms_Click(object sender, RoutedEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtSms.Text = string.Empty;
                TxtSms.Text = DefaulSmsMessage;
                BtnClearSms.Visibility = Visibility.Collapsed;
            });
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DispatcherTimerClock();

                if (await _obdDriver.InitializeConnection())
                {
                    DispatcherTimerOdb();
                    IsClosed = false;
                }

                //if (await _sim800Driver.InitializeConnection())
                //{

                //    DispatcherTimerSetupSms();
                //}

                DispatcherTimerSetupForecastWheather();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        ~MainPage()
        {
            _obdDriver.Close().Wait();
        }
    }
}
