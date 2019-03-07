using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace HostHealthServiceDemo
{
    class HostHealthService
    {
        protected static PerformanceCounter cpuCounter;
        protected static PerformanceCounter ramCounter;
        protected static PerformanceCounter diskCounter;
        public float cpu { get; set; }
        public float ram { get; set; }
        public float disk { get; set; }

        public HostHealthService()
        {
            DetectCPUHealth();
        }

        private void DetectCPUHealth()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes in Use");
            diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

            try
            {
                System.Timers.Timer t = new System.Timers.Timer(1000);
                t.Elapsed += new ElapsedEventHandler(TimerElapsed);
                t.Start();
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine("catched exception");
            }
            //Console.ReadLine();
        }
        public void TimerElapsed(object source, ElapsedEventArgs e)
        {
            float _cpu = cpuCounter.NextValue();
            float _ram = ramCounter.NextValue();
            float _disk = diskCounter.NextValue();

            cpu = _cpu;
            ram = _ram;
            disk = _disk;
        }

    }
}
