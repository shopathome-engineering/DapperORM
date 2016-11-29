using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopAtHome.DapperORMCore.Tests
{
    [TestClass]
    public class SQLExpressionVisitorTests
    {
        private string St = "Foo";

        public enum TestEnum
        {
            Foo = 1,
            Bar = 2
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo";
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SimpleInequalityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo != "Foo";
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] <> 'Foo'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ChainedInequalityPredicate_GivenStringLiteralAndBoolLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo != "Foo" && t.Zowie;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] <> 'Foo' AND [Zowie] = 1", StringComparison.OrdinalIgnoreCase));

            expression = t => t.Foo != "Foo" && !t.Zowie;
            result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] <> 'Foo' AND [Zowie] <> 1", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void GetsLocalScopeVariableValueCorrectly()
        {
            var s = "Foo";
            Expression<Func<TestClass, bool>> expression = t => t.Foo == s;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void GetsClassScopeVariableValueCorrectly()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == this.St;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenIntegerLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zing == 42;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Zing] = 42", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenBoolLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zowie == true;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Zowie] = 1", StringComparison.OrdinalIgnoreCase));
            expression = t => t.Zowie == false;
            result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Zowie] = 0", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenImplicitBool_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zowie;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Zowie] = 1", StringComparison.OrdinalIgnoreCase));

            expression = t => !t.Zowie;
            result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Zowie] <> 1", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ChainedANDEqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" && t.FooTwo == "Fizzle";
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo' AND [FooTwo] = 'Fizzle'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ChainedOREqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" || t.FooTwo == "Fizzle";
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo' OR [FooTwo] = 'Fizzle'", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ChainedANDEqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL_ForThreeExpressions()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" && t.FooTwo == "Fizzle" && t.Zing == 1;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals("[Bar] = 'Foo' AND [FooTwo] = 'Fizzle' AND [Zing] = 1", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void DateEqualityPredicate_GeneratesCorrectSQL()
        {
            DateTime now = DateTime.Now;
            Expression<Func<TestClass, bool>> expression = t => t.WhenItHappened <= now;
            var result = SQLExpressionVisitor.GetQuery(expression);
            Assert.IsTrue(result.Equals($"[WhenItHappened] <= '{now}'"));
        }

        [TestMethod]
        public void QueryingOnEnumValue_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.MyEnumColumn == TestEnum.Bar;
            var result = SQLExpressionVisitor.GetQuery(expression.Body);
            Assert.IsTrue(result.Equals("[MyEnumColumn] = 2"));
        }

        [TestMethod]
        public void QueryingOnEnumValue_WithAdditionalLogical_GeneratesCorrectSQL()
        {
            var temp = TestEnum.Foo;
            var result = SqlMapperExtensions.TransformToSQL<TestClass>(t => t.Id == 24 && t.MyEnumColumn == temp);
            Assert.IsTrue(result.Equals("[Id] = 24 AND [MyEnumColumn] = 1"));
        }

        public static string GetDebugView(Expression exp)
        {
            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }
    }

    public class TestClass
    {
        [Key]
        public int Id { get; set; }

        [Column("Bar")]
        public string Foo { get; set; }

        public string FooTwo { get; set; }

        public int Zing { get; set; }

        public bool Zowie { get; set; }

        public DateTime WhenItHappened { get; set; }

        public SQLExpressionVisitorTests.TestEnum MyEnumColumn { get; set; }
    }
}
