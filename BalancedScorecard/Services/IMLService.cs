using Microsoft.ML;

namespace BalancedScorecard.Services
{
    public interface IMLService
    {
        public IDataView Data { get; }
        public IDataView TrainData { get; }
        public IDataView TestData { get; }
        Task LoadData();
        Task TrainModel();
        Task TrainModelWithAutoOptimizationAsync(uint seconds);
        Task CheckMissingValuesInAllColumns(IDataView data);
    }
}
