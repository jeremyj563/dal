using DataRepositories.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataRepositories.Interfaces
{
    public interface IDataRepository
    {
        /// <summary>
        /// Creates a new record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="TSchema">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to create the new record.</param>
        /// <param name="record">The domain object instance containing the new record.</param>
        /// <returns>The ID of the newly created record.</returns>
        Task<int> NewAsync<TSchema>(string cmd, TSchema record) where TSchema : new();

        /// <summary>
        /// Creates new records according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="TSchema">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to create the new records.</param>
        /// <param name="records">The collection of domain object instances containing the new records.</param>
        Task NewAsync<TSchema>(IEnumerable<TSchema> records, string tableName) where TSchema : new();

        /// <summary>
        /// Retrieves a collection of records according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="TSchema">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to retrieve the records.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>A collection of domain object instances representing the records returned from the command.</returns>
        Task<IQueryable<TSchema>> GetAsync<TSchema>(string cmd, (string, object)[] @params = null) where TSchema : new();

        /// <summary>
        /// Retrieves a collection of dynamically typed records according to the schema defined by the specified dynamic type instance.
        /// </summary>
        /// <param name="schema">An instance of the dynamically typed record that defines the record schema.</param>
        /// <param name="cmd">The command to retrieve the records.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>A collection of dynamic instances representing the records returned from the command.</returns>
        Task<IQueryable<Dynamic>> GetDynamicAsync(Dynamic schema, string cmd, (string, object)[] @params = null);

        /// <summary>
        /// Edits a record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="TSchema">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to edit the record.</param>
        /// <param name="record">The domain object instance containing the edited record.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>The ID of the edited record.</returns>
        Task<int> EditAsync<TSchema>(string cmd, TSchema record, (string, object)[] @params = null) where TSchema : new();

        /// <summary>
        /// Removes a record according to the schema defined by the specified type.
        /// </summary>
        /// <typeparam name="TSchema">The type that defines the record schema.</typeparam>
        /// <param name="cmd">The command to remove the record.</param>
        /// <param name="record">The domain object instance containing the record to remove.</param>
        /// <param name="params">The optional parameters to be added to the command.</param>
        /// <returns>The ID of the removed record.</returns>
        Task<int> RemoveAsync<TSchema>(string cmd, TSchema record, (string, object)[] @params = null) where TSchema : new();

        /// <summary>
        /// Tests the connection availability by attempting to open the connection.
        /// </summary>
        /// <returns>True if the connection attempt succeeds or false if it fails.</returns>
        Task<bool> IsConnectionAvailableAsync();
    }
}
