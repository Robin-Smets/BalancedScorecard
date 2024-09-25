

using BalancedScorecard.ML.DataModel;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BalancedScorecard.Services
{
    public class MLService : IMLService
    {
        private readonly MLContext _mLContext;
        private readonly EstimatorChain<RegressionPredictionTransformer<LightGbmRegressionModelParameters>> _pipeLine;
        private IDataView? _data;
        private IDataView? _trainData;
        private IDataView? _testData;
        private IDataStoreService _dataStoreService;

        public MLService(IDataStoreService dataStoreService) 
        {
            _mLContext = new MLContext();
            // Pipeline definieren
            _pipeLine = _mLContext.Transforms.Concatenate("Features", new[] {"ProductID", "SalesPersonID", "TerritoryID", "CustomerID"})
                          .Append(_mLContext.Regression.Trainers.LightGbm(labelColumnName: "OrderVolume"));
            _dataStoreService = dataStoreService;
        }

        public async Task LoadData()
        {
            var dataTable = _dataStoreService.DataTables["OrderVolume"];
            var orderVolumeList = dataTable.AsEnumerable().Select(row => new OrderVolumeModel
            {
                OrderDateCalenderWeek = Int32.Parse(row.Field<string>("OrderDateCalenderWeek")),
                OrderDateMonth = Int32.Parse(row.Field<string>("OrderDateMonth")),
                OrderDateQuarter = Int32.Parse(row.Field<string>("OrderDateQuarter")),
                TimeUnitCalenderWeek = row.Field<string>("TimeUnitCalenderWeek"),
                TimeUnitMonth = row.Field<string>("TimeUnitCalenderWeek"),
                TimeUnitQuarter = row.Field<string>("TimeUnitCalenderWeek"),
                OrderDate = row.Field<DateTime>("OrderDate"),
                SalesOrderID = Int32.Parse(row.Field<string>("SalesOrderID")),
                CustomerID = Int32.Parse(row.Field<string>("CustomerID")),
                CustomerName = row.Field<string>("CustomerName"),
                TerritoryID = Int32.Parse(row.Field<string>("TerritoryID")),
                TerritoryName = row.Field<string>("TerritoryName"),
                SalesPersonID = Int32.Parse(row.Field<string>("SalesPersonID")),
                SalesPersonName = row.Field<string>("SalesPersonName"),
                ProductID = Int32.Parse(row.Field<string>("ProductID")),
                ProductName = row.Field<string>("ProductName"),
                OrderVolume = float.Parse(row.Field<string>("OrderVolume")),
                TotalOrderVolume = float.Parse(row.Field<string>("TotalOrderVolume")),
                OrderVolumePercentage = float.Parse(row.Field<string>("OrderVolumePercentage"))
            });
            _data = _mLContext.Data.LoadFromEnumerable(orderVolumeList);
            var split = _mLContext.Data.TrainTestSplit(_data, testFraction: 0.2);
            _trainData = split.TrainSet;
            _testData = split.TestSet;
        }

        public async Task TrainModel()
        {
            // Modell trainieren
            var model = _pipeLine.Fit(_trainData);

            // Modell evaluieren
            var predictions = model.Transform(_testData);
            var metrics = _mLContext.Regression.Evaluate(_data, labelColumnName: "OrderVolume");

            // Metriken in der Konsole ausgeben
            Console.WriteLine($"R²: {metrics.RSquared:0.##}");
            Console.WriteLine($"MAE: {metrics.MeanAbsoluteError:0.##}");
            Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError:0.##}");
        }
    }
}
