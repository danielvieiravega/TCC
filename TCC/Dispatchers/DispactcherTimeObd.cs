using System;
using System.Globalization;
using Windows.UI.Core;
using Windows.UI.Xaml;
using ThingSpeakWinRT;
using System.Threading.Tasks;

namespace TCC
{
    public sealed partial class MainPage
    {
        public void DispatcherTimerOdb()
        {
            _dispatcherTimerOdb = new DispatcherTimer();
            _dispatcherTimerOdb.Tick += DispatcherTimer_Tick;
            _dispatcherTimerOdb.Interval = new TimeSpan(0, 0, 2);
            _dispatcherTimerOdb.Start();
        }
        
        private async void DispatcherTimer_Tick(object sender, object e)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        var data = await _obdDriver.GetAllData();

                        var currentSpeed = data.Speed;
                        var speed = currentSpeed > MaxSpeed ? MaxSpeed : currentSpeed;
                        RadialBarGaugeSpeed.Value = speed;
                        TxtSpeed.Text = speed + " km/h";
                        await Task.Delay(50);
                        var rpm = data.RPM;
                        var normalizedRpm = rpm / 1000.0;
                        TxtRpm.Text = normalizedRpm.ToString("#.#") + " x 1000 rpm";
                        RadialBarGaugeRpm.Value = normalizedRpm;
                        await Task.Delay(50);
                        var fuelTankLevel = data.FuelTankLevelInput;
                        if (Math.Abs(fuelTankLevel) > 0 && fuelTankLevel <= 100.0)
                        {
                            BarFuelLevel.Value = fuelTankLevel;
                        }
                        await Task.Delay(50);
                        var engineTemp = data.EngineTemperature;
                        if (engineTemp > 0)
                        {
                            TxtTempEngine.Text = engineTemp + " °C";
                        }
                        await Task.Delay(50);
                        var intakeTemp = data.IntakeAirTemperature;
                        if (intakeTemp > 0)
                        {
                            TxtTempIntake.Text = intakeTemp + " °C";
                        }
                        await Task.Delay(50);
                        //var throtlePos = await _obdDriver.GetThrottlePosition();
                        //if (throtlePos > 0)
                        //{
                        //    TxtThrotlePosition.Text = (int)throtlePos + " %";
                        //}

                        //var fuelPres = await _obdDriver.GetFuelPressure();
                        //if (fuelPres > 0)
                        //{
                        //    TxtFuelPressure.Text = fuelPres + " kPa";
                        //}

                        try
                        {
                            var dataFeed = new ThingSpeakFeed
                            {
                                Field1 = speed.ToString(CultureInfo.InvariantCulture),
                                Field2 = rpm.ToString(CultureInfo.InvariantCulture),
                                //Field3 = fuelPres.ToString(CultureInfo.InvariantCulture),
                                Field4 = engineTemp.ToString(CultureInfo.InvariantCulture),
                                //Field5 = throtlePos.ToString(CultureInfo.InvariantCulture),
                                Field6 = intakeTemp.ToString(CultureInfo.InvariantCulture),
                                Field7 = fuelTankLevel.ToString(CultureInfo.InvariantCulture),
                            };
                            await _thingSpeakClient.UpdateFeedAsync(ThingSpeakWriteApiKey, dataFeed);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
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
