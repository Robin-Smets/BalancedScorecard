// IDataStoreService.cs

using System.Data;

namespace BalancedScorecard.Services
{
    /// <summary>
    /// Defines the interface for a service that manages the datastore, including
    /// data loading, updating, and encryption features.
    /// </summary>
    public interface IDataStoreService
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
        /// Executes the analysis and loads the data into the properties for the visualizing components.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation of analyzing data and loading data into the visualizing components' properties.</returns>
        Task LoadData();

        /// <summary>
        /// Encrypts the entire locally persisted datastore using the provided key to secure its contents.
        /// </summary>
        /// <param name="key">The encryption key used to encrypt the data.</param>
        /// <returns>A task that represents the asynchronous operation of encrypting the datastore.</returns>
        Task EncryptDataStore(string key);

        /// <summary>
        /// Decrypts the entire locally persisted datastore using the provided key to access its contents.
        /// </summary>
        /// <param name="key">The decryption key used to decrypt the data.</param>
        /// <returns>A task that represents the asynchronous operation of decrypting the datastore.</returns>
        Task DecryptDataStore(string key);

        /// <summary>
        /// Ensures that the specified directory exists and is marked as hidden, creating it if necessary.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to be checked or created.</param>
        void EnsureDirectoryExistsAndHidden(string directoryPath);
    }
}
