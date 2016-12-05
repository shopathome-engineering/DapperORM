using System.Linq.Expressions;

namespace ShopAtHome.DapperORMCore
{
    internal class SQLPredicateComponent
    {
        public string FollowingOperator { get; set; }
        public Expression Expression { get; set; }
    }
}
