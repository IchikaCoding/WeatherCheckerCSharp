using System.Diagnostics;

namespace WeatherCheckerCSharp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var s = new Sample();
            //s.Value = (1, 1);
            //(int x, int y) t = s.GetValue();
            //Debug.WriteLine(t); // (1, 1)

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}