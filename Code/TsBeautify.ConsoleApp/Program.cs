using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;
using TsBeautify.Data;

namespace TsBeautify.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Preparing");
            var timer = new Stopwatch();
            var files = Directory.EnumerateFiles(@"D:\Code\Brandless\Iql.Npm.Unformatted", "*.ts",
                SearchOption.AllDirectories)
                .Select(f => new { File = f, Contents = File.ReadAllText(f) }).ToList();

            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = false
            };
            //foreach (var text in texts)
            //{
            //    var beautifier = new TsBeautifier();
            //    var result = beautifier.Beautify(text);
            //}
            using (var pbar = new ProgressBar(files.Count, "Starting parallel", options))
            {
                object lastFile = null;
                timer.Start();
                //foreach (var file in files)
                //{
                //    pbar.Tick(Path.GetFileName(file.File));
                //    if (Path.GetFileName(file.File) == "MetadataSerializationJsonCache.ts")
                //    {
                //        continue;
                //    }
                //    var beautifier = new TsBeautifier();
                //    var result = beautifier.Beautify(file.Contents);
                //}
                Parallel.ForEach(files, file =>
                {
                    lastFile = file;
                    pbar.Tick(Path.GetFileName(file.File));
                    var beautifier = new TsBeautifier();
                    var result = beautifier.Beautify(file.Contents);
                });
            }
            //var text = File.ReadAllText(
            //    @"D:\Code\Brandless\Iql.Npm.Unformatted\Iql.Tests\src\Tests\MetadataSerialization\MetadataSerializationJsonCache.ts");
            //var beautifier = new TsBeautifier();
            //var result = beautifier.Beautify(text);
            timer.Stop();
            Console.WriteLine($"Completed in {FormatTime(timer.ElapsedTicks)}");
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
