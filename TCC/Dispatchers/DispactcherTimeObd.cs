using System;
using System.Globalization;
using Windows.UI.Core;
using Windows.UI.Xaml;
using ThingSpeakWinRT;

namespace TCC
{
    public sealed partial class MainPage
    {
        public void DispatcherTimerOdb()
        {
            _dispatcherTimerOdb = new DispatcherTimer();
            _dispatcherTimerOdb.Tick += DispatcherTimer_Tick;
            _dispatcherTimerOdb.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimerOdb.Start();
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            if (IsClosed)
            {
                _dispatcherTimerOdb.Stop();
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
    }
}
