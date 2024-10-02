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
        /// Gets or sets the starting date filter for data to be considered in the analysis.
        /// Only data after this date will be processed.
        /// </summary>
        DateTime? FromDateFilter { get; set; }

        /// <summary>
        /// Gets or sets the end date filter for data to be considered in the analysis.
        /// Only data up to and including this date will be processed.
        /// </summary>
        DateTime? UntilDateFilter { get; set; }

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
        Task<(List<string>, List<decimal>)> CreatePlotDataSource(string groupByColumn, int top = 0, bool cutID = false);
    }
}
