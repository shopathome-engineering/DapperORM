using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using FluentAssertions;

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
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar");
            dynamic args = result.GetParameterizedSQLArgs();
            // runtime binding fails on the dynamic property with FluentAssertions so we're stuck with the built-in asserts
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
        }

        [TestMethod]
        public void SimpleInequalityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo != "Foo";
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] <> @Bar");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
        }

        [TestMethod]
        public void ChainedInequalityPredicate_GivenStringLiteralAndBoolLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo != "Foo" && t.Zowie;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] <> @Bar AND [Zowie] = @Zowie");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
            Assert.AreEqual(args.Zowie, true);


            expression = t => t.Foo != "Foo" && !t.Zowie;
            result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] <> @Bar AND [Zowie] = @Zowie");
            args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
            Assert.AreEqual(args.Zowie, false);
        }

        [TestMethod]
        public void GetsLocalScopeVariableValueCorrectly()
        {
            var s = "Foo";
            Expression<Func<TestClass, bool>> expression = t => t.Foo == s;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
        }

        [TestMethod]
        public void GetsClassScopeVariableValueCorrectly()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == this.St;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenIntegerLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zing == 42;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Zing] = @Zing");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Zing, 42);
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenBoolLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zowie == true;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Zowie] = @Zowie");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Zowie, true);

            expression = t => t.Zowie == false;
            result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Zowie] = @Zowie");
            args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Zowie, false);
        }

        [TestMethod]
        public void SimpleEqualityPredicate_GivenImplicitBool_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Zowie;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Zowie] = @Zowie");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Zowie, true);

            expression = t => !t.Zowie;
            result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Zowie] = @Zowie");
            args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Zowie, false);
        }

        [TestMethod]
        public void ChainedANDEqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" && t.FooTwo == "Fizzle";
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar AND [FooTwo] = @FooTwo");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
            Assert.IsTrue(string.Equals(args.FooTwo, "Fizzle"));
        }

        [TestMethod]
        public void ChainedOREqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" || t.FooTwo == "Fizzle";
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar OR [FooTwo] = @FooTwo");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
            Assert.IsTrue(string.Equals(args.FooTwo, "Fizzle"));
        }

        [TestMethod]
        public void ChainedANDEqualityPredicate_GivenStringLiteral_GeneratesCorrectSQL_ForThreeExpressions()
        {
            Expression<Func<TestClass, bool>> expression = t => t.Foo == "Foo" && t.FooTwo == "Fizzle" && t.Zing == 1;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[Bar] = @Bar AND [FooTwo] = @FooTwo AND [Zing] = @Zing");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.IsTrue(string.Equals(args.Bar, "Foo"));
            Assert.IsTrue(string.Equals(args.FooTwo, "Fizzle"));
            Assert.AreEqual(args.Zing, 1);
        }

        [TestMethod]
        public void DateEqualityPredicate_GeneratesCorrectSQL()
        {
            DateTime now = DateTime.Now;
            Expression<Func<TestClass, bool>> expression = t => t.WhenItHappened <= now;
            var result = SQLExpressionVisitor.GetQuery(expression);
            result.GetParameterizedSQLString().Should().Be("[WhenItHappened] <= @WhenItHappened");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.WhenItHappened, now);
        }

        [TestMethod]
        public void QueryingOnEnumValue_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> expression = t => t.MyEnumColumn == TestEnum.Bar;
            var result = SQLExpressionVisitor.GetQuery(expression.Body);
            result.GetParameterizedSQLString().Should().Be("[MyEnumColumn] = @MyEnumColumn");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.MyEnumColumn, (int)TestEnum.Bar);
        }

        [TestMethod]
        public void QueryingOnEnumValue_WithAdditionalLogical_GeneratesCorrectSQL()
        {
            var temp = TestEnum.Foo;
            Expression<Func<TestClass, bool>> exp = t => t.Id == 24 && t.MyEnumColumn == temp;
            var result = SQLExpressionVisitor.GetQuery(exp);

            result.GetParameterizedSQLString().Should().Be("[Id] = @Id AND [MyEnumColumn] = @MyEnumColumn");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Id, 24);
            Assert.AreEqual(args.MyEnumColumn, temp);
        }

        [TestMethod]
        public void ChainedExpressionWithMixOfAndsAndOrs_GeneratesCorrectSQL()
        {
            Expression<Func<TestClass, bool>> exp = t => t.Id == 24 && t.MyEnumColumn == TestEnum.Foo || t.Foo != "Foo" && t.FooTwo == "Fizzle" || t.Zing > 42;
            var result = SQLExpressionVisitor.GetQuery(exp);

            result.GetParameterizedSQLString().Should().Be("[Id] = @Id AND [MyEnumColumn] = @MyEnumColumn OR [Bar] <> @Bar AND [FooTwo] = @FooTwo OR [Zing] > @Zing");
            dynamic args = result.GetParameterizedSQLArgs();
            Assert.AreEqual(args.Id, 24);
            Assert.AreEqual(args.MyEnumColumn, 1);
            Assert.AreEqual(args.Bar, "Foo");
            Assert.AreEqual(args.FooTwo, "Fizzle");
            Assert.AreEqual(args.Zing, 42);
        }

        [TestMethod]
        public void BreakBinaryExpression_GivenComplexQueryChain_GeneratesExpectedValues()
        {
            Expression<Func<TestClass, bool>> exp = t => t.Id == 24 && t.MyEnumColumn == TestEnum.Foo || t.Foo != "Foo" && t.FooTwo == "Fizzle" || t.Zing > 42;
            var results = SQLExpressionVisitor.BreakBinaryExpression(exp);
            var aResults = results.ToArray();
            aResults[0].Expression.NodeType.Should().Be(ExpressionType.Equal);
            aResults[0].FollowingOperator.Should().Be("AND");
            aResults[1].Expression.NodeType.Should().Be(ExpressionType.Equal);
            aResults[1].FollowingOperator.Should().Be("OR");
            aResults[2].Expression.NodeType.Should().Be(ExpressionType.NotEqual);
            aResults[2].FollowingOperator.Should().Be("AND");
            aResults[3].Expression.NodeType.Should().Be(ExpressionType.Equal);
            aResults[3].FollowingOperator.Should().Be("OR");
            aResults[4].Expression.NodeType.Should().Be(ExpressionType.GreaterThan);
            aResults[4].FollowingOperator.Should().BeNull();
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
