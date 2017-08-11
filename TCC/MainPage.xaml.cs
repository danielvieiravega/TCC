using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416
namespace TCC
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DataReader _reader;
        private DataWriter _writer;
        readonly CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        private DispatcherTimer _dispatcherTimer;
        private readonly ObdProvider _obdProvider;

        public MainPage()
        {
            this.InitializeComponent();

            _obdProvider = new ObdProvider("danielvv");
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            GetSpeed();
            GetRpm();
        }
        
        private void SetupStreams()
        {
            _reader = new DataReader(_obdProvider.StreamSocket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };

            _writer = new DataWriter(_obdProvider.StreamSocket.OutputStream);
        }

        /// <summary>
        /// Initializes the communication with the ELM327
        /// </summary>
        private async Task SendInitializationCommands()
        {
            SetupStreams();

            await SendCommand("ATZ\r");
            await SendCommand("ATSP6\r");
            await SendCommand("ATH0\r");
            await SendCommand("ATCAF1\r");
        }

        private async Task<string> SendCommand(string command)
        {
            _writer.WriteString(command);
            await _writer.StoreAsync();
            await _writer.FlushAsync();
            var count = await _reader.LoadAsync(512);
            return _reader.ReadString(count).Trim('>');
        }

        private async void GetSpeed()
        {
            var response = await SendCommand("010D\r");
            await DecodeAndShowSpeed(response);
        }

        private async void GetRpm()
        {
            var response = await SendCommand("010C\r");
            await DecodeAndShowRpm(response);
        }

        private async Task DecodeAndShowSpeed(string response)
        {
            var speedHexString = response.Substring(4);
            var speed = DecodeHexNumber(speedHexString);
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GaugeSpeed.Value = speed;
            });
        }

        private async Task DecodeAndShowRpm(string response)
        {
            var rpmString = response.Substring(4);
            var rpm = (Convert.ToDouble(DecodeHexNumber(response.Substring(4))));
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GaugeRpm.Value = rpm;
            });
        }

        private static int DecodeHexNumber(string hexString)
        {
            var raw = new byte[hexString.Length / 2];

            for (var i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Convert.ToInt32(raw[0].ToString());
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            await _obdProvider.ConfigureConnectionToElmAdapter();
            await SendInitializationCommands();
            DispatcherTimerSetup();
        }
    }
}
