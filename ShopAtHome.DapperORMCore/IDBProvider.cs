using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// Lightweight data-provider interface
    /// TODO: Remove Connect() method and make this IDisposable?
    /// </summary>
    public interface IDBProvider
    {
        /// <summary>
        /// Returns a connection to the database specified by the connection string
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        /// <remarks>Will throw if the specified connection string does not exist in the app.config</remarks>
        IDbConnection Connect(string connectionStringName);

            /// <summary>
        /// Returns the results of the specified stored procedure called without arguments
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="storedProcedureName"></param>
        /// <returns></returns>
        IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName);

        /// <summary>
        /// Returns the results of the specified stored procedure called with the specified arguments
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName, object args);

        /// <summary>
        /// Returns the results of the specified stored procedure called with the specified arguments
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="args"></param>
        /// <param name="timeoutSeconds">The number of seconds allowed to elapse before the connection throws a timeout error</param>
        /// <returns></returns>
        IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName, object args, int timeoutSeconds);

        /// <summary>
        /// Selects from the table to which TData belongs with the provided predicate
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IEnumerable<TData> Select<TData>(IDbConnection connection, Expression<Func<TData, bool>> predicate);        

        /// <summary>
        /// Executes a stored procedure using the connection and arguments provided.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="args"></param>
        int Execute(IDbConnection connection, string storedProcedureName, object args);

        /// <summary>
        /// Executes a stored procedure and passes the resulting data reader to the action method.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="args"></param>
        /// <param name="readerAction"></param>
        void ExecuteReader(IDbConnection connection, string storedProcedureName, object args, Action<IDataReader> readerAction);

        /// <summary>
        /// Executes a scalar stored procedure and returns the typed result.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="command">Either a stored procedure name or a SQL literal - use the isInlineSql argument to change the behavior</param>
        /// <param name="args"></param>
        /// <param name="isInlineSql">If FALSE, treats the Command parameter as a stored procedure identifier. If TRUE, treads the Command parameter as a SQL literal</param>
        /// <returns></returns>
        /// <remarks>TODO: Make this better, having an optional bool param to change behavior sucks</remarks>
        TData ExecuteScalar<TData>(IDbConnection connection, string command, object args, bool isInlineSql = false);

        /// <summary>
        /// Executes the sql and returns the data mapped to the TData object.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="param">Any paramters to pass to the query</param>
        /// <returns></returns>
        IEnumerable<TData> Query<TData>(IDbConnection connection, string sql, object param = null);

        /// <summary>
        /// Executes the sql and returns the data mapped to the TData object.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="timeoutSeconds">The number of seconds allowed to elapse before the connection throws a timeout error</param>
        /// <param name="param">Any paramters to pass to the query</param>
        /// <returns></returns>
        IEnumerable<TData> Query<TData>(IDbConnection connection, string sql, int timeoutSeconds, object param = null);

        TData Get<TData>(IDbConnection connection, int id) where TData : class;
        TData Get<TData>(IDbConnection connection, long id) where TData : class;
        TData Get<TData>(IDbConnection connection, Guid id) where TData : class;

        void Insert<TData>(IDbConnection connection, TData item) where TData : class;
        void Insert<TData>(IDbConnection connection, List<TData> list) where TData : class;
        bool Update<TData>(IDbConnection connection, TData item) where TData : class;
        bool Update<TData>(IDbConnection connection, List<TData> list) where TData : class;
        bool Delete<TData>(IDbConnection connection, TData target) where TData : class;
        void Execute(IDbConnection connection, string sql);

        /// <summary>
        /// The actual connection string for a given shortcut name.
        /// </summary>
        /// <param name="connectionStringName">Shortcut name of the connection string.</param>
        /// <returns></returns>
        string FullConnectionString(string connectionStringName);
    }
}