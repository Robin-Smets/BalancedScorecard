namespace BalancedScorecard.Services
{
    public interface IMLService
    {
        Task LoadData();
        Task TrainModel();
    }
}
