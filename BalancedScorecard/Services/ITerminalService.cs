using BalancedScorecard.Components.Controls;

namespace BalancedScorecard.Services
{
    public interface ITerminalService
    {
        EventConsole? Console { get; set; }
        Task ExecuteCommand(string command);
    }
}
