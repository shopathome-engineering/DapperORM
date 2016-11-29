using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// TODO: Contains/LIKE, IS IN, PARENTHESES
    /// </summary>
    public class SQLQueryPredicate
    {
        public static SQLQueryPredicate Build(Expression expression)
        {
            var result = new SQLQueryPredicate {SourceExpression = expression};
            string parameterName;
            switch (expression.NodeType)
            {
                case ExpressionType.GreaterThanOrEqual:
                    result.SQLOperator = ">=";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.LessThanOrEqual:
                    result.SQLOperator = "<=";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.LessThan:
                    result.SQLOperator = "<";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.GreaterThan:
                    result.SQLOperator = ">";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.Equal:
                    result.SQLOperator = "=";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.Not:
                    return Build(Expression.Equal(((UnaryExpression)expression).Operand, Expression.Constant(false)));
                case ExpressionType.NotEqual:
                    result.SQLOperator = "<>";
                    VisitBinary(expression as BinaryExpression, result);
                    break;
                case ExpressionType.Lambda:
                    return Build((expression as LambdaExpression).Body);
                case ExpressionType.Convert:
                    return Build((expression as UnaryExpression).Operand);
                case ExpressionType.Constant:
                    parameterName = VisitMember(expression as MemberExpression).ToString();
                    result.ParameterSQLName = SqlMapperExtensions.EncaseWithSquareBrackets(parameterName);
                    result.ParameterSQLValue = VisitConstant(expression as ConstantExpression);
                    result.ParameterArgumentSQLName = parameterName;
                    break;
                case ExpressionType.MemberAccess:
                    parameterName = VisitMember(expression as MemberExpression).ToString();
                    result.ParameterSQLName = SqlMapperExtensions.EncaseWithSquareBrackets(parameterName);
                    PropertyInfo boolMemberAccess;
                    if (TryCheckForMemberAccessBoolExpression(expression, out boolMemberAccess))
                    {
                        result.SQLOperator = "=";
                        result.ParameterSQLValue = true;
                    }
                    else
                    {
                        // ???
                        throw new NotImplementedException();
                    }
                    result.ParameterArgumentSQLName = parameterName;
                    break;
                default:
                    throw new NotSupportedException($"The query expression type {expression.NodeType} is not supported");
            }
            return result;
        }
        
        internal Expression SourceExpression { get; set; }
        private string ParameterSQLName { get; set; }
        internal string ParameterArgumentSQLName { get; set; }
        private string SQLOperator { get; set; }
        internal object ParameterSQLValue { get; set; }
        internal string PredicateSQL => $"{ParameterSQLName} {SQLOperator} @{ParameterArgumentSQLName}";

        private static void VisitBinary(BinaryExpression node, SQLQueryPredicate predicate)
        {
            var parameterName = node.Left.NodeType == ExpressionType.Convert ? VisitMember(((UnaryExpression) node.Left).Operand as MemberExpression).ToString() 
                                                                             : VisitMember(node.Left as MemberExpression).ToString();
            predicate.ParameterSQLName = SqlMapperExtensions.EncaseWithSquareBrackets(parameterName);
            switch (node.Right.NodeType)
            {
                case ExpressionType.Convert:
                    predicate.ParameterSQLValue = VisitMember((node.Right as UnaryExpression).Operand as MemberExpression);
                    break;
                case ExpressionType.Constant:
                    predicate.ParameterSQLValue = VisitConstant(node.Right as ConstantExpression);
                    break;
                case ExpressionType.MemberAccess:
                    predicate.ParameterSQLValue = VisitMember(node.Right as MemberExpression);
                    break;
                default:
                    throw new NotSupportedException($"The query expression type {node.Right.NodeType} is not supported");
            }
            predicate.ParameterArgumentSQLName = parameterName;
        }

        private static object VisitMember(MemberExpression node)
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
            var property = (PropertyInfo)node.Member;

            return GetName(property);
        }

        private static string GetName(PropertyInfo info)
        {
            return info.GetCustomAttribute<ColumnAttribute>()?.Name ?? info.Name;
        }

        private static object VisitConstant(ConstantExpression node)
        {
            return node.Value ?? "NULL";
        }

        private static bool TryCheckForMemberAccessBoolExpression(Expression predicate, out PropertyInfo boolMemberAccess)
        {
            var prop = (PropertyInfo)((MemberExpression)predicate).Member;
            var result = prop.PropertyType == typeof(bool);
            boolMemberAccess = result ? prop : null;
            return result;
        }
    }
}
