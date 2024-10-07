// IDataStoreService.cs

using System.Data;

namespace BalancedScorecard.Services
{
    /// <summary>
    /// Defines the interface for a service that manages the datastore, including
    /// data loading, updating, and encryption features.
    /// </summary>
    public interface IDataStoreService : IHostedService
    {
        /// <summary>
        /// Gets the collection of DataTables representing the datastore's current state.
        /// </summary>
        DataTableCollection DataTables { get; }

        /// <summary>
        /// Updates the datastore with the latest data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation of updating the datastore.</returns>
        Task UpdateDataStore();

        /// <summary>
        /// Creates a data source for a two dimensional plot.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation of updating the datastore.</returns>
        Task<(List<string>, List<decimal>)> CreatePlotDataSource(string groupByColumn, DateTime fromDateFilter, DateTime untilDateFilter, int top = 0, bool cutID = false);
    }
}
