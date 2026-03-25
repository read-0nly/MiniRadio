using System.Net;
using System.Text;

namespace MiniRadio
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MiniRadio());//http://localhost:8001/Beautiful%20Midnight/Suburbia.mp3
        }
    }
}