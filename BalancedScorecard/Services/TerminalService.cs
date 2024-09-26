using BalancedScorecard.Components.Controls;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;
using System.Diagnostics;

namespace BalancedScorecard.Services
{
    public class TerminalService : ITerminalService
    {
        public EventConsole? Console { get; set; }
        private IMLService _mLService => _services.GetRequiredService<IMLService>();
        private IServiceProvider _services;
        
        public TerminalService(IServiceProvider services) 
        {
            _services = services;
        }

        public async Task MeasurePerformanceAsync(int iterations, Func<Task> task)
        {
            // Zeitmessung starten
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Schleife durchlaufen und Task ausführen
            for (int i = 0; i < iterations; i++)
            {
                await task();
            }

            // Zeitmessung beenden
            stopwatch.Stop();
            long totalMilliseconds = stopwatch.ElapsedMilliseconds;

            // Durchschnittliche Zeit pro Task
            double averageTimePerTask = (double)totalMilliseconds / iterations;

            // Ergebnis in der Konsole ausgeben
            Console.Log($"Gesamtzeit: {totalMilliseconds} ms für {iterations} Durchläufe.");
            Console.Log($"Durchschnittliche Zeit pro Task: {averageTimePerTask} ms.");
        }

        public async Task ExecuteCommand(string command)
        {
            Console.Log($"Command: {command}");
            if (command.StartsWith("ml train optimized model "))
            {
                var commandSuffix = command.Replace("ml train optimized model ", "");

                try
                {
                    if (commandSuffix != "" && uint.Parse(commandSuffix) is uint seconds)
                    {
                        await _mLService.TrainModelWithAutoOptimizationAsync(seconds);
                    }
                    else
                    {
                        Console.Log($"Not a valid command");
                        return;
                    }
                }
                catch (Exception)
                {
                    Console.Log($"Not a valid command");
                    return;
                }

            }
            else if (command.StartsWith("test performance load data "))
            {
                var commandSuffix = command.Replace("test performance load data ", "");

                try
                {
                    if (commandSuffix != "" && int.Parse(commandSuffix) is int iterations)
                    {
                        await MeasurePerformanceAsync(iterations, _services.GetRequiredService<IDataStoreService>().LoadData);
                    }
                    else
                    {
                        Console.Log($"Not a valid command");
                        return;
                    }
                }
                catch (Exception)
                {
                    Console.Log($"Not a valid command");
                    return;
                }

            }
            else
            {
                switch (command)
                {
                    case "ml load data":
                        await _mLService.LoadData();
                        break;

                    case "ml train model":
                        await _mLService.TrainModel();
                        break;

                    case "ml check train":
                        await _mLService.CheckMissingValuesInAllColumns(_mLService.TrainData);
                        break;

                    case "ml check test":
                        await _mLService.CheckMissingValuesInAllColumns(_mLService.TestData);
                        break;

                    case "ml check data":
                        await _mLService.CheckMissingValuesInAllColumns(_mLService.Data);
                        break;

                    default:
                        Console.Log($"Not a valid command");
                        return;
                }
            }

            Console.Log("Command executed");
        }
    }
}
