using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// TODO: NULL, Contains/LIKE, IS IN, AND ALSO
    /// </summary>
    public static class SQLExpressionVisitor
    {
        public static string GetQuery(Expression predicate)
        {
            PropertyInfo boolMemberAccess;
            if (predicate.NodeType == ExpressionType.MemberAccess && TryCheckForMemberAccessBoolExpression(predicate, out boolMemberAccess))
            {
                return VisitMember((MemberExpression) predicate) + " = 1";
            }
            switch (predicate.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetQuery(((LambdaExpression) predicate).Body);
                case ExpressionType.AndAlso:
                    return string.Join(" AND ", new[] { ((BinaryExpression)predicate).Left, ((BinaryExpression)predicate).Right }.Select(GetQuery));
                case ExpressionType.OrElse:
                    return string.Join(" OR ", GetMultipleExpressions(predicate));
                default:
                    return VisitExpression(predicate);
            }
        }

        private static bool TryCheckForMemberAccessBoolExpression(Expression predicate, out PropertyInfo boolMemberAccess)
        {
            var prop = (PropertyInfo) ((MemberExpression) predicate).Member;
            var result = prop.PropertyType == typeof (bool);
            boolMemberAccess = result ? prop : null;
            return result;
        }

        private static IEnumerable<string> GetMultipleExpressions(Expression expression)
        {
            var binaryExpression = (BinaryExpression)expression;
            yield return VisitExpression(binaryExpression.Left);
            yield return VisitExpression(binaryExpression.Right);
        }

        private static string VisitExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return VisitExpression(Expression.NotEqual(((UnaryExpression)expression).Operand, Expression.Constant(true)));
                case ExpressionType.GreaterThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, ">=");
                case ExpressionType.LessThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, "<=");
                case ExpressionType.LessThan:
                    return VisitBinary(expression as BinaryExpression, "<");
                case ExpressionType.GreaterThan:
                    return VisitBinary(expression as BinaryExpression, ">");
                case ExpressionType.Equal:
                    return VisitBinary(expression as BinaryExpression, "=");
                case ExpressionType.NotEqual:
                    return VisitBinary(expression as BinaryExpression, "<>");
                case ExpressionType.Lambda:
                    return VisitLambda(expression as LambdaExpression);
                case ExpressionType.Convert:
                    return VisitExpression((expression as UnaryExpression).Operand);
                case ExpressionType.Constant:
                    return VisitConstant(expression as ConstantExpression);
                case ExpressionType.MemberAccess:
                    return VisitMember(expression as MemberExpression);
                default:
                    throw new NotSupportedException($"The query expression type {expression.NodeType} is not supported");
            }
        }

        private static string VisitBinary(BinaryExpression node, string operatorString)
        {
            return VisitExpression(node.Left) + $" {operatorString} " + VisitExpression(node.Right);
        }

        private static string VisitMember(MemberExpression node)
        {
            var expression = node.Expression as ConstantExpression;
            if (expression != null)
            {
                var container = expression.Value;
                var member = node.Member;
                var info = member as FieldInfo;
                if (info != null)
                {
                    var value = info.GetValue(container);
                    return VisitConstant(Expression.Constant(value));
                }
                var propertyInfo = member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(container, null);
                    return VisitConstant(Expression.Constant(value));
                }
                return VisitConstant(expression);
            }
            var property = (PropertyInfo) node.Member;

            return GetName(property);
        }
        
        private static string GetName(this PropertyInfo info)
        {
            var result = info.GetCustomAttribute<ColumnAttribute>()?.Name ?? info.Name;
            return SqlMapperExtensions.EncaseWithSquareBrackets(result);
        }

        private static string VisitConstant(ConstantExpression node)
        {
            var nodeType = node.Value?.GetType();
            if (nodeType == typeof(string) || nodeType == typeof(DateTime))
            {
                return $"'{node.Value}'";
            }
            if (nodeType == typeof(bool))
            {
                return (bool)node.Value ? "1" : "0";
            }
            if (nodeType.IsEnum)
            {
                return ((int) node.Value).ToString();
            }
            return node.Value?.ToString() ?? "NULL";
        }

        private static string VisitLambda(LambdaExpression node)
        {
            return VisitExpression(node.Body);
        }
    }
}
