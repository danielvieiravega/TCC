using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using OpenWeatherMap;

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
            _dispatcherTimerForecastWheather.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimerForecastWheather.Start();
        }

        private static string ParseTemperature(double value)
        {
            return ((int)(value - 273.15)).ToString(CultureInfo.InvariantCulture) + " °C";
        }

        public class WeatherForecast
        {
            public BitmapImage Icon;
            public string Temperature;
            public string Description;
            public string DayOfTheWeek;
        }

        private WeatherForecast RetrieveWeatherForecast(IEnumerable<ForecastTime> forecastTime, int day)
        {
            try
            {
                var amanha0Temp = forecastTime.First(d => d.From.Day == day && d.From.Hour >= 9);

                var forecast = new WeatherForecast
                {
                    Icon = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{amanha0Temp.Symbol.Var}.png",
                        UriKind.Absolute)),
                    //Temperature = ParseTemperature(amanha0Temp.Temperature.Value),
                    Temperature = amanha0Temp.Temperature.Value + "ºC",
                    //Description = _foreacastWheaterIconList[amanha0Temp.Symbol.Name],
                    Description = amanha0Temp.Symbol.Name,
                    DayOfTheWeek = amanha0Temp.From.ToString("dddd", new CultureInfo("pt-BR"))
                };

                return forecast;
            }
            catch (Exception e)
            {
                var adfa = e;

            }

            return null;
        }

        private void SetWeatherForecast(
            IEnumerable<ForecastTime> forecastTime,
            int day,
            Image image,
            TextBlock txtTemp,
            TextBlock txtDesc,
            TextBlock txtDayOfWeek)
        {
            try
            {

                var amanha0Temp = RetrieveWeatherForecast(forecastTime, day);
                if (amanha0Temp == null) return;
                image.Source = amanha0Temp.Icon;
                txtTemp.Text = amanha0Temp.Temperature;
                txtDesc.Text = amanha0Temp.Description;
                txtDayOfWeek.Text = amanha0Temp.DayOfTheWeek;
            }
            catch (Exception e)
            {
                var adfa = e;

            }
        }

        private async void DispatcherTimer_Tick_Forecast(object sender, object e)
        {

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    var dateTimeNow = PegaHoraBrasilia();
                    const int portoAlegreId = 3452925;
                    var currentWeather = await _openWeatherMapClient.CurrentWeather.GetByCityId(portoAlegreId, MetricSystem.Metric, OpenWeatherMapLanguage.PT);
                    ImgTemp.Source = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{currentWeather.Weather.Icon}.png", UriKind.Absolute));
                    //TxtTemp.Text = ParseTemperature(currentWeather.Temperature.Value);
                    TxtTemp.Text = currentWeather.Temperature.Value + " °C";
                    //TxtTempDescription.Text = _foreacastWheaterIconList[currentWeather.Weather.Value];
                    TxtTempDescription.Text = currentWeather.Weather.Value;
                    TxtHoje.Text = dateTimeNow.ToString("dddd", new CultureInfo("pt-BR"));

                    var forecast = (await _openWeatherMapClient.Forecast.GetByCityId(portoAlegreId, false, MetricSystem.Metric, OpenWeatherMapLanguage.PT)).Forecast;

                    SetWeatherForecast(forecast, dateTimeNow.Day + 1, ImgTemp0, TxtAmanha0Temp, TxtAmanha0Desc,
                            TxtAmanha0);

                    SetWeatherForecast(forecast, dateTimeNow.Day + 2, ImgTemp1, TxtAmanha1Temp, TxtAmanha1Desc,
                            TxtAmanha1);

                    SetWeatherForecast(forecast, dateTimeNow.Day + 3, ImgTemp2, TxtAmanha2Temp, TxtAmanha2Desc,
                            TxtAmanha2);

                    _dispatcherTimerForecastWheather.Interval = new TimeSpan(0, 1, 0);
                }
                catch (Exception exception)
                {
                    var xx = exception;
                }
            });

        }
    }
}
