using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TsBeautify.Data;

namespace TsBeautify.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Preparing");
            var timer = new Stopwatch();
            Console.WriteLine("Starting");
            timer.Start();
            var files = Directory.EnumerateFiles(@"D:\Code\Brandless\Iql.Npm.Unformatted", "*.ts",
                SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var beautifier = new TsBeautifier();
                var text = File.ReadAllText(file);
                var result = beautifier.Beautify(text);
            }
            timer.Stop();
            Console.WriteLine($"Completed in {FormatTime(timer.ElapsedTicks)}");
            //Iterate(timer);
        }

        private static void Iterate(Stopwatch timer)
        {
            var typescript = TsBeautifyTs.Code;
            var iterations = 100;
            for (var i = 0; i < iterations; i++)
            {
                var beautifier = new TsBeautifier();
                var result = beautifier.Beautify(typescript);
            }

            timer.Stop();
            Console.WriteLine($"Completed {iterations} iterations in {FormatTime(timer.ElapsedTicks)}");
            Console.WriteLine($"Each iteration took an average of {FormatTime(timer.ElapsedTicks / iterations)}");
        }

        private static string FormatTime(long ticks)
        {
            TimeSpan t = TimeSpan.FromTicks(ticks);
            var timeParts = new List<string>();
            if (t.Hours > 0)
            {
                timeParts.Add(string.Format("{0:D2}h", t.Hours));
            }

            if (t.Minutes > 0)
            {
                timeParts.Add(string.Format("{0:D2}m", t.Minutes));
            }

            if (t.Seconds > 0)
            {
                timeParts.Add(string.Format("{0:D2}s", t.Seconds));
            }

            if (t.Milliseconds > 0)
            {
                timeParts.Add(string.Format("{0:D3}ms", t.Milliseconds));
            }

            var time = string.Join(":", timeParts);
            return time;
        }
    }
}
