using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace ShopAtHome.DapperORMCore
{
    public class SQLQuery
    {
        internal List<SQLQueryPredicate> Predicates { get; }
        private Dictionary<SQLQueryPredicate, string> ArgumentJoiningMap { get; }

        public SQLQuery()
        {
            Predicates = new List<SQLQueryPredicate>();
            ArgumentJoiningMap = new Dictionary<SQLQueryPredicate, string>();
        }

        internal void AddPredicate(SQLQueryPredicate predicate, string andOr)
        {
            Predicates.Add(predicate);
            if (!string.IsNullOrEmpty(andOr))
            {
                ArgumentJoiningMap[predicate] = andOr;
            }
        }

        internal string GetParameterizedSQLString()
        {
            var sb = new StringBuilder();
            foreach (var predicate in Predicates)
            {
                sb.Append(predicate.PredicateSQL);
                if (ArgumentJoiningMap.ContainsKey(predicate))
                {
                    sb.Append($" {ArgumentJoiningMap[predicate]} ");
                }
            }
            return sb.ToString();
        }

        internal object GetParameterizedSQLArgs()
        {
            var result = new ExpandoObject();
            var builder = (IDictionary<string, object>) result;
            foreach (var predicate in Predicates)
            {
                builder.Add(predicate.ParameterArgumentSQLName, predicate.ParameterSQLValue);
            }
            return result;
        }
    }
}
