using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherCheckerCSharp
{
    public class CityNotFoundException: Exception
    {
        public CityNotFoundException(string city): base($"「{city}」が取得出来ませんでした") {}
    }
}
