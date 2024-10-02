using BalancedScorecard.ML.DataModel;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML.AutoML;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.ML.Transforms;

namespace BalancedScorecard.Services
{
    public class MLService : IMLService
    {
        private readonly MLContext _mLContext;
        private readonly EstimatorChain<RegressionPredictionTransformer<LightGbmRegressionModelParameters>> _pipeLine;
        public IDataView Data => _data;
        private IDataView? _data;
        public IDataView TrainData => _trainData;
        private IDataView? _trainData;
        public IDataView TestData => _testData;
        private IDataView? _testData;

        private readonly IServiceProvider _services;
        private IDataStoreService _dataStoreService => _services.GetRequiredService<IDataStoreService>();

        public MLService(IServiceProvider services) 
        {
            _mLContext = new MLContext();

            var options = new LightGbmRegressionTrainer.Options
            {
                NumberOfLeaves = 64,
                MinimumExampleCountPerLeaf = 10,
                LearningRate = 0.05,
                NumberOfIterations = 500,
                LabelColumnName = "Label"
            };

            _pipeLine = _mLContext.Transforms.CopyColumns("Label", "OrderVolume")
                .Append(_mLContext.Transforms.Conversion.ConvertType("OrderDate", "OrderDate", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("ProductID", "ProductID", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("SalesPersonID", "SalesPersonID", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("TerritoryID", "TerritoryID", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("CustomerID", "CustomerID", DataKind.Single))
                .Append(_mLContext.Transforms.Concatenate("Features", new[]
                {
                    "OrderDate", "ProductID", "SalesPersonID", "TerritoryID", "CustomerID"
                }))
                .Append(_mLContext.Transforms.NormalizeMeanVariance("Features"))
                .Append(_mLContext.Regression.Trainers.LightGbm(options));

            _services = services;
        }

        public async Task LoadData()
        {
            try
            {
                var dataTable = _dataStoreService.DataTables["OrderVolume"];
                var orderVolumeList = dataTable.AsEnumerable().Select(row => new OrderVolumeModel
                {
                    OrderDateCalenderWeek = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateCalenderWeek"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderDateCalenderWeek")),

                    OrderDateMonth = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateMonth"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderDateMonth")),

                    OrderDateQuarter = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateQuarter"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderDateQuarter")),

                    OrderDateYear = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateYear"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderDateYear")),

                    TimeUnitCalenderWeek = row.Field<string>("TimeUnitCalenderWeek"),
                    TimeUnitMonth = row.Field<string>("TimeUnitMonth"),
                    TimeUnitQuarter = row.Field<string>("TimeUnitQuarter"),

                    OrderDate = string.IsNullOrWhiteSpace(row.Field<string>("OrderDate"))
                        ? DateTime.MinValue
                        : DateTime.Parse(row.Field<string>("OrderDate")),

