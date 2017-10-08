using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace TCC
{
    public sealed partial class MainPage
    {
        public void DispatcherTimerSetupSms()
        {
            _dispatcherTimerSms = new DispatcherTimer();
            _dispatcherTimerSms.Tick += DispatcherTimer_Tick_Sms;
            _dispatcherTimerSms.Interval = new TimeSpan(0, 0, 10);
            _dispatcherTimerSms.Start();
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
    }
}
