using System.Collections.Generic;
using System.Data.Common;

namespace DataRepositories
{
    public interface IDataRepository
    {
        /// <summary>
        /// Creates a new record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="T">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to create the new record.</param>
        /// <param name="record">The domain object instance containing the new record.</param>
        /// <returns>The ID of the newly created record.</returns>
        int New<T>(string cmd, T record) where T : new();

        /// <summary>
        /// Retrieves a collection of records according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="T">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to retrieve the records.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>A collection of domain object instances representing the records returned from the command.</returns>
        IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null) where T : new();

        /// <summary>
        /// Edits a record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="T">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to edit the record.</param>
        /// <param name="record">The domain object instance containing the edited record.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>The ID of the edited record.</returns>
        int Edit<T>(string cmd, T record, (string, object)[] @params = null) where T : new();

        /// <summary>
        /// Removes a record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="T">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to remove the record.</param>
        /// <param name="record">The domain object instance containing the record to remove.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>The ID of the removed record.</returns>
        int Remove<T>(string cmd, T record, (string, object)[] @params = null) where T : new();
    }
}