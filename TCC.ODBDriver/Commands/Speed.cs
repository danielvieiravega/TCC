using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace TCC.ODBDriver.Commands
{
    public class Speed : Command, ICommand
    {
        public Speed(DataReader reader, DataWriter writer) 
            : base(reader, writer)
        {
        }

        public override PID PID { get => PID.Speed; }

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
            var speedHexString = response.Substring(4);
            var speed = Convert.ToInt32(speedHexString, 16);
            var result = Convert.ToDouble(speed);

            return result;
        }
    }
}
