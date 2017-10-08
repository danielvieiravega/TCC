using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using TCC.ODBDriver;
using ThingSpeakWinRT;
using System.Globalization;
using Windows.UI.Xaml.Media.Imaging;
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
        private DispatcherTimer _dispatcherTimer;
        private DispatcherTimer _dispatcherTimerClock;
        private DispatcherTimer _dispatcherTimerForecastWheather;
        private DispatcherTimer _dispatcherTimerSms;

        private readonly ObdDriver _obdDriver = new ObdDriver();
        private readonly Sim800LDriver _sim800Driver = new Sim800LDriver(); 
        private readonly ThingSpeakClient _thingSpeakClient = new ThingSpeakClient(false);
        private readonly OpenWeatherMapClient _openWeatherMapClient = new OpenWeatherMapClient(WeatherApi);
        private readonly Dictionary<string, string> _iconList = new Dictionary<string, string>();

        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();

            BtnClearSms.Visibility = Visibility.Collapsed;
            TxtSms.Text = DefaulSmsMessage;
            PopulateWheaterDictionary();
        }

        private void PopulateWheaterDictionary()
        {
            _iconList.Add("clear sky", "Céu limpo");
            _iconList.Add("few clouds", "Poucas nuvens");
            _iconList.Add("scattered clouds", "Nuvens dispersas");
            _iconList.Add("broken clouds", "Nuvens quebradas");
            _iconList.Add("shower rain", "Chuva de banho");
            _iconList.Add("rain", "Chuva");
            _iconList.Add("thunderstorm", "Tempestade");
            _iconList.Add("snow", "Neve");
            _iconList.Add("mist", "Névoa");
            _iconList.Add("light rain", "Chuva leve");
            _iconList.Add("moderate rain", "Chuva moderada");
            _iconList.Add("heavy intensity rain", "Chuva pesada");
            _iconList.Add("very heavy rain", "chuva muito pesada");
            _iconList.Add("extreme rain", "Chuva extrema");
            _iconList.Add("freezing rain", "chuva congelante");
            _iconList.Add("light intensity shower rain", "Chuva leve");
            _iconList.Add("heavy intensity shower rain", "chuva intensa");
            _iconList.Add("ragged shower rain", "chuva esfarrapada");
            _iconList.Add("overcast clouds", "Nublado");
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        public void DispatcherTimerClock()
        {
            _dispatcherTimerClock = new DispatcherTimer();
            _dispatcherTimerClock.Tick += DispatcherTimer_Tick_Clock;
            _dispatcherTimerClock.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimerClock.Start();
        }
        public void DispatcherTimerSetupSms()
        {
            _dispatcherTimerSms = new DispatcherTimer();
            _dispatcherTimerSms.Tick += DispatcherTimer_Tick_Sms;
            _dispatcherTimerSms.Interval = new TimeSpan(0, 0, 10);
            _dispatcherTimerSms.Start();
        }

        public void DispatcherTimerSetupForecastWheather()
        {
            _dispatcherTimerForecastWheather = new DispatcherTimer();
            _dispatcherTimerForecastWheather.Tick += DispatcherTimer_Tick_Forecast;
            _dispatcherTimerForecastWheather.Interval = new TimeSpan(0, 1, 0);
            _dispatcherTimerForecastWheather.Start();
        }

        private async void DispatcherTimer_Tick_Clock(object sender, object e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtHour.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("pt-BR"));
            });
        }

        private async void DispatcherTimer_Tick_Forecast(object sender, object e)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var currentWeather = await _openWeatherMapClient.CurrentWeather.GetByName("Porto Alegre");
                    ImgTemp.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{currentWeather.Weather.Icon}.png", UriKind.Absolute));
                    TxtTemp.Text = ((int)(currentWeather.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtTempDescription.Text = _iconList[currentWeather.Weather.Value];
                    TxtHoje.Text = PegaHoraBrasilia().ToString("dddd", new CultureInfo("pt-BR"));

                    var forecast = (await _openWeatherMapClient.Forecast.GetByName("Porto Alegre")).Forecast;

                    var amanha0 = PegaHoraBrasilia().Day + 1;
                    var amanha1 = PegaHoraBrasilia().Day + 2;
                    var amanha2 = PegaHoraBrasilia().Day + 3;

                    var amanha0Temp = forecast.First(d => d.From.Day == amanha0 && d.From.Hour >= 9);
                    ImgTemp0.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha0Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha0Temp.Text = ((int)(amanha0Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha0Desc.Text = _iconList[amanha0Temp.Symbol.Name];
                    TxtAmanha0.Text = amanha0Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                    var amanha1Temp = forecast.First(d => d.From.Day == amanha1 && d.From.Hour >= 9);
                    ImgTemp1.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha1Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha1Temp.Text = ((int)(amanha1Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha1Desc.Text = _iconList[amanha1Temp.Symbol.Name];
                    TxtAmanha1.Text = amanha1Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                    var amanha2Temp = forecast.First(d => d.From.Day == amanha2 && d.From.Hour >= 9);
                    ImgTemp2.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha2Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha2Temp.Text = ((int)(amanha2Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha2Desc.Text = _iconList[amanha2Temp.Symbol.Name];
                    TxtAmanha2.Text = amanha2Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                });
            }
            catch (Exception exception)
            {
                var xx = exception;
            }
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
                DispatcherTimerClock();

                if (await _obdDriver.InitializeConnection("danielvv"))
                {
                    DispatcherTimerSetup();
                    IsClosed = false;
                }

                if (await _sim800Driver.InitializeConnection())
                {
                    DispatcherTimerSetupSms();
                }

                DispatcherTimerSetupForecastWheather();
            }
            catch (Exception)
            {
                // ignored
            }
        }


        public DateTime PegaHoraBrasilia()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }
       
    }
}
