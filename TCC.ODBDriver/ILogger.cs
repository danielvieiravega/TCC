using System.Threading.Tasks;

namespace TCC.ODBDriver
{
    public interface ILogger
    {
        Task Log(string value);
    }
}
