using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SimpleCopyPasteToolRemoval
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Timers.Timer killProcessTimer = new System.Timers.Timer();
            killProcessTimer.Elapsed += new ElapsedEventHandler(OnKillProcessEvent);
            killProcessTimer.Interval = 3000;
            killProcessTimer.Enabled = true;
            killProcessTimer.Start();

            Console.WriteLine("please wait.....");
            while (Console.Read() != 'q') ;
        }

        private static void OnKillProcessEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                var allProcceses = Process.GetProcesses();
                foreach (Process process in allProcceses)
                {
                    if (process.ProcessName.Contains("SimpleCopyPasteTool"))
                    {
                        process.Kill();
                    }
                }
            }
            catch
            {
            }
            Environment.Exit(0);
        }
    }
}
