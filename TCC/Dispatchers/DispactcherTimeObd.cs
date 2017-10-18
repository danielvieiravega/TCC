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
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {                    
                    var currentSpeed = await _obdDriver.GetSpeed();
                    var speed = currentSpeed > MaxSpeed ? MaxSpeed : currentSpeed;
                    
                    RadialBarGaugeSpeed.Value = speed;
                    TxtSpeed.Text = speed + " km/h";

                    var rpm = await _obdDriver.GetRpm();
                    var normalizedRpm = Math.Round(rpm / 1000);
                    TxtRpm.Text = normalizedRpm + " x 1000 rpm";
                    RadialBarGaugeRpm.Value = normalizedRpm;

                    var fuelTankLevel = await _obdDriver.GetFuelTankLevelInput();
                    if (Math.Abs(fuelTankLevel) > 0 && fuelTankLevel <= 100.0)
                    {
                        BarFuelLevel.Value = fuelTankLevel;
                    }

                    var engineTemp = await _obdDriver.GetEngineTemperature();
                    if (engineTemp > 0 )
                    {
                        TxtTempEngine.Text = engineTemp + " °C";
                    }
                    
                    var intakeTemp = await _obdDriver.GetIntakeAirTemperature();
                    if (intakeTemp > 0)
                    {
                        TxtTempIntake.Text = intakeTemp + " °C";
                    }

                    var throtlePos = await _obdDriver.GetThrottlePosition();
                    if (throtlePos > 0)
                    {
                        TxtThrotlePosition.Text = (int)throtlePos + " %";
                    }

                    var fuelPres = await _obdDriver.GetFuelPressure();
                    if (fuelPres > 0)
                    {
                        TxtFuelPressure.Text = fuelPres + " kPa";
                    }
                    
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
            catch (Exception ex)
            {
                var sdfa = ex;
                // ignored
            }
        }
    }
}
