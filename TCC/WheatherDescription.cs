using System.Collections.Generic;

namespace TCC
{
    public static class WheatherDescription
    {
        public static Dictionary<string, string> GetDictionary()
        {
            return new Dictionary<string, string>
            {
                {"few clouds", "Poucas nuvens"},
                {"scattered clouds", "Nuvens dispersas"},
                {"broken clouds", "Nuvens quebradas"},
                {"shower rain", "Chuva de banho"},
                {"rain", "Chuva"},
                {"thunderstorm", "Tempestade"},
                {"snow", "Neve"},
                {"mist", "Névoa"},
                {"light rain", "Chuva leve"},
                {"moderate rain", "Chuva moderada"},
                {"heavy intensity rain", "Chuva pesada"},
                {"very heavy rain", "chuva muito pesada"},
                {"extreme rain", "Chuva extrema"},
                {"freezing rain", "chuva congelante"},
                {"light intensity shower rain", "Chuva leve"},
                {"heavy intensity shower rain", "chuva intensa"},
                {"ragged shower rain", "chuva esfarrapada"},
                {"overcast clouds", "Nublado"}
            };
        }
    }
}