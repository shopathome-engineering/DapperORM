using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Dapper;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// Uses Dapper to provide data access through stored procedures
    /// </summary>
    public class DapperDBProvider : IDBProvider
    {
        public IDbConnection Connect(string connectionStringName)
        {
            return new SqlConnection(FullConnectionString(connectionStringName));
        }

        public string FullConnectionString(string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionString == null) throw new InvalidOperationException($"No connection string with name {connectionStringName} found in the configuration file!");
            return connectionString.ToString();
        }

        #region Read
        public IEnumerable<TData> Query<TData>(IDbConnection connection, string sql, object param = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return ExecuteSQLQuery<TData>(connection, sql, param);
        }

        public IEnumerable<TData> Query<TData>(IDbConnection connection, string sql, int timeoutSeconds, object param = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return ExecuteSQLQuery<TData>(connection, sql, param, timeoutSeconds);
        }

        public TData Get<TData>(IDbConnection connection, int id)
                where TData : class
        {
            return Get<TData, int>(connection, id);
        }

        public TData Get<TData>(IDbConnection connection, long id)
                where TData : class
        {
            return Get<TData, long>(connection, id);
        }

        public TData Get<TData>(IDbConnection connection, Guid id)
                where TData : class
        {
            return Get<TData, Guid>(connection, id);
        }

        private TData Get<TData, TKey>(IDbConnection connection, TKey id)
                where TData : class
                where TKey : struct
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection.Get<TData>(id);
        }

        public IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return ExecuteStoredProcedure<TData>(connection, storedProcedureName);
        }

        public IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName, object args)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return ExecuteStoredProcedure<TData>(connection, storedProcedureName, args);
        }

        public IEnumerable<TData> Select<TData>(IDbConnection connection, string storedProcedureName, object args, int timeoutSeconds)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return ExecuteStoredProcedure<TData>(connection, storedProcedureName, args, timeoutSeconds);
        }

        public IEnumerable<TData> Select<TData>(IDbConnection connection, Expression<Func<TData, bool>> predicate)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            var name = SqlMapperExtensions.GetTableName(typeof(TData));

            var query = SQLExpressionVisitor.GetQuery(predicate);
            // this no-locks by default... Something to be aware of if ever we need to read only committed data
            var sql = "select * from " + SqlMapperExtensions.EncaseWithSquareBrackets(name) + " with (nolock) where " + query.GetParameterizedSQLString();
            return ExecuteSQLQuery<TData>(connection, sql, query.GetParameterizedSQLArgs());
        }

        #endregion

        #region Insert / Update / Delete

        public void Execute(IDbConnection connection, string sql)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            connection.Execute(sql);
        }

        public void Insert<TData>(IDbConnection connection, TData item)
                where TData : class
        {
            connection.Insert(item);
        }

        public void Insert<TData>(IDbConnection connection, List<TData> list)
                where TData : class
        {
            connection.Insert(list);
        }

        public bool Update<TData>(IDbConnection connection, TData item)
                where TData : class
        {
            return connection.Update(item);
        }

        public bool Update<TData>(IDbConnection connection, List<TData> list)
                where TData : class
        {
            return connection.Update(list);
        }

        public bool Delete<TData>(IDbConnection connection, TData target)
                where TData : class
        {
            return connection.Delete(target);
        }

        #endregion

        #region Stored Procs

        public int Execute(IDbConnection connection, string storedProcedureName, object args)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection.Execute(storedProcedureName, args, commandType: CommandType.StoredProcedure);
        }

        public void ExecuteReader(IDbConnection connection, string storedProcedureName, object args, Action<IDataReader> readerAction)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var reader = connection.ExecuteReader(storedProcedureName, args, commandType: CommandType.StoredProcedure))
            {
                readerAction(reader);
            }
        }

        public TData ExecuteScalar<TData>(IDbConnection connection, string command, object args, bool isInlineSql = false)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return connection.ExecuteScalar<TData>(command, args, commandType: isInlineSql ? CommandType.Text : CommandType.StoredProcedure);
        }

        #endregion


        #region Helpers

        private IEnumerable<TData> ExecuteStoredProcedure<TData>(IDbConnection connection, string storedProcedureName, object param, int? timeoutSeconds = null)
        {
            return connection.Query<TData>(storedProcedureName, param, commandType: CommandType.StoredProcedure, commandTimeout: timeoutSeconds);
        }

        private IEnumerable<TData> ExecuteStoredProcedure<TData>(IDbConnection connection, string storedProcedureName)
        {
            return ExecuteStoredProcedure<TData>(connection, storedProcedureName, null);
        }

        private IEnumerable<TData> ExecuteSQLQuery<TData>(IDbConnection connection, string sql)
        {
            return ExecuteSQLQuery<TData>(connection, sql, null);
        }

        private IEnumerable<TData> ExecuteSQLQuery<TData>(IDbConnection connection, string sql, object param, int? timeoutSeconds = null)
        {
            try
            {
                return connection.Query<TData>(sql, param, commandType: CommandType.Text, commandTimeout: timeoutSeconds);
            }
            catch (SqlException sqlError)
            {
                sqlError.Data.Add("SQL", $"The SQL that was attempted to be executed was: {sql}");
                throw;
            }
        }

        #endregion

    }
}
