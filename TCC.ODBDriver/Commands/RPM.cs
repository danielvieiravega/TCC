using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace TCC.ODBDriver.Commands
{
    public class RPM : Command, ICommand
    {
        public RPM(DataReader reader, DataWriter writer) : base(reader, writer)
        {
        }

        public override PID PID => PID.EngineRpm;
        
        public async Task<double> GetValue()
        {
            var result = 0.0;
            try
            {
                result = await RetrieveData(Mode.CurrentData, PID);
            }
            catch (Exception)
            {
                //Ignored
            }

            return result;
        }

        public override double ParseData(string response, PID pid)
        {
            var rpmInHex = response.Substring(4);
            var rpmA = rpmInHex.Substring(0, 2);
            var rpmB = rpmInHex.Substring(2);
            var rpmAInt = Convert.ToInt32(rpmA, 16);
            var rpmBInt = Convert.ToInt32(rpmB, 16);
            var rpm = ((256 * rpmAInt) + rpmBInt) / 4;
            var result = Convert.ToDouble(rpm);

            return result;
        }
    }
}
