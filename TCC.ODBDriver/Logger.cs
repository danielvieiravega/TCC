using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace TCC.ODBDriver
{
    public class Logger : ILogger
    {
        private readonly CoreDispatcher _coreDispatcher;
        private readonly TextBox _txtBox;

        public Logger(CoreDispatcher dispatcher, TextBox txtBox)
        {
            _txtBox = txtBox;
            _coreDispatcher = dispatcher;
        }

        public async Task Log (string value)
        {
            await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _txtBox.Text = string.Format("Status: {0}", value);
            });       
        }
    }
}
