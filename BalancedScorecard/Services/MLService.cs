

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
            var options = new LightGbmRegressionTrainer.Options
            {
                NumberOfLeaves = 64,            // Erhöhung der Blätteranzahl
                MinimumExampleCountPerLeaf = 10, // Minimale Anzahl an Beispielen pro Blatt
                LearningRate = 0.05,             // Niedrigere Lernrate
                NumberOfIterations = 500,        // Erhöhte Anzahl an Iterationen
                LabelColumnName = "OrderVolume"
            };
            // Pipeline definieren
            _pipeLine = _mLContext.Transforms.Conversion.ConvertType("ProductID", "ProductID", DataKind.Single)
                .Append(_mLContext.Transforms.Conversion.ConvertType("SalesPersonID", "SalesPersonID", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("TerritoryID", "TerritoryID", DataKind.Single))
                .Append(_mLContext.Transforms.Conversion.ConvertType("CustomerID", "CustomerID", DataKind.Single))
                .Append(_mLContext.Transforms.Concatenate("Features", new[]
                {
                    "ProductID", "SalesPersonID", "TerritoryID", "CustomerID"
                }))
                .Append(_mLContext.Regression.Trainers.LightGbm(options));
            _dataStoreService = dataStoreService;
        }

        public async Task LoadData()
        {
            try
            {
                var dataTable = _dataStoreService.DataTables["OrderVolume"];
                var orderVolumeList = dataTable.AsEnumerable().Select(row => new OrderVolumeModel
                {
                    OrderDateCalenderWeek = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateCalenderWeek"))
                        ? 0
                        : Int32.Parse(row.Field<string>("OrderDateCalenderWeek")),

                    OrderDateMonth = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateMonth"))
                        ? 0
                        : Int32.Parse(row.Field<string>("OrderDateMonth")),

                    OrderDateQuarter = string.IsNullOrWhiteSpace(row.Field<string>("OrderDateQuarter"))
                        ? 0
                        : Int32.Parse(row.Field<string>("OrderDateQuarter")),

                    TimeUnitCalenderWeek = row.Field<string>("TimeUnitCalenderWeek"),
                    TimeUnitMonth = row.Field<string>("TimeUnitMonth"),
                    TimeUnitQuarter = row.Field<string>("TimeUnitQuarter"),

                    // Wenn das Datum leer ist, dann verwende null, ansonsten parsen
                    OrderDate = string.IsNullOrWhiteSpace(row.Field<string>("OrderDate"))
                        ? DateTime.MinValue
                        : DateTime.Parse(row.Field<string>("OrderDate")),

                    SalesOrderID = string.IsNullOrWhiteSpace(row.Field<string>("SalesOrderID"))
                        ? 0
                        : Int32.Parse(row.Field<string>("SalesOrderID")),

                    CustomerID = string.IsNullOrWhiteSpace(row.Field<string>("CustomerID"))
                        ? 0
                        : Int32.Parse(row.Field<string>("CustomerID")),

                    CustomerName = row.Field<string>("CustomerName"),

                    TerritoryID = string.IsNullOrWhiteSpace(row.Field<string>("TerritoryID"))
                        ? 0
                        : Int32.Parse(row.Field<string>("TerritoryID")),

                    TerritoryName = row.Field<string>("TerritoryName"),

                    SalesPersonID = string.IsNullOrWhiteSpace(row.Field<string>("SalesPersonID"))
                        ? 0
                        : Int32.Parse(row.Field<string>("SalesPersonID")),

                    SalesPersonName = row.Field<string>("SalesPersonName"),

                    ProductID = string.IsNullOrWhiteSpace(row.Field<string>("ProductID"))
                        ? 0
                        : Int32.Parse(row.Field<string>("ProductID")),

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
                //var split = _mLContext.Data.TrainTestSplit(_data, testFraction: 0.2);
                //_trainData = split.TrainSet;
                //_testData = split.TestSet;
                Console.WriteLine("ML Service: data loaded");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public async Task TrainModel()
        {
            try
            {
                int numFolds = 5;

                // K-Fold Cross-Validation durchführen
                var crossValResults = _mLContext.Regression.CrossValidate(_data, _pipeLine, numberOfFolds: numFolds, labelColumnName: "OrderVolume");

                // Metriken für jeden Fold ausgeben
                foreach (var result in crossValResults)
                {
                    Console.WriteLine($"Fold: {result.Fold} - R²: {result.Metrics.RSquared:0.##}, MAE: {result.Metrics.MeanAbsoluteError:0.##}, RMSE: {result.Metrics.RootMeanSquaredError:0.##}");
                }

                // Durchschnittliche Metriken berechnen
                var averageRSquared = crossValResults.Average(r => r.Metrics.RSquared);
                var averageMAE = crossValResults.Average(r => r.Metrics.MeanAbsoluteError);
                var averageRMSE = crossValResults.Average(r => r.Metrics.RootMeanSquaredError);

                Console.WriteLine($"Average R²: {averageRSquared:0.##}");
                Console.WriteLine($"Average MAE: {averageMAE:0.##}");
                Console.WriteLine($"Average RMSE: {averageRMSE:0.##}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }
    }
}
