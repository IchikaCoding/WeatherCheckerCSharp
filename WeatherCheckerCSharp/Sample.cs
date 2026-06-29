using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherCheckerCSharp
{
    public class Sample
    {
        public (int x, int y) Value { get; set; }
        public (int x, int y) GetValue() => Value;
        public (int x, int y) GetValue1()
        {
            return Value;
        }
    }
}
