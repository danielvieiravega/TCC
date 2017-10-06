using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using TCC.ODBDriver;
using ThingSpeakWinRT;
using System.Globalization;
using Windows.UI.Xaml.Media.Imaging;
using OpenWeatherMap;
using WeatherAPI.OpenWeatherMap.org;
using Weather = WeatherAPI.OpenWeatherMap.org.Models.Weather;

namespace TCC
{
    public sealed partial class MainPage
    {
        private readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimer;
        public bool IsClosed { get; set; }
        private readonly ObdDriver _obdDriver = new ObdDriver();
        private readonly Sim800Driver.Sim800Driver _sim800Driver = new Sim800Driver.Sim800Driver(); 
        private readonly ThingSpeakClient _thingSpeakClient = new ThingSpeakClient(false);
        private const string ThingSpeakWriteApiKey = "DM63F2BD1CS70GJC";

        private const int MaxSpeed = 200;
        private const string DefaulSmsMessage = "Nenhum SMS a exibir no momento!";

        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();

            BtnClearSms.Visibility = Visibility.Collapsed;

            TxtSms.Text = DefaulSmsMessage;
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        public void DispatcherTimerSetupSms()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick_Sms;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            _dispatcherTimer.Start();
        }

        private async void DispatcherTimer_Tick_Sms(object sender, object e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var messages = await _sim800Driver.ReadSms();
                var sms = messages.LastOrDefault();
                if (sms != null)
                {
                    var isSmsRead = sms.Status.Contains("READ");
                    if (isSmsRead)
                    {
                        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            TxtSms.Text = string.Empty;
                            TxtSms.Text = $" De: {sms.Sender}\n Mensagem: {sms.Message}";
                            BtnClearSms.Visibility = Visibility.Visible;
                        });
                    }
                }
            });
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtHour.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("pt-BR"));
            });

            if (IsClosed)
            {
                _dispatcherTimer.Stop();
                return;
            }

            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var currentSpeed = await _obdDriver.GetSpeed();
                    var speed = currentSpeed > MaxSpeed ? MaxSpeed : currentSpeed;
                    var rpm = await _obdDriver.GetRpm();
                    var engineTemp = await _obdDriver.GetEngineTemperature();
                    var intakeTemp = await _obdDriver.GetIntakeAirTemperature();
                    var throtlePos = await _obdDriver.GetThrottlePosition();
                    var fuelPres = await _obdDriver.GetFuelPressure();
                    var fuelTankLevel = await _obdDriver.GetFuelTankLevelInput();

                    RadialBarGaugeSpeed.Value = speed;
                    TxtSpeed.Text = speed + " km/h";

                    var normalizedRpm = Math.Round(rpm / 1000);

                    RadialBarGaugeRpm.Value = normalizedRpm;
                    TxtRpm.Text = normalizedRpm + " x 1000 rpm";

                    TxtTempEngine.Text = engineTemp + " °C";
                    TxtTempIntake.Text = intakeTemp + " °C";
                    TxtFuelPressure.Text = fuelPres + " kPa";
                    TxtThrotlePosition.Text = (int)throtlePos + " %";

                    BarFuelLevel.Value = fuelTankLevel;

                    try
                    {
                        var dataFeed = new ThingSpeakFeed
                        {
                            Field1 = speed.ToString(CultureInfo.InvariantCulture),
                            Field2 = rpm.ToString(CultureInfo.InvariantCulture),
                            Field3 = fuelPres.ToString(CultureInfo.InvariantCulture),
                            Field4 = engineTemp.ToString(CultureInfo.InvariantCulture),
                            Field5 = throtlePos.ToString(CultureInfo.InvariantCulture),
                            Field6 = intakeTemp.ToString(CultureInfo.InvariantCulture),
                            Field7 = fuelTankLevel.ToString(CultureInfo.InvariantCulture),
                        };
                        await _thingSpeakClient.UpdateFeedAsync(ThingSpeakWriteApiKey, dataFeed);
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

        ~MainPage()
        {
            _obdDriver.Close().Wait();
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
                if (await _obdDriver.InitializeConnection("danielvv"))
                {
                    DispatcherTimerSetup();
                    IsClosed = false;
                }

                if (await _sim800Driver.InitializeConnection())
                {
                    DispatcherTimerSetupSms();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }


        private const string WeatherApi = "298782c5087abc5a14fe9d10d8fa46a4";
        private readonly OpenWeatherMapClient _openWeatherMapClient = new OpenWeatherMapClient(WeatherApi);

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var currentWeather = await _openWeatherMapClient.CurrentWeather.GetByName("Porto Alegre");

                var xxx = await _openWeatherMapClient.Forecast.GetByName("Porto Alegre");

                var forecast = xxx.Forecast;

                var amanha = DateTime.Now.Day + 1;
                var amanha1 = DateTime.Now.Day + 2;
                var amanha2 = DateTime.Now.Day + 3;

                currentWeather.Temperature.Unit = "metric";

                ImgTemp.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{currentWeather.Weather.Icon}.png", UriKind.Absolute));

                TxtTemp.Text = currentWeather.Temperature.Value.ToString(CultureInfo.InvariantCulture) + " °C";

                TxtTempDescription.Text = currentWeather.Weather.Value;

                var x = 444;
            }
            catch (Exception exception)
            {
                var xx = exception;
            }
            
        }
    }
}