                    SalesOrderID = string.IsNullOrWhiteSpace(row.Field<string>("SalesOrderID"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("SalesOrderID")),

                    CustomerID = string.IsNullOrWhiteSpace(row.Field<string>("CustomerID"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("CustomerID")),

                    CustomerName = row.Field<string>("CustomerName"),

                    TerritoryID = string.IsNullOrWhiteSpace(row.Field<string>("TerritoryID"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("TerritoryID")),

                    TerritoryName = row.Field<string>("TerritoryName"),

                    SalesPersonID = string.IsNullOrWhiteSpace(row.Field<string>("SalesPersonID"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("SalesPersonID")),

                    SalesPersonName = row.Field<string>("SalesPersonName"),

                    ProductID = string.IsNullOrWhiteSpace(row.Field<string>("ProductID"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("ProductID")),

                    ProductName = row.Field<string>("ProductName"),

                    OrderVolume = string.IsNullOrWhiteSpace(row.Field<string>("OrderVolume"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderVolume")),

                    TotalOrderVolume = string.IsNullOrWhiteSpace(row.Field<string>("TotalOrderVolume"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("TotalOrderVolume")),

                    OrderVolumePercentage = string.IsNullOrWhiteSpace(row.Field<string>("OrderVolumePercentage"))
                        ? 0.0f
                        : float.Parse(row.Field<string>("OrderVolumePercentage"))
                });

                _data = _mLContext.Data.LoadFromEnumerable(orderVolumeList);
                var split = _mLContext.Data.TrainTestSplit(_data, testFraction: 0.2);
                _trainData = split.TrainSet;
                _testData = split.TestSet;
                Console.WriteLine("ML Service: data loaded");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public async Task TrainModel()
        {
            //try
            //{
            //    var model = _pipeLine.Fit(_trainData);
            //    var predictions = model.Transform(_testData);
            //    var metrics = _mLContext.Regression.Evaluate(predictions, labelColumnName: "Label");

            //    Console.WriteLine($"Mean Absolute Error: {metrics.MeanAbsoluteError}");
            //    Console.WriteLine($"Mean Squared Error: {metrics.MeanSquaredError}");
            //    Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError}");
            //    Console.WriteLine($"Average Loss Function: {metrics.LossFunction}");
            //    Console.WriteLine($"R-Squared: {metrics.RSquared}");

            //    _dataStoreService.EnsureDirectoryExistsAndHidden("./Models/");
            //    _mLContext.Model.Save(model, _trainData.Schema, "./Models/sales_forecast_model.zip");
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception.Message);
            //}

        }

        public async Task CheckMissingValuesInAllColumns(IDataView data)
        {
            foreach (var column in data.Schema)
            {
                var columnType = column.Type.RawType;
                if (columnType == typeof(float) || columnType == typeof(double))
                {
                    var missingValues = data.GetColumn<float>(column.Name).Where(x => float.IsNaN(x)).Count();
                    Console.WriteLine($"Spalte: {column.Name}, Fehlende Werte (NaN): {missingValues}");
                }
                else if (columnType == typeof(int))
                {
                    var missingValues = data.GetColumn<int>(column.Name).Where(x => x == 0).Count();
                    Console.WriteLine($"Spalte: {column.Name}, Fehlende Werte (Standardwert 0): {missingValues}");
                }
                else if (columnType == typeof(string))
                {
                    var missingValues = data.GetColumn<string>(column.Name).Where(x => string.IsNullOrWhiteSpace(x)).Count();
                    Console.WriteLine($"Spalte: {column.Name}, Fehlende Werte (Leere Strings): {missingValues}");
                }
                else
                {
                    Console.WriteLine($"Spalte: {column.Name} vom Typ {columnType} wird nicht auf fehlende Werte überprüft.");
                }
            }
        }

        // BUG: Does not recognize label column
        //public async Task TrainModelWithAutoOptimizationAsync(uint seconds)
        //{
        //    try
        //    {
        //        var experimentSettings = new RegressionExperimentSettings
        //        {
        //            MaxExperimentTimeInSeconds = seconds,
        //            OptimizingMetric = RegressionMetric.RSquared
        //        };

        //        var experiment = _mLContext.Auto().CreateRegressionExperiment(experimentSettings);
        //        var experimentResult = experiment.Execute(_trainData);
        //        var bestRun = experimentResult.BestRun;
        //        var bestModel = bestRun.Model;
        //        var predictions = bestModel.Transform(_testData);
        //        var metrics = _mLContext.Regression.Evaluate(predictions, labelColumnName: "Label");

        //        Console.WriteLine($"Max Experiment Time: {seconds} s");
        //        Console.WriteLine($"Best R-Squared: {metrics.RSquared}");
        //        Console.WriteLine($"Best MAE: {metrics.MeanAbsoluteError}");
        //        Console.WriteLine($"Best MSE: {metrics.MeanSquaredError}");
        //        Console.WriteLine($"Best RMSE: {metrics.RootMeanSquaredError}");
        //        Console.WriteLine("\nBestRun Hyperparameter:");
        //        foreach (var param in bestRun.TrainerName.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            Console.WriteLine(param);
        //        }
        //        var hyperParams = bestRun.Estimator.ToString();
        //        Console.WriteLine($"BestRun Estimator: {hyperParams}");

        //        _mLContext.Model.Save(bestModel, _trainData.Schema, "best_sales_forecast_model.zip");
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception.Message);
        //    }
        //}

    }
}
