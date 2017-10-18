﻿using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using MetroLog;
using TCC.ODBDriver.Services;
using System.Linq;

namespace TCC.ODBDriver
{
    public class ObdDriver
    {
        private DeviceInformationCollection _deviceCollection;
        private DeviceInformation _selectedDevice;
        private RfcommDeviceService _deviceService;
        private DataReader _reader;
        private DataWriter _writer;

        private StreamSocket _streamSocket;
        
        public async Task<bool> InitializeConnection()
        {
            var result = false;
            var device = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            _deviceCollection = await DeviceInformation.FindAllAsync(device);

            if (_deviceCollection.Count > 0)
            {
                _selectedDevice = _deviceCollection[0];
                if (_selectedDevice != null)
                    _deviceService = await RfcommDeviceService.FromIdAsync(_selectedDevice.Id);

                if (_deviceService != null)
                {
                    try
                    {
                        _streamSocket = new StreamSocket();

                        await _streamSocket.ConnectAsync(_deviceService.ConnectionHostName,
                            _deviceService.ConnectionServiceName);

                        SetupStreams();

                        await SendInitializationCommands();

                        result = true;
                    }
                    catch (Exception e)
                    {
                        LoggingServices.Instance.WriteLine<ObdDriver>($"Failure sending initialization commands: {e.Message}", LogLevel.Fatal);
                    }
                    
                }
            }

            return result;
        }

        private void SetupStreams()
        {
            _reader = new DataReader(_streamSocket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };
            _writer = new DataWriter(_streamSocket.OutputStream);
        }

        /// <summary>
        /// Initializes the communication with the ELM327
        /// </summary>
        private async Task SendInitializationCommands()
        {
            await SendCommand("ATZ\r");
            
            await SendCommand("ATSP6\r");

            await SendCommand("ATH0\r");

            await SendCommand("ATCAF1\r");
        }

        private async Task<string> SendCommand(string command)
        {
            var result = "Failure sending command!";
            try
            {
                _writer.WriteString(command);
                await _writer.StoreAsync();
                await _writer.FlushAsync();

                IAsyncOperation<uint> count = _reader.LoadAsync(512);
                count.AsTask().Wait();

                result = _reader.ReadString(count.GetResults());
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure sending sending command: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        private static string GetCommand(Mode mode, PID pid)
        {
            return $"{Convert.ToUInt32(mode):X2}{Convert.ToUInt32(pid):X2}\r";
        }

        public async Task<double> GetSpeed()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.Speed);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting speed: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        public async Task<double> GetRpm()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.EngineRpm);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting RPM: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        public async Task<double> GetEngineTemperature()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.EngineTemperature);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting EngineTemperature: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        public async Task<double> GetFuelPressure()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.FuelPressure);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting FuelPressure: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        public async Task<double> GetThrottlePosition()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.ThrottlePosition);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting ThrottlePosition: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        public async Task<double> GetIntakeAirTemperature()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.IntakeAirTemperature);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting IntakeAirTemperature: {e.Message}", LogLevel.Warn);
            }

            return result;
        }

        private async Task<double> RetrieveData(Mode mode, PID pid)
        {
            var result = 0.0;
            const int retries = 3;

            var cmd = GetCommand(mode, pid);
            var response = await SendCommand(cmd);

            var pidString = pid.ToString("X").Replace("000000", "");

            for (var i = 0; i < retries; i++)
            {
                if (!response.Contains("41") && !response.Contains(pidString))
                {
                    response = await SendCommand(cmd);
                }
                else
                {
                    break;
                }
            }

            if (response.Contains(pidString))
            {
                response = NormalizeResponse(response, $"41{pidString}");
                result = ParseData(response, pid);
            }

            return result;
        }

        public async Task<double> GetFuelTankLevelInput()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID.FuelTankLevelInput);
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure getting FuelTankLevelInput: {e.Message}", LogLevel.Warn);
            }

            return result;
        }
        
        private static double ParseData(string response, PID pid)
        {
            var result = 0.0;
            if (response.Length <= 4)
                return result;
            try
            {
                switch (pid)
                {
                    case PID.EngineRpm:
                        var rpmInHex = response.Substring(4);
                        var rpmA = rpmInHex.Substring(0, 2);
                        var rpmB = rpmInHex.Substring(2);
                        var rpmAInt = Convert.ToInt32(rpmA, 16);
                        var rpmBInt = Convert.ToInt32(rpmB, 16);
                        var rpm = ((256 * rpmAInt) + rpmBInt) / 4;
                        result = Convert.ToDouble(rpm);
                        break;

                    case PID.Speed:
                        var speedHexString = response.Substring(4);
                        var speed = Convert.ToInt32(speedHexString, 16);
                        result = Convert.ToDouble(speed);
                        break;

                    case PID.EngineTemperature:
                        var engineTemperatureHexString = response.Substring(4);
                        var engineTemperature = Convert.ToInt32(engineTemperatureHexString, 16);
                        result = Convert.ToDouble(engineTemperature) - 40;
                        break;

                    case PID.FuelPressure:
                        var fuelPressureHexString = response.Substring(4);
                        var fuelPressure = Convert.ToInt32(fuelPressureHexString, 16);
                        result = Convert.ToDouble(fuelPressure) * 3;
                        break;

                    case PID.ThrottlePosition:
                        var throttlePositionHexString = response.Substring(4);
                        var throttlePosition = Convert.ToInt32(throttlePositionHexString, 16);
                        result = Convert.ToDouble(throttlePosition) * 100 / 255;
                        break;

                    case PID.IntakeAirTemperature:
                        var intakeAirTemperatureHexString = response.Substring(4);
                        var intakeAirTemperature = Convert.ToInt32(intakeAirTemperatureHexString, 16);
                        result = Convert.ToDouble(intakeAirTemperature) - 40;
                        break;

                    case PID.FuelTankLevelInput:
                        var fuelTankLevelHexString = response.Substring(4);
                        var fuelTankLevel = Convert.ToInt32(fuelTankLevelHexString, 16);
                        result = Convert.ToDouble(fuelTankLevel) * 100 / 255;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(pid), pid, null);
                }
            }
            catch (Exception e)
            {
                LoggingServices.Instance.WriteLine<ObdDriver>($"Failure parsing data: {e.Message}", LogLevel.Warn);
            }

            return result;
        }
        
        private static string NormalizeResponse(string res, string delimiter)
        {
            var result = res.Replace(" ", "").Replace(">","");
            var resSplitted = result.Split('\r');
            var expectedRes = resSplitted.FirstOrDefault(x => x.Contains(delimiter));

            if (expectedRes != null)
                return expectedRes;
            
            return res.Replace("\r", "").Replace(" ", "");
        }
        
        public async Task Close()
        {
            if (_streamSocket != null)
            {
                await _streamSocket.CancelIOAsync();
                _streamSocket.Dispose();
                _streamSocket = null;
            }
            _deviceService.Dispose();
            _deviceService = null;
        }
    }
}
    