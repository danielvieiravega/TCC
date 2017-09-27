using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TCC.ODBDriver;
using ThingSpeakWinRT;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TCC
{
    public sealed partial class MainPage : Page
    {
        private readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimer;
        public bool IsClosed { get; set; }
        private readonly ObdDriver _obdDriver = new ObdDriver();
        private readonly Sim800Driver.Sim800Driver _sim800Driver = new Sim800Driver.Sim800Driver(); 
        private readonly ThingSpeakClient _thingSpeakClient = new ThingSpeakClient(false);
        private const string WriteApiKey = "DM63F2BD1CS70GJC";
        
        public MainPage()
        {
            IsClosed = true;
            InitializeComponent();

            TxtSms.Text = string.Empty;

            //TxtSms.Text =
            //    @"Os USRTs são utilizados em sistemas de comunicação para aplicações específicas, com sincronização feita por hardware e a muito curtas distâncias.";
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
                            TxtSms.Text = $"De: {sms.Sender}\nMensagem: {sms.Message}";
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
                    var speed = await _obdDriver.GetSpeed();
                    var rpm = await _obdDriver.GetRpm();
                    var engineTemp = await _obdDriver.GetEngineTemperature();
                    var intakeTemp = await _obdDriver.GetIntakeAirTemperature();
                    var throtlePos = await _obdDriver.GetThrottlePosition();
                    var fuelPres = await _obdDriver.GetFuelPressure();

                    //GaugeSpeed.Value = speed;
                    //GaugeRpm.Value = rpm;
                    TxtTempEngine.Text = engineTemp + " °C";
                    TxtTempIntake.Text = intakeTemp + " °C";
                    TxtFuelPressure.Text = fuelPres + " kPa";
                    TxtThrotlePosition.Text = (int)throtlePos + " %";

                    var sms = await _sim800Driver.ReadSms();
                    //if (sms != "nothing")

                    try
                    {
                        var dataFeed = new ThingSpeakFeed
                        {
                            Field1 = speed.ToString(),
                            Field2 = rpm.ToString(),
                            Field3 = fuelPres.ToString(),
                            Field4 = engineTemp.ToString(),
                            Field5 = throtlePos.ToString(),
                            Field6 = intakeTemp.ToString()
                        };
                        await _thingSpeakClient.UpdateFeedAsync(WriteApiKey, dataFeed);
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

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await _sim800Driver.InitializeConnection())
                {
                    DispatcherTimerSetupSms();
                    //var messages = await _sim800Driver.ReadSms();
                    //var sms = messages.FirstOrDefault();
                    //if (sms != null)
                    //{
                    //    var isSmsRead = sms.Status.Contains("READ");
                    //    if (isSmsRead)
                    //    {
                    //        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //        {
                    //            TxtSms.Text = $"De: {sms.Sender}\nMensagem: {sms.Message}";
                    //        });
                    //    }
                    //}
                }

                //var x = await _sim800Driver.SendSms("Enviando novamente um SMS", "51989108383");
                
            }
            catch (Exception exception)
            {
                var x = exception;
               
            }
            

            //try
            //{
            //    if (await _obdDriver.InitializeConnection("danielvv"))
            //    {
            //        DispatcherTimerSetup();
            //        IsClosed = false;
            //    }
            //}
            //catch (Exception)
            //{
            //    // ignored
            //}
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
            //if (IsClosed)
            //{
            //    DispatcherTimerSetup();
            //    IsClosed = false;
            //}

            RadialBarGaugeSpeed.Value = value;
            MarkerGaugeSpeed.Value = value;
            TxtSpeed.Text = value + " km/h";

            TxtTempEngine.Text = value + " °C";
            TxtTempIntake.Text = value + " °C";
            //GaugeRpm.Value = value;
            //GaugeSpeed.Value = value;
            TxtFuelPressure.Text = value + " KpA";
            TxtThrotlePosition.Text = value + " %";

            value += 2;
        }

        private async void BtnClearSms_Click(object sender, RoutedEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtSms.Text = string.Empty;
            });
        }
    }
}
