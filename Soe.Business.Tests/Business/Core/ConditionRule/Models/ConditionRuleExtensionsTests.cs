using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.ConditionRule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.ConditionRule.Models.Tests
{
    [TestClass()]
    public class ConditionRuleExtensionsTests
    {

        [TestMethod]
        public void CreateConditionRuleContainer_Should_Return_Same_Expression()
        {
            // Arrange
            string expression = "(Contains(1,2)&&!Contains(3,4))";

            // Act
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            string concatenatedExpression = container.ConcatenateExpression();

            // Assert
            Assert.AreEqual(expression, concatenatedExpression);
        }

        [TestMethod]
        public void CreateConditionRuleContainer_Should_Return_Same_Expression_WithOrOperator()
        {
            // Arrange
            string expression = "(Contains(1,2)||!Contains(3,4))";

            // Act
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            string concatenatedExpression = container.ConcatenateExpression();

            // Assert
            Assert.AreEqual(expression, concatenatedExpression);
        }

        [TestMethod]
        public void CreateConditionRuleContainer_Should_Return_Same_Expression_WithAndOrOperators()
        {
            // Arrange
            string expression = "(Contains(1,2)&&!Contains(3,4)||Contains(5,6))";

            // Act
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            string concatenatedExpression = container.ConcatenateExpression();

            // Assert
            Assert.AreEqual(expression, concatenatedExpression);
        }

        [TestMethod]
        public void CreateConditionRuleContainer_Should_Return_Same_Expression_WithNestedExpressions()
        {
            // Arrange
            string expression = "((Contains(1,2)&&!Contains(3,4))||(Contains(5,6)&&Contains(7,8)))";

            // Act
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            string concatenatedExpression = container.ConcatenateExpression();

            // Assert
            Assert.AreEqual(expression, concatenatedExpression);
        }

        [TestMethod]
        public void CreateConditionRuleContainer_Should_Return_Same_Expression_WithMultipleOperators()
        {
            // Arrange
            string expression = "(Contains(1,2)&&!Contains(3,4)||Contains(5,6)&&Contains(7,8)||!Contains(9,10))";

            // Act
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            string concatenatedExpression = container.ConcatenateExpression();

            // Assert
            Assert.AreEqual(expression, concatenatedExpression);
        }

        [TestMethod]
        public void EvaluateCondition_ShouldReturnTrue_WhenValueMatchesCondition()
        {
            // Arrange
            var expression = "Contains(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value1";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EvaluateCondition_ShouldReturnFalse_WhenValueIsNotinCondition()
        {
            // Arrange
            var expression = "Contains(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value5";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void EvaluateCondition_ShouldReturnTrue_WhenValueDoesNotMatchCondition()
        {
            // Arrange
            var expression = "!Contains(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value5";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EvaluateCondition_ShouldReturnTrue_WhenValueMatchesAllValuesInCondition()
        {
            // Arrange
            var expression = "ContainsAll(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value1,value2";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EvaluateCondition_ShouldReturnTrue_WhenValueDoesNotMatchAllValuesInCondition()
        {
            // Arrange
            var expression = "!ContainsAll(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value1,value2, value3";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsFalse(result);
        }

        // now test where all values are foudn and no more
        [TestMethod]
        public void EvaluateCondition_ShouldReturnTrue_WhenValueMatchesAllValuesInConditionAndNoMore()
        {
            // Arrange
            var expression = "ContainsAll(value1,value2)";
            var container = ConditionRuleExtensions.CreateConditionRuleContainer(expression);
            var value = "value1,value2";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void EvaluateCondition_ShouldReturnFalse_WhenValueDoesNotMatchCondition()
        {
            // Arrange
            var container = new ConditionRuleContainer
            {
                Rows = new List<ConditionRuleRow>
                {
                    new ConditionRuleRow
                    {
                        RowType = ConditionRuleRowType.Contains,
                        Values = new List<ConditionRuleRowValue>
                        {
                            new ConditionRuleRowValue { Value = "value1" },
                            new ConditionRuleRowValue { Value = "value2" }
                        }
                    },
                    new ConditionRuleRow
                    {
                        RowType = ConditionRuleRowType.Operator,
                        Expression = "&&"
                    },
                    new ConditionRuleRow
                    {
                        RowType = ConditionRuleRowType.ContainsAll,
                        Values = new List<ConditionRuleRowValue>
                        {
                            new ConditionRuleRowValue { Value = "value3" },
                            new ConditionRuleRowValue { Value = "value4" }
                        }
                    }
                }
            };
            var value = "value5";

            // Act
            var result = container.EvaluateCondition(value);

            // Assert
            Assert.IsFalse(result);
        }

    }
}