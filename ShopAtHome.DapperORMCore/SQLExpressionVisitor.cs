using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// TODO: Contains/LIKE, IS IN, PARENTHESES
    /// </summary>
    public static class SQLExpressionVisitor
    {
        public static SQLQuery GetQuery(Expression predicate)
        {
            if (predicate.NodeType == ExpressionType.Lambda)
            {
                return GetQuery(((LambdaExpression)predicate).Body);
            }
            var predicates = BreakBinaryExpression(predicate);
            return GetQuery(predicates);
        }

        internal static List<SQLPredicateComponent> BreakBinaryExpression(Expression expression)
        {
            var results = new List<SQLPredicateComponent>();
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return BreakBinaryExpression(((LambdaExpression)expression).Body);
            }
            if (!IsBinary(expression))
            {
                results.Add(new SQLPredicateComponent { Expression = expression});
                return results;
            }
            var binary = (BinaryExpression) expression;
            var left = binary.Left;
            var right = binary.Right;
            var op = binary.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
            if (IsBinary(left) && IsBinary(right))
            {
                results.AddRange(BreakBinaryExpression(left));
                results.Last().FollowingOperator = op;
                results.AddRange(BreakBinaryExpression(right));
            }
            else if (IsBinary(left))
            {
                results.AddRange(BreakBinaryExpression(left));
                results.Last().FollowingOperator = op;
                results.Add(new SQLPredicateComponent { Expression = right });
            }
            else if (IsBinary(right))
            {
                results.Add(new SQLPredicateComponent {Expression = left, FollowingOperator = op });
                results.AddRange(BreakBinaryExpression(right));
            }
            else
            {
                results.Add(new SQLPredicateComponent { Expression = left, FollowingOperator = op });
                results.Add(new SQLPredicateComponent { Expression = right });
            }
            return results;
        }

        private static bool IsBinary(Expression expression)
        {
            return expression.NodeType == ExpressionType.AndAlso || expression.NodeType == ExpressionType.OrElse;
        }

        private static SQLQuery GetQuery(IEnumerable<SQLPredicateComponent> predicates)
        {
            var result = new SQLQuery();
            foreach (var predicate in predicates)
            {
                result.AddPredicate(SQLQueryPredicate.Build(predicate.Expression), predicate.FollowingOperator);
            }
            return result;
        }
    }
}
