using BalancedScorecard.Components.Controls;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;

namespace BalancedScorecard.Services
{
    public class TerminalService : ITerminalService
    {
        public EventConsole? Console { get; set; }

        private readonly IServiceProvider _services;
        private IMLService _mLService => _services.GetRequiredService<IMLService>();
        
        public TerminalService(IServiceProvider services) 
        { 
            _services = services;
        }

        public async Task ExecuteCommand(string command)
        {
            Console.Log($"Command: {command}");
            //if (command.StartsWith("ml train optimized model "))
            //{
            //    var commandSuffix = command.Replace("ml train optimized model ", "");

            //    try
            //    {
            //        if (commandSuffix != "" && uint.Parse(commandSuffix) is uint seconds)
            //        {
            //            await _mLService.TrainModelWithAutoOptimizationAsync(seconds);
            //        }
            //        else
            //        {
            //            Console.Log($"Not a valid command");
            //            return;
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        Console.Log($"Not a valid command");
            //        return;
            //    }

            //}
            //else
            //{
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
                //}
            }

            Console.Log("Command executed");
        }
    }
}
