using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace TCC
{
    public sealed partial class MainPage
    {
        public DateTime PegaHoraBrasilia()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }

        public void DispatcherTimerSetupForecastWheather()
        {
            _dispatcherTimerForecastWheather = new DispatcherTimer();
            _dispatcherTimerForecastWheather.Tick += DispatcherTimer_Tick_Forecast;
            _dispatcherTimerForecastWheather.Interval = new TimeSpan(0, 0, 10);
            _dispatcherTimerForecastWheather.Start();
        }

        private async void DispatcherTimer_Tick_Forecast(object sender, object e)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var dateTimeNow = PegaHoraBrasilia();

                    var currentWeather = await _openWeatherMapClient.CurrentWeather.GetByName("Porto Alegre");
                    ImgTemp.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{currentWeather.Weather.Icon}.png", UriKind.Absolute));
                    TxtTemp.Text = ((int)(currentWeather.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtTempDescription.Text = _foreacastWheaterIconList[currentWeather.Weather.Value];
                    TxtHoje.Text = dateTimeNow.ToString("dddd", new CultureInfo("pt-BR"));

                    var forecast = (await _openWeatherMapClient.Forecast.GetByName("Porto Alegre")).Forecast;

                    var amanha0 = dateTimeNow.Day + 1;
                    var amanha1 = dateTimeNow.Day + 2;
                    var amanha2 = dateTimeNow.Day + 3;

                    var amanha0Temp = forecast.First(d => d.From.Day == amanha0 && d.From.Hour >= 9);
                    ImgTemp0.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha0Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha0Temp.Text = ((int)(amanha0Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha0Desc.Text = _foreacastWheaterIconList[amanha0Temp.Symbol.Name];
                    TxtAmanha0.Text = amanha0Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                    var amanha1Temp = forecast.First(d => d.From.Day == amanha1 && d.From.Hour >= 9);
                    ImgTemp1.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha1Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha1Temp.Text = ((int)(amanha1Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha1Desc.Text = _foreacastWheaterIconList[amanha1Temp.Symbol.Name];
                    TxtAmanha1.Text = amanha1Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                    var amanha2Temp = forecast.First(d => d.From.Day == amanha2 && d.From.Hour >= 9);
                    ImgTemp2.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha2Temp.Symbol.Var}.png", UriKind.Absolute));
                    TxtAmanha2Temp.Text = ((int)(amanha2Temp.Temperature.Value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
                    TxtAmanha2Desc.Text = _foreacastWheaterIconList[amanha2Temp.Symbol.Name];
                    TxtAmanha2.Text = amanha2Temp.From.ToString("dddd", new CultureInfo("pt-BR"));

                });
            }
            catch (Exception exception)
            {
                var xx = exception;
            }
        }
    }
}
