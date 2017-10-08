using System;
using System.Globalization;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace TCC
{
    public sealed partial class MainPage
    {
        public void DispatcherTimerClock()
        {
            _dispatcherTimerClock = new DispatcherTimer();
            _dispatcherTimerClock.Tick += DispatcherTimer_Tick_Clock;
            _dispatcherTimerClock.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimerClock.Start();
        }

        private async void DispatcherTimer_Tick_Clock(object sender, object e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtHour.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("pt-BR"));
            });
        }
    }
}
