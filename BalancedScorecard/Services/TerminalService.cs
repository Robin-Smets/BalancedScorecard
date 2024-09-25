using BalancedScorecard.Components.Controls;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;

namespace BalancedScorecard.Services
{
    public class TerminalService : ITerminalService
    {
        public EventConsole? Console { get; set; }
        private IMLService _mLService;
        
        public TerminalService(IMLService mLService) 
        { 
            _mLService = mLService;
        }

        public async Task ExecuteCommand(string command)
        {
            Console.Log($"Command: {command}");
            switch (command)
            {
                case "ml load data":
                    await _mLService.LoadData();
                    break;

                case "ml train model":
                    await _mLService.TrainModel();
                    break;
                default:
                    Console.Log($"Not a valid command");
                    return;
            }
            Console.Log("Command executed");
        }
    }
}
