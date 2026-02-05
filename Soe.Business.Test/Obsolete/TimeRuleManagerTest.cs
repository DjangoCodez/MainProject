//using SoftOne.Soe.Business.Core;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SoftOne.Soe.Data;
//using System.Collections.Generic;
//using SoftOne.Soe.Util;
//using System.Data.Entity.Core.EntityClient;
//using SoftOne.Soe.Business.Util;
//using System;

//namespace Soe.Business.Test
//{
//    /// <summary>
//    ///This is a test class for TimeRuleManagerTest and is intended
//    ///to contain all TimeRuleManagerTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class TimeRuleManagerTest
//    {
//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Manager

//        /// <summary>
//        ///A test for RemoveRedundantRules
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RemoveRedundantRules_WhenDuplicates_VerifyThatDuplicatesAreRemoved() //unit test
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            //Act
//            List<TimeRule> source = new List<TimeRule>() { };
//            source.Add(new TimeRule() { TimeRuleId = 2 });
//            source.Add(new TimeRule() { TimeRuleId = 4 });
//            source.Add(new TimeRule() { TimeRuleId = 6 });

//            List<TimeRule> colllectionComparingTo = new List<TimeRule>();
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 1 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 2 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 3 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 4 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 5 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 6 });
//            colllectionComparingTo.Add(new TimeRule() { TimeRuleId = 7 });

//            target.RemoveRedundantRules(source, colllectionComparingTo);

//            //Assert
//            Assert.IsTrue(colllectionComparingTo.Count == 4);
//            Assert.IsFalse(colllectionComparingTo.Contains(new TimeRule() { TimeRuleId = 2 }));
//            Assert.IsFalse(colllectionComparingTo.Contains(new TimeRule() { TimeRuleId = 4 }));
//            Assert.IsFalse(colllectionComparingTo.Contains(new TimeRule() { TimeRuleId = 6 }));
//        }

//        #endregion

//        #region Rule engine

//        #region Rule Evaluation

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateRule_WhenContainingNestedRule_VerifyCorrectEvaluation_IntegrationTest()
//        {
//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                TimeRule timeRule = new TimeRule()
//                {
//                    RuleStartDirection = (int)SoeTimeRuleDirection.Forward,
//                };

//                TimeRuleExpression startExpression = new TimeRuleExpression()
//                {
//                    IsStart = true,
//                };

//                TimeRuleOperand startExpressionOperand = new TimeRuleOperand();
//                TimeRuleOperand startExpressionNestedLeftSideOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn,
//                    Minutes = 10,
//                };
//                TimeRuleOperand startExpressionNestedComparisonOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorAnd,
//                };
//                TimeRuleOperand startExpressionNestedRightSideOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorNot,
//                    LeftValueId = 1,
//                };

//                //Add nested operand expression
//                startExpressionOperand.NestedRuleExpression = new TimeRuleExpression();
//                startExpressionOperand.NestedRuleExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                startExpressionOperand.NestedRuleExpression.TimeRuleOperand.Add(startExpressionNestedLeftSideOperand);
//                startExpressionOperand.NestedRuleExpression.TimeRuleOperand.Add(startExpressionNestedComparisonOperand);
//                startExpressionOperand.NestedRuleExpression.TimeRuleOperand.Add(startExpressionNestedRightSideOperand);

//                TimeRuleExpression stopExpression = new TimeRuleExpression()
//                {
//                    IsStart = false,
//                };

//                TimeRuleOperand stopExpressionOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock,
//                    Minutes = 34,
//                };

//                //Init lists
//                startExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                stopExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                timeRule.TimeRuleExpression = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleExpression>();

//                //Add operands
//                startExpression.TimeRuleOperand.Add(startExpressionOperand);
//                stopExpression.TimeRuleOperand.Add(stopExpressionOperand);

//                //Add expressions
//                timeRule.TimeRuleExpression.Add(startExpression);
//                timeRule.TimeRuleExpression.Add(stopExpression);

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 21, 0),
//                    Stop = new TimeSpan(0, 0, 32, 0),
//                };

//                List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//                List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//                TimeScheduleTemplateBlock block = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(2009, 10, 1, 0, 10, 0),
//                    StopTime = new DateTime(2009, 10, 1, 0, 20, 0),
//                };
//                scheduleTemplateBlocks.Add(block);

//                //Setup expected timechunk result
//                List<TimeChunk> expected = new List<TimeChunk>();
//                expected.Add(new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 21, 0),
//                    Stop = new TimeSpan(0, 0, 32, 0),
//                });

//                //Act
//                List<TimeChunk> actual = target.EvaluateRule(timeRule, unevaluatedTimeChunk, previousTransactions, scheduleTemplateBlocks, 0, null);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);
//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].Start, actual[i].Start);
//                    Assert.AreEqual(expected[i].Stop, actual[i].Stop);
//                }
//            }
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateRule_WhenNotRuleWithMatchingTransInMiddleOfTimeChunk_VerifyMultipeTimeChunks()
//        {
//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//                TimeRule timeRule = new TimeRule()
//                {
//                    RuleStartDirection = (int)SoeTimeRuleDirection.Forward,
//                };

//                TimeRuleExpression startExpression = new TimeRuleExpression()
//                {
//                    IsStart = true,
//                };

//                TimeRuleOperand startExpressionOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorNot,
//                    LeftValueId = 1,
//                };

//                TimeRuleExpression stopExpression = new TimeRuleExpression()
//                {
//                    IsStart = false,
//                };

//                TimeRuleOperand stopExpressionOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorNot,
//                    LeftValueId = 1,
//                };

//                //Init lists
//                startExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                stopExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                timeRule.TimeRuleExpression = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleExpression>();

//                //Add operands
//                startExpression.TimeRuleOperand.Add(startExpressionOperand);
//                stopExpression.TimeRuleOperand.Add(stopExpressionOperand);

//                //Add expressions
//                timeRule.TimeRuleExpression.Add(startExpression);
//                timeRule.TimeRuleExpression.Add(stopExpression);

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 0, 0),
//                    Stop = new TimeSpan(0, 1, 40, 0),
//                };

//                List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//                var transaction = new TimeCodeTransaction()
//                {
//                    Start = new DateTime(1900, 1, 1, 0, 40, 0, 0, DateTimeKind.Local),
//                    Stop = new DateTime(1900, 1, 1, 0, 50, 0, 0, DateTimeKind.Local),
//                };
//                transaction.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 1);
//                previousTransactions.Add(transaction);

//                List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = null;

//                //Setup expected timechunk result
//                List<TimeChunk> expected = new List<TimeChunk>();
//                expected.Add(new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 0, 0),
//                    Stop = new TimeSpan(0, 0, 39, 0),
//                });
//                expected.Add(new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 55, 0),
//                    Stop = new TimeSpan(0, 1, 40, 0),
//                });

//                //Act
//                List<TimeChunk> actual = target.EvaluateRule(timeRule, unevaluatedTimeChunk, previousTransactions, scheduleTemplateBlocks, 0, null);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);
//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].Start, actual[i].Start);
//                    Assert.AreEqual(expected[i].Stop, actual[i].Stop);
//                }
//            }
//        }

//        #endregion

//        #region Result parsing

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenAndComparisonAndFailedPart_ReturnFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.succeeded);
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.and);
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.falsified);

//            //Act
//            bool actual = target.ParseEvaluationStatus(evaluationReults);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenAndComparisonAndNoFailedPart_VerifySuccess()
//        {
//            ////Arrange
//            //ParameterObject parameterObject = null;
//            //TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            //List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.succeeded);
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.and);
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.succeeded);

//            ////Act
//            //bool actual = target.ParseEvaluationStatus(evaluationReults);

//            ////Assert
//            //Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenOrComparisonAndSucceededPart_VerifySuccess()
//        {
//            ////Arrange
//            //ParameterObject parameterObject = null;
//            //TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            //List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.succeeded);
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.or);
//            //evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.falsified);

//            ////Act
//            //bool actual = target.ParseEvaluationStatus(evaluationReults);

//            ////Assert
//            //Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenOrComparisonAndNoSucceededPart_VerifyFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.falsified);
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.or);
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.falsified);

//            //Act
//            bool actual = target.ParseEvaluationStatus(evaluationReults);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenSingleValueIsTrue_VerifySucceededResult()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.succeeded);

//            //Act
//            bool actual = target.ParseEvaluationStatus(evaluationReults);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_ParseEvaluationStatus_WhenSingleValueIsFalse_VerifyFailedResult()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            List<TimeRuleManager.RuleEvaluationResult> evaluationReults = new List<TimeRuleManager.RuleEvaluationResult>();
//            evaluationReults.Add(TimeRuleManager.RuleEvaluationResult.falsified);

//            //Act
//            bool actual = target.ParseEvaluationStatus(evaluationReults);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for MergeTimeBlocks
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void MergeTimeBlocks_WhenNotAdded_AddToResult()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            List<TimeBlock> splittedBlocks = new List<TimeBlock>();
//            splittedBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 1, 0, 0) });
//            splittedBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 2, 0, 0) });
//            splittedBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 3, 0, 0) });
//            splittedBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 4, 0, 0) });
//            splittedBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 5, 0, 0) });

//            List<TimeBlock> originalTimeBlocks = new List<TimeBlock>();
//            originalTimeBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 1, 0, 0) });
//            originalTimeBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 2, 0, 0) });
//            originalTimeBlocks.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 6, 0, 0) });

//            List<TimeBlock> expected = new List<TimeBlock>();
//            expected.AddRange(splittedBlocks);
//            expected.Add(new TimeBlock() { StartTime = new DateTime(1900, 1, 1, 6, 0, 0) });

//            //Act
//            List<TimeBlock> actual = target.MergeTimeBlocks(splittedBlocks, originalTimeBlocks);

//            //Assert
//            Assert.AreEqual(expected.Count, actual.Count);
//            for (int i = 0; i < actual.Count; i++)
//            {
//                Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//            }
//        }

//        #endregion

//        #region Operand tests

//        #region Clock

//        /// <summary>
//        ///A test for EvaluateClockOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateClockOperand_WhenTimeChunkStartsBeforeRuleValue_ReturnFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = 50,
//            };

//            TimeChunk chunkToEvaluate = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 49, 0),
//                Stop = new TimeSpan(0, 53, 0),
//            };

//            //Act
//            bool actual = target.EvaluateClockOperand(chunkToEvaluate, true, operand);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateClockOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateClockOperand_WhenTimeChunkStartsAfterRuleValue_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = 50,
//            };

//            TimeChunk chunkToEvaluate = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 53, 0),
//            };

//            //Act
//            bool actual = target.EvaluateClockOperand(chunkToEvaluate, true, operand);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateClockOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateClockOperand_WhenNegativeClockAndTimeChunkStartsAfterRuleValue_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = -50,
//            };

//            TimeChunk chunkToEvaluate = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 53, 0),
//            };

//            //Act
//            bool actual = target.EvaluateClockOperand(chunkToEvaluate, true, operand);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateClockOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateClockOperand_WhenNextDayClockAndTimeChunkStartsAfterRuleValue_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = 1500,
//            };

//            TimeChunk chunkToEvaluate = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 1550, 0),
//                Stop = new TimeSpan(0, 53, 0),
//            };

//            //Act
//            bool actual = target.EvaluateClockOperand(chunkToEvaluate, true, operand);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        #endregion

//        #region Not

//        /// <summary>
//        ///A test for EvaluateNotOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateNotOperand_WhenMatchingTransactionOverlaps_ReturnFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 60, 0),
//            };

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueId = 1,
//            };
//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            var transaction = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 0, 55, 0, 0, DateTimeKind.Local),
//                Stop = new DateTime(1900, 1, 1, 1, 55, 0, 0, DateTimeKind.Local),
//            };
//            transaction.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 1);
//            previousTransactions.Add(transaction);

//            //Act
//            bool actual = target.EvaluateNotOperand(chunkUnderEvaluation, operand, previousTransactions, null);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateNotOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateNotOperand_WhenMatchingTransactionDoesntOverlap_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 60, 0),
//            };

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueId = 1,
//            };
//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            var transaction = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 1, 55, 0, 0, DateTimeKind.Local),
//                Stop = new DateTime(1900, 1, 1, 1, 55, 0, 0, DateTimeKind.Local),
//            };
//            transaction.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 1);
//            var transaction2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 0, 40, 0, 0, DateTimeKind.Local),
//                Stop = new DateTime(1900, 1, 1, 0, 45, 0, 0, DateTimeKind.Local),
//            };
//            transaction2.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 1);
//            previousTransactions.Add(transaction);
//            previousTransactions.Add(transaction2);

//            //Act
//            bool actual = target.EvaluateNotOperand(chunkUnderEvaluation, operand, previousTransactions, null);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateNotOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateNotOperand_WhenNotMatchingTransactionOverlap_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 60, 0),
//            };

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueId = 1,
//            };
//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            var transaction = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 0, 50, 0, 0, DateTimeKind.Local),
//                Stop = new DateTime(1900, 1, 1, 0, 59, 0, 0, DateTimeKind.Local),
//            };
//            transaction.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 2);
//            previousTransactions.Add(transaction);

//            //Act
//            bool actual = target.EvaluateNotOperand(chunkUnderEvaluation, operand, previousTransactions, null);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        #endregion

//        #region Schedule Out

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleOutOperand_WhenRelativeValueBelowScheduleInUseScheduleOut()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 50, 0),
//                Stop = new TimeSpan(0, 52, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = -50,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 10, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleOutOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleOutOperand_WhenAfterScheduleOut_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(1, 10, 0),
//                Stop = new TimeSpan(1, 21, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = -1,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 10, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleOutOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleOutOperand_WhenBeforeScheduleOut_ReturnFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 39, 0),
//                Stop = new TimeSpan(0, 49, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = -10,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 10, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleOutOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        #endregion

//        #region Schedule In

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleInWhenRelativeValueExceedsScheduleOut_EvaluteAsScheduleOut()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(1, 20, 0),
//                Stop = new TimeSpan(1, 40, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = -300,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 20, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleInOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleInOperand_WhenBeforeSchedule_ReturnSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(1, 1, 0),
//                Stop = new TimeSpan(1, 2, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = 10,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 10, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleInOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateScheduleInOperand_WhenAfterSchedule_ReturnFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 44, 0),
//                Stop = new TimeSpan(1, 0, 0),
//            };
//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                Minutes = 10,
//            };
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock blockIn = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 0, 50, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 0, 0),
//            };
//            TimeScheduleTemplateBlock blockOut = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 1, 10, 0),
//            };

//            scheduleTemplateBlocks.Add(blockIn);
//            scheduleTemplateBlocks.Add(blockOut);

//            //Act
//            bool actual = target.EvaluateScheduleInOperand(chunkUnderEvaluation, true, operand, scheduleTemplateBlocks);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        #endregion

//        #region Balance

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumAllWorkedCompareOverAndTransactionSummaryExceedsScheduleSummary_ReportSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresence,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 9, 0, 0),
//            };
//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 2, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 6, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 12, 0, 0),
//            };
//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [CssProjectStructure("vstfs:///Classification/Node/0c43fade-4f90-4786-82c3-725ca76e4cb1"), Owner("Jesper"), CssIteration("vstfs:///Classification/Node/26afed74-26fa-4ec4-a57f-fa207ff6a609"), TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumAllWorkedCompareOverAndTransactionSummaryBelowScheduleSummary_ReportFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresence,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 9, 0, 0),
//            };
//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 2, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 8, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 9, 0, 0),
//            };

//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumAllWorkedCompareUnderAndTransactionSummaryExceedsScheduleSummary_ReportFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresence,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLess,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 9, 0, 0),
//            };
//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 1, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 5, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 5, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 7, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 7, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 12, 0, 0),
//            };

//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumAllWorkedCompareUnderAndTransactionSummaryBelowScheduleSummary_ReportSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresence,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLess,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 9, 0, 0),
//            };
//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 1, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 8, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction() //overlapping block
//            {
//                Start = new DateTime(1900, 1, 1, 2, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction() //overlapping block 
//            {
//                Start = new DateTime(1900, 1, 1, 3, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 5, 0, 0),
//            };

//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumRestrictedScheduleWithinScheduleCompareOverAndTransactionSummaryExceedsScheduleSummary_ReportSuccess()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            int timeCodeIdToInclude = 1;
//            int scheduleTimeCodeToInclude = 5;

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedTime,
//                LeftValueId = timeCodeIdToInclude,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTimeCode,
//                RightValueId = scheduleTimeCodeToInclude,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Backward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 7, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 10, 0, 0),
//            };

//            block1.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude };
//            block2.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude };
//            block3.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude + 1 }; //not included

//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 0, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 2, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 6, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 12, 0, 0),
//            };

//            trans1.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude);
//            trans2.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude + 1);
//            trans3.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude);

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(1, 10, 5),
//                Stop = new TimeSpan(1, 10, 10),
//            };

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, null);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceRule_Bug2256()
//        {
//            /*
//             * Standard balance rule, buggfix for item 2256
//             * Desc: When work > 120 min and not within schedule > create overtime
//             * Test successfull outcome, that the transaction is generated
//             */

//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                #region Arrange time rule

//                TimeRule timeRule = new TimeRule()
//                {
//                    RuleStartDirection = (int)SoeTimeRuleDirection.Forward,
//                };

//                #region Arrange start expression

//                TimeRuleExpression startExpression = new TimeRuleExpression()
//                {
//                    IsStart = true,
//                };

//                TimeRuleOperand startExpressionOperand = new TimeRuleOperand();
//                TimeRuleOperand startExpressionOperand1 = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock,
//                    LeftValueId = 19,
//                    Minutes = 485,
//                    ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonClockPositive,
//                    OrderNbr = 1,
//                };
//                TimeRuleOperand startExpressionOperand2 = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorAnd,
//                    OrderNbr = 2,
//                };
//                TimeRuleOperand startExpressionOperand3 = new TimeRuleOperand()
//                {
//                    NestedRuleExpression = new TimeRuleExpression(),
//                    OrderNbr = 3,
//                };

//                TimeRuleOperand startExpressionOperand3NestedOperand1 = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorBalance,
//                    OrderNbr = 1,
//                    LeftValueType = (int)SoeTimeRuleValueType.WorkedTime,
//                    LeftValueId = 19,
//                    Minutes = 120,
//                    ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                };
//                TimeRuleOperand startExpressionOperand3NestedOperand2 = new TimeRuleOperand()
//                {

//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorAnd,
//                    OrderNbr = 2,
//                };
//                TimeRuleOperand startExpressionOperand3NestedOperand3 = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut,
//                    LeftValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                    OrderNbr = 3,
//                };

//                //Add nested operand expression
//                startExpressionOperand3.NestedRuleExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                startExpressionOperand3.NestedRuleExpression.TimeRuleOperand.Add(startExpressionOperand3NestedOperand1);
//                startExpressionOperand3.NestedRuleExpression.TimeRuleOperand.Add(startExpressionOperand3NestedOperand2);
//                startExpressionOperand3.NestedRuleExpression.TimeRuleOperand.Add(startExpressionOperand3NestedOperand3);

//                #endregion

//                #region Arrange stop expression

//                TimeRuleExpression stopExpression = new TimeRuleExpression()
//                {
//                    IsStart = false,
//                };

//                TimeRuleOperand stopExpressionOperand1 = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock,
//                    Minutes = 1200,
//                    ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonClockNegative,
//                    LeftValueId = 19,
//                    LeftValueType = (int)SoeTimeRuleValueType.WorkedTime,
//                };

//                #endregion

//                //Init lists
//                startExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                stopExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                timeRule.TimeRuleExpression = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleExpression>();

//                //Add operands
//                startExpression.TimeRuleOperand.Add(startExpressionOperand1);
//                startExpression.TimeRuleOperand.Add(startExpressionOperand2);
//                startExpression.TimeRuleOperand.Add(startExpressionOperand3);
//                stopExpression.TimeRuleOperand.Add(stopExpressionOperand1);

//                //Add expressions
//                timeRule.TimeRuleExpression.Add(startExpression);
//                timeRule.TimeRuleExpression.Add(stopExpression);

//                #endregion

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(16, 0, 0),
//                    Stop = new TimeSpan(20, 0, 0),
//                };

//                List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();

//                //Add work transaction time to make balance rule apply

//                TimeCodeTransaction timeTrans = new TimeCodeTransaction()
//                {
//                    Quantity = 600,
//                    Start = new DateTime(2009, 10, 1, 8, 0, 0),
//                    Stop = new DateTime(2009, 10, 1, 20, 0, 0),
//                };
//                timeTrans.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", 19);
//                previousTransactions.Add(timeTrans);

//                List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();

//                //Scheduled between 8 - 17
//                TimeScheduleTemplateBlock block = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(2009, 10, 1, 8, 0, 0),
//                    StopTime = new DateTime(2009, 10, 1, 17, 0, 0),
//                };
//                scheduleTemplateBlocks.Add(block);

//                //Setup expected timechunk result
//                List<TimeChunk> expected = new List<TimeChunk>();
//                expected.Add(new TimeChunk()
//                {
//                    Start = new TimeSpan(19, 0, 0),
//                    Stop = new TimeSpan(20, 0, 0),
//                });

//                List<TimeBlock> presence = new List<TimeBlock>();
//                presence.Add(new TimeBlock()
//                {
//                    StartTime = new DateTime(2009, 10, 1, 8, 0, 0),
//                    StopTime = new DateTime(2009, 10, 1, 20, 0, 0),
//                });

//                /*
//                 *  Scehdule
//                 *  SSSSSSSSSSSSSSSSSSSS 
//                 *  Actual
//                 *  SSSSSSSSSSSSSSSSSSSSWWWWWWWW
//                 *  Trans expected (XXXX = WWWW > 2h)
//                 *  SSSSSSSSSSSSSSSSSSSSWWWWXXXX
//                 */

//                //Act
//                List<TimeChunk> actual = target.EvaluateRule(timeRule, unevaluatedTimeChunk, previousTransactions, scheduleTemplateBlocks, 0, presence);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);
//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].Start, actual[i].Start);
//                    Assert.AreEqual(expected[i].Stop, actual[i].Stop);
//                }
//            }
//        }














//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumRestrictedScheduleWithinScheduleCompareOverAndTransactionSummaryBelowScheduleSummary_ReportFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            int timeCodeIdToInclude = 1;
//            int scheduleTimeCodeToInclude = 5;

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedTime,
//                LeftValueId = timeCodeIdToInclude,
//                RightValueId = scheduleTimeCodeToInclude,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTime,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 2, 0, 0, 0),
//                StopTime = new DateTime(1900, 1, 2, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 2, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 2, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 2, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 2, 9, 0, 0),
//            };

//            block1.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude };
//            block2.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude };
//            block3.TimeCode = new TimeCode() { TimeCodeId = scheduleTimeCodeToInclude + 1 }; //not included

//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 2, 0, 0),
//                Stop = new DateTime(1900, 1, 2, 4, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 2, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 2, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 2, 8, 0, 0),
//                Stop = new DateTime(1900, 1, 2, 15, 0, 0),
//            };
//            trans1.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude);
//            trans2.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude + 1);
//            trans3.TimeCodeReference.EntityKey = new System.Data.EntityKey("CompEntities.TimeCode", "TimeCodeId", timeCodeIdToInclude);

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, null);

//            //Assert
//            Assert.IsFalse(actual);
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumRestrictedScheduleForTimeCodeCompareOverAndTransactionSummaryExceedsScheduleSummaryByOverlaps_ReportFailure()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            int timeCodeToInclude = 1;

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresenceAccordingToSchedule,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTimeCode,
//                RightValueId = timeCodeToInclude,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorMore,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 9, 0, 0),
//            };

//            block1.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude };
//            block2.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude + 1 }; //not included
//            block3.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude };

//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 0, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 6, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 12, 0, 0),
//            };

//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Break }; //not included
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsFalse(actual); //Can never be true, overlaps should be shrunken and the remaining transactions can never exceed schedule in this scenario
//        }

//        /// <summary>
//        ///A test for EvaluateBalanceOperand
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateBalanceOperand_WhenSumRestrictedScheduleForTimeCodeCompareBeloowAndTransactionSummaryAboveScheduleSummary_ReportSuccessSinceSumOfRestrictedToScheduleCanNeverExceedTransactions()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//            int timeCodeToInclude = 1;

//            TimeRuleOperand operand = new TimeRuleOperand()
//            {
//                LeftValueType = (int)SoeTimeRuleValueType.WorkedPresenceAccordingToSchedule,
//                RightValueType = (int)SoeTimeRuleValueType.ScheduledTimeCode,
//                RightValueId = timeCodeToInclude,
//                ComparisonOperator = (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLess,
//                Minutes = 60,
//            };
//            int ruleDirection = (int)SoeTimeRuleDirection.Forward;
//            List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//            TimeScheduleTemplateBlock block1 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeScheduleTemplateBlock block2 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                StopTime = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeScheduleTemplateBlock block3 = new TimeScheduleTemplateBlock()
//            {
//                StartTime = new DateTime(1900, 1, 1, 6, 0, 0),
//                StopTime = new DateTime(1900, 1, 2, 9, 0, 0),
//            };

//            block1.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude };
//            block2.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude + 1 }; //not included
//            block3.TimeCode = new TimeCode() { TimeCodeId = timeCodeToInclude };

//            scheduleTemplateBlocks.Add(block1);
//            scheduleTemplateBlocks.Add(block2);
//            scheduleTemplateBlocks.Add(block3);

//            List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//            TimeCodeTransaction trans1 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 2, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 4, 0, 0),
//            };
//            TimeCodeTransaction trans2 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 4, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 6, 0, 0),
//            };
//            TimeCodeTransaction trans3 = new TimeCodeTransaction()
//            {
//                Start = new DateTime(1900, 1, 1, 8, 0, 0),
//                Stop = new DateTime(1900, 1, 1, 9, 0, 0),
//            };

//            trans1.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };
//            trans2.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Break }; //not included
//            trans3.TimeCode = new TimeCode() { Type = (int)SoeTimeCodeType.Work };

//            previousTransactions.Add(trans1);
//            previousTransactions.Add(trans2);
//            previousTransactions.Add(trans3);

//            TimeChunk chunkUnderEvaluation = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 0, 0),
//            };

//            List<TimeBlock> presence = new List<TimeBlock>();

//            //Act
//            bool actual = target.EvaluateBalanceOperand(chunkUnderEvaluation, operand, ruleDirection, scheduleTemplateBlocks, previousTransactions, 0, presence);

//            //Assert
//            Assert.IsTrue(actual);
//        }

//        #endregion

//        #endregion

//        #region Performance testing

//        /// <summary>
//        ///A test for EvaluateRule
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RuleEngine_EvaluateRule_WhenEvaluatingSeveralDays_VerifyExecutionTimeDontExceedNonFunctionalRequirement()
//        {
//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                //Test with 10 rules
//                TimeRule timeRule = new TimeRule()
//                {
//                    RuleStartDirection = (int)SoeTimeRuleDirection.Forward,
//                };

//                TimeRuleExpression startExpression = new TimeRuleExpression()
//                {
//                    IsStart = true,
//                };

//                TimeRuleExpression stopExpression = new TimeRuleExpression()
//                {
//                    IsStart = false,
//                };

//                TimeRuleOperand startExpressionOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock,
//                    Minutes = 10,
//                };

//                TimeRuleOperand stopExpressionOperand = new TimeRuleOperand()
//                {
//                    OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorClock,
//                    Minutes = 10,
//                };

//                //Init lists
//                startExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                stopExpression.TimeRuleOperand = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleOperand>();
//                timeRule.TimeRuleExpression = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleExpression>();

//                //Add operands
//                startExpression.TimeRuleOperand.Add(startExpressionOperand);
//                stopExpression.TimeRuleOperand.Add(stopExpressionOperand);

//                //Add expressions
//                timeRule.TimeRuleExpression.Add(startExpression);
//                timeRule.TimeRuleExpression.Add(stopExpression);

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 0, 10, 0),
//                    Stop = new TimeSpan(0, 0, 20, 0),
//                };

//                List<TimeCodeTransaction> previousTransactions = new List<TimeCodeTransaction>();
//                List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = new List<TimeScheduleTemplateBlock>();
//                TimeScheduleTemplateBlock block = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(2009, 10, 1, 8, 0, 0),
//                    StopTime = new DateTime(2009, 10, 1, 17, 0, 0),
//                };

//                //Act
//                DateTime start = DateTime.Now;

//                for (int i = 0; i < 10; i++)
//                {
//                    target.EvaluateRule(timeRule, unevaluatedTimeChunk, previousTransactions, scheduleTemplateBlocks, 0, null);
//                }
//                DateTime stop = DateTime.Now;
//                var difference = stop - start;

//                //Assert
//                //Max alloted time is 76 milliseconds ((1 sek / (27.5/2)) with margin for other rule engine processing and saving)
//                //27.5 is the number of days legacy system can process a day with 10 rules in one second (total engine execution time)
//                //our implementation needs to be no slower than double the legacy systems time
//                Assert.IsTrue(difference.TotalMilliseconds < 96, "Timed: " + difference.TotalMilliseconds.ToString() + " milliseconds");
//            }
//        }

//        #endregion

//        #region Exception testing

//        /// <summary>
//        ///A test for EvaluateRule, that will cast exception since expression collection exists
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        [ExpectedException(typeof(NullReferenceException))] //Assert
//        public void EvaluateRule_WhenRuleContainsNoExceptions_ThrowException()
//        {
//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//                TimeRule timeRule = new TimeRule();

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 1, 0),
//                    Stop = new TimeSpan(0, 2, 0)
//                };

//                //Act
//                target.EvaluateRule(timeRule, unevaluatedTimeChunk, null, null, 0, null);
//            }
//        }

//        /// <summary>
//        ///A test for EvaluateRule, that will cast exception since no stop expression exists
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        [ExpectedException(typeof(NullReferenceException))] //Assert
//        public void EvaluateRule_WhenRuleContainsNoStopExpression_ThrowException()
//        {
//            //Arrange
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//                TimeRule timeRule = new TimeRule();
//                timeRule.TimeRuleExpression = new System.Data.Objects.DataClasses.EntityCollection<TimeRuleExpression>();
//                timeRule.TimeRuleExpression.Add(new TimeRuleExpression() { IsStart = true });

//                TimeChunk unevaluatedTimeChunk = new TimeChunk()
//                {
//                    Start = new TimeSpan(0, 1, 0),
//                    Stop = new TimeSpan(0, 2, 0)
//                };

//                //Act
//                target.EvaluateRule(timeRule, unevaluatedTimeChunk, null, null, 0, null);
//            }
//        }

//        #endregion

//        #region Break parsing

//        /// <summary>
//        ///A test for ParseBreaks
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        [Ignore] //not repeatable
//        public void ParseBreaks_WhenActualBlocksEqualsSchedule_ApplyRules()
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                TimeCode work = new TimeCode()
//                {
//                    TimeCodeId = 1,
//                    Type = (int)SoeTimeCodeType.Work
//                };
//                TimeCode brk = new TimeCode()
//                {
//                    TimeCodeId = 2,
//                    Type = (int)SoeTimeCodeType.Break,
//                };

//                List<TimeBlock> timeBlocks = new List<TimeBlock>();

//                TimeBlock t1 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                    TimeDeviationCauseStart = new TimeDeviationCause() { TimeDeviationCauseId = 2 },
//                    TimeDeviationCauseStop = new TimeDeviationCause() { TimeDeviationCauseId = 2 },
//                };
//                t1.TimeCode.Add(work);

//                TimeBlock t2 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                    TimeDeviationCauseStart = new TimeDeviationCause() { TimeDeviationCauseId = 2 },
//                    TimeDeviationCauseStop = new TimeDeviationCause() { TimeDeviationCauseId = 2 },
//                };
//                t2.TimeCode.Add(brk);

//                TimeBlock t3 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                };
//                t3.TimeCode.Add(work);

//                timeBlocks.Add(t1);
//                timeBlocks.Add(t2);
//                timeBlocks.Add(t3);

//                List<TimeScheduleTemplateBlock> schedule = new List<TimeScheduleTemplateBlock>();
//                TimeScheduleTemplateBlock s1 = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                    TimeCode = work
//                };
//                TimeScheduleTemplateBlock s2 = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                    TimeCode = brk
//                };
//                TimeScheduleTemplateBlock s3 = new TimeScheduleTemplateBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                    TimeCode = work
//                };

//                schedule.Add(s1);
//                schedule.Add(s2);
//                schedule.Add(s3);

//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock exp1 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 1, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                };
//                TimeBlock exp2 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 2, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                };
//                TimeBlock exp3 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 3, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 4, 0, 0),
//                };

//                exp1.TimeCode.Add(new TimeCode() { TimeCodeId = 1, Type = 1 });
//                exp2.TimeCode.Add(new TimeCode() { TimeCodeId = 1, Type = 1 });
//                exp3.TimeCode.Add(new TimeCode() { TimeCodeId = 1, Type = 1 });

//                expected.Add(exp1);
//                expected.Add(exp2);
//                expected.Add(exp3);


//                //Act
//                List<TimeBlock> actual = target.EvaluateBreaks(entities, timeBlocks, schedule, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);

//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//                    Assert.AreEqual(expected[i].StopTime, actual[i].StopTime);
//                    Assert.AreEqual(expected[i].TimeCode.Count, actual[i].TimeCode.Count);
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleBetweenStdAndMax
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleBetweenStdAndMaxTest()
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                List<TimeBlock> splittedBreak = new List<TimeBlock>();
//                TimeBlock breakPart = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 13, 0, 0),
//                };
//                TimeDeviationCause start = new TimeDeviationCause() { TimeDeviationCauseId = 2 };
//                TimeDeviationCause stop = new TimeDeviationCause() { TimeDeviationCauseId = 2 };

//                breakPart.TimeDeviationCauseStart = start;
//                breakPart.TimeDeviationCauseStop = stop;

//                splittedBreak.Add(breakPart);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    Value = 2, //timeCodeId
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    DefaultMinutes = 30,
//                    MaxMinutes = 120
//                };

//                int breakStart = 340;
//                int breakStop = 400;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock restBreak = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 12, 30, 0),
//                };
//                TimeBlock ruleGenerated = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 30, 0),
//                    StopTime = new DateTime(1900, 1, 1, 13, 0, 0),
//                };

//                expected.Add(restBreak);
//                expected.Add(ruleGenerated);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleBetweenStdAndMax(entities, splittedBreak, rule, scheduleBreak, breakStart, breakStop, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);

//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//                    Assert.AreEqual(expected[i].StopTime, actual[i].StopTime);
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleBetweenMinAndStd
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleBetweenMinAndStdTest()
//        {
//            /*
//             *  Schedule: Arb + BreakStd + Arb
//             *  Actual: Arb + BreakMin>Break<BreakStd + Arb
//             *  (Break rule gives X)
//             *  Expected outcome: Arb + Break length of type X + Arb
//             */
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);

//                ParameterObject param = ParameterObject.Empty();
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(param);

//                TimeCode targetTimeCode = new TimeCode()
//                {
//                    TimeCodeId = 1,
//                };
//                TimeCode work = new TimeCode()
//                {
//                    TimeCodeId = 2,
//                };

//                TimeDeviationCause standardCause = new TimeDeviationCause()
//                {
//                    TimeDeviationCauseId = 1,
//                };

//                List<TimeBlock> timeBlocks = new List<TimeBlock>();
//                TimeBlock actual1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };

//                actual1.TimeCode.Add(work);
//                TimeBlock actualBreak = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 11, 15, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                actualBreak.TimeCode.Add(work);

//                TimeBlock actual3 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                actual3.TimeCode.Add(work);

//                timeBlocks.Add(actual1);
//                timeBlocks.Add(actualBreak);
//                timeBlocks.Add(actual3);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    TimeCode = targetTimeCode,
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    MinMinutes = 30,
//                    DefaultMinutes = 60,
//                };
//                int breakStart = 600;
//                int breakStop = 660;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock expected1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 15, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected1.TimeCode.Add(work);
//                TimeBlock expected2 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected2.TimeCode.Add(work);

//                TimeBlock expected3 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected3.TimeCode.Add(work);

//                TimeBlock expected4 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1901, 1, 1, 10, 15, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                };
//                expected4.TimeCode.Add(targetTimeCode);

//                expected.Add(expected1);
//                expected.Add(expected2);
//                expected.Add(expected3);
//                expected.Add(expected4);

//                List<TimeBlock> splittedBreak = new List<TimeBlock>();
//                splittedBreak.Add(actualBreak);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleBetweenMinAndStd(entities, splittedBreak, rule, scheduleBreak, breakStart, breakStop, ref timeBlocks, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, timeBlocks.Count);
//                for (int i = 0; i < expected.Count; i++)
//                {
//                    Assert.AreEqual(expected[0].StartTime, timeBlocks[0].StartTime);
//                    Assert.AreEqual(expected[0].StopTime, timeBlocks[0].StopTime);
//                    if (expected[0].TimeDeviationCauseStart != null)
//                    {
//                        Assert.AreEqual(expected[0].TimeDeviationCauseStart.TimeDeviationCauseId, timeBlocks[0].TimeDeviationCauseStart.TimeDeviationCauseId);
//                        Assert.AreEqual(expected[0].TimeDeviationCauseStop.TimeDeviationCauseId, timeBlocks[0].TimeDeviationCauseStop.TimeDeviationCauseId);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleStandard
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleStandardTest()
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                List<TimeBlock> splittedBreak = new List<TimeBlock>();
//                TimeBlock breakPart = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 13, 0, 0),
//                };
//                TimeDeviationCause start = new TimeDeviationCause() { TimeDeviationCauseId = 2 };
//                TimeDeviationCause stop = new TimeDeviationCause() { TimeDeviationCauseId = 2 };

//                breakPart.TimeDeviationCauseStart = start;
//                breakPart.TimeDeviationCauseStop = stop;

//                splittedBreak.Add(breakPart);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    Value = 2, //timeCodeId
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    DefaultMinutes = 60,
//                };

//                int breakStart = 340;
//                int breakStop = 400;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock restBreak = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 13, 00, 0),
//                };
//                expected.Add(restBreak);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleStandard(entities, splittedBreak, rule, scheduleBreak, breakStart, breakStop, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);

//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//                    Assert.AreEqual(expected[i].StopTime, actual[i].StopTime);
//                    //Assert.AreEqual(2, actual[i].TimeCode.FirstOrDefault().TimeCodeId);
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleLessThanMin
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleLessThanMinTest()
//        {
//            /*
//             *  Schedule: Arb + BreakStd + Arb
//             *  Actual: Arb + BreakMin>Break<BreakStd + Arb
//             *  (Break rule gives X)
//             *  Expected outcome: Arb + Break length of type X + Arb
//             */
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);

//                ParameterObject param = ParameterObject.Empty();
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(param);

//                TimeCode targetTimeCode = new TimeCode()
//                {
//                    TimeCodeId = 1,
//                };
//                TimeCode work = new TimeCode()
//                {
//                    TimeCodeId = 2,
//                };

//                TimeDeviationCause standardCause = new TimeDeviationCause()
//                {
//                    TimeDeviationCauseId = 1,
//                };

//                List<TimeBlock> timeBlocks = new List<TimeBlock>();
//                TimeBlock actual1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };

//                actual1.TimeCode.Add(work);
//                TimeBlock actualBreak = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                actualBreak.TimeCode.Add(work);

//                TimeBlock actual3 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                actual3.TimeCode.Add(work);

//                timeBlocks.Add(actual1);
//                timeBlocks.Add(actualBreak);
//                timeBlocks.Add(actual3);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    TimeCode = targetTimeCode,
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    MinMinutes = 40,
//                };
//                int breakStart = 600;
//                int breakStop = 660;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock expected1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 20, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected1.TimeCode.Add(work);
//                TimeBlock expected2 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected2.TimeCode.Add(work);

//                TimeBlock expected3 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 11, 0, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                expected3.TimeCode.Add(work);

//                TimeBlock expected4 = new TimeBlock()
//                {
//                    StartTime = new DateTime(1901, 1, 1, 10, 20, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                };
//                expected4.TimeCode.Add(targetTimeCode);

//                expected.Add(expected1);
//                expected.Add(expected2);
//                expected.Add(expected3);
//                expected.Add(expected4);

//                List<TimeBlock> splittedBreak = new List<TimeBlock>();
//                splittedBreak.Add(actualBreak);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleLessThanMin(entities, splittedBreak, rule, scheduleBreak, breakStart, breakStop, ref timeBlocks, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, timeBlocks.Count);
//                for (int i = 0; i < expected.Count; i++)
//                {
//                    Assert.AreEqual(expected[0].StartTime, timeBlocks[0].StartTime);
//                    Assert.AreEqual(expected[0].StopTime, timeBlocks[0].StopTime);
//                    if (expected[0].TimeDeviationCauseStart != null)
//                    {
//                        Assert.AreEqual(expected[0].TimeDeviationCauseStart.TimeDeviationCauseId, timeBlocks[0].TimeDeviationCauseStart.TimeDeviationCauseId);
//                        Assert.AreEqual(expected[0].TimeDeviationCauseStop.TimeDeviationCauseId, timeBlocks[0].TimeDeviationCauseStop.TimeDeviationCauseId);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleMoreThanMax
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleMoreThanMaxTest()
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);
//                ParameterObject parameterObject = null;
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);

//                List<TimeBlock> splittedBreak = new List<TimeBlock>();
//                TimeBlock breakPart = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 14, 0, 0),
//                };
//                TimeDeviationCause start = new TimeDeviationCause() { TimeDeviationCauseId = 2 };
//                TimeDeviationCause stop = new TimeDeviationCause() { TimeDeviationCauseId = 2 };

//                breakPart.TimeDeviationCauseStart = start;
//                breakPart.TimeDeviationCauseStop = stop;

//                splittedBreak.Add(breakPart);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    Value = 2, //timeCodeId
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    DefaultMinutes = 30,
//                    MaxMinutes = 60
//                };

//                int breakStart = 340;
//                int breakStop = 400;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock restBreak = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 12, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 13, 0, 0),
//                };
//                TimeBlock ruleGenerated = new TimeBlock()
//                {
//                    StartTime = new DateTime(1900, 1, 1, 13, 0, 0),
//                    StopTime = new DateTime(1900, 1, 1, 14, 0, 0),
//                };

//                expected.Add(restBreak);
//                expected.Add(ruleGenerated);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleMoreThanMax(entities, splittedBreak, rule, scheduleBreak, breakStart, breakStop, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);

//                for (int i = 0; i < actual.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//                    Assert.AreEqual(expected[i].StopTime, actual[i].StopTime);
//                }
//            }
//        }

//        /// <summary>
//        ///A test for ApplyBreakRuleLessThanMinOnRemovedBreak
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void ApplyBreakRuleLessThanMinOnRemovedBreakTest()
//        {
//            /*
//             *  Schedule: Arb + Break + Arb
//             *  Actual: Arb + Arb
//             *  (Break rule gives X where X is the length of Min break)
//             *  Expected outcome: Arb + Default Break length of type X + Arb
//             */
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);

//                ParameterObject param = ParameterObject.Empty();
//                TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(param);

//                TimeCode targetTimeCode = new TimeCode()
//                {
//                    TimeCodeId = 1,
//                };
//                TimeCode work = new TimeCode()
//                {
//                    TimeCodeId = 2,
//                };

//                TimeDeviationCause standardCause = new TimeDeviationCause()
//                {
//                    TimeDeviationCauseId = 1,
//                };
//                targetTimeCode.TimeDeviationCause.Add(standardCause);
//                work.TimeDeviationCause.Add(standardCause);

//                List<TimeBlock> timeBlocks = new List<TimeBlock>();
//                TimeBlock actual1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };

//                actual1.TimeCode.Add(work);
//                TimeBlock actual2 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 00, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };
//                actual2.TimeCode.Add(work);
//                timeBlocks.Add(actual1);
//                timeBlocks.Add(actual2);

//                TimeCodeRule rule = new TimeCodeRule()
//                {
//                    TimeCode = targetTimeCode,
//                };
//                TimeCodeBreak scheduleBreak = new TimeCodeBreak()
//                {
//                    MinMinutes = 20,
//                };
//                int breakStart = 600;
//                int breakStop = 660;
//                int actorCompanyId = 2;

//                List<TimeBlock> expected = new List<TimeBlock>();
//                TimeBlock expected1 = new TimeBlock()
//                {
//                    TimeBlockId = 1,
//                    StartTime = new DateTime(1901, 1, 1, 8, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };

//                TimeBlock expected2 = new TimeBlock()
//                {
//                    TimeBlockId = 2,
//                    StartTime = new DateTime(1901, 1, 1, 10, 50, 0),
//                    StopTime = new DateTime(1901, 1, 1, 17, 0, 0),
//                    TimeDeviationCauseStart = standardCause,
//                    TimeDeviationCauseStop = standardCause,
//                };

//                TimeBlock expected3 = new TimeBlock()
//                {
//                    TimeBlockId = 0,
//                    StartTime = new DateTime(1901, 1, 1, 10, 30, 0),
//                    StopTime = new DateTime(1901, 1, 1, 10, 50, 0),
//                    //TimeDeviationCauseStart = standardCause,
//                    //TimeDeviationCauseStop = standardCause,
//                };

//                expected1.TimeCode.Add(work);
//                expected2.TimeCode.Add(work);
//                expected3.TimeCode.Add(targetTimeCode);

//                expected.Add(expected1);
//                expected.Add(expected2);
//                expected.Add(expected3);

//                //Act
//                List<TimeBlock> actual = target.ApplyBreakRuleLessThanMinOnRemovedBreak(entities, timeBlocks, rule, scheduleBreak, breakStart, breakStop, actorCompanyId);

//                //Assert
//                Assert.AreEqual(expected.Count, actual.Count);
//                for (int i = 0; i < expected.Count; i++)
//                {
//                    Assert.AreEqual(expected[i].StartTime, actual[i].StartTime);
//                    Assert.AreEqual(expected[i].StopTime, actual[i].StopTime);
//                    Assert.AreEqual(expected[i].TimeBlockId, actual[i].TimeBlockId);
//                    if (expected[i].TimeDeviationCauseStart != null)
//                    {
//                        Assert.AreEqual(expected[i].TimeDeviationCauseStart.TimeDeviationCauseId, actual[i].TimeDeviationCauseStart.TimeDeviationCauseId);
//                        Assert.AreEqual(expected[i].TimeDeviationCauseStop.TimeDeviationCauseId, actual[i].TimeDeviationCauseStop.TimeDeviationCauseId);
//                    }
//                }
//            }
//        }

//        #endregion

//        #region Rounding

//        /// <summary>
//        ///A test for RoundEvaluatedTimeChunk
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RoundEvaluatedTimeChunk_WhenPositiveRounding_RoundToValue()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk timeChunk = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 10, 0),
//            };
//            int value = 6;
//            TermGroup_TimeCodeRoundingType roundingType = TermGroup_TimeCodeRoundingType.RoundUp;
//            TimeChunk expected = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 12, 0),
//            };

//            //Act
//            TimeChunk actual = target.RoundEvaluatedTimeChunk(timeChunk, value, roundingType);

//            //Assert
//            Assert.AreEqual(expected.Start, actual.Start);
//            Assert.AreEqual(expected.Stop, actual.Stop);
//        }

//        /// <summary>
//        ///A test for RoundEvaluatedTimeChunk
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RoundEvaluatedTimeChunk_WhenNegativeRounding_RoundAwayFromValue()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk timeChunk = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 21, 0),
//            };
//            int value = 6;
//            TermGroup_TimeCodeRoundingType roundingType = TermGroup_TimeCodeRoundingType.RoundDown;
//            TimeChunk expected = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 18, 0),
//            };

//            //Act
//            TimeChunk actual = target.RoundEvaluatedTimeChunk(timeChunk, value, roundingType);

//            //Assert
//            Assert.AreEqual(expected.Start, actual.Start);
//            Assert.AreEqual(expected.Stop, actual.Stop);
//        }

//        /// <summary>
//        ///A test for RoundEvaluatedTimeChunk
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RoundEvaluatedTimeChunk_WhenNegativeRoundingAndEven_NoRounding()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk timeChunk = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 18, 0),
//            };
//            int value = 6;
//            TermGroup_TimeCodeRoundingType roundingType = TermGroup_TimeCodeRoundingType.RoundDown;
//            TimeChunk expected = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 18, 0),
//            };

//            //Act
//            TimeChunk actual = target.RoundEvaluatedTimeChunk(timeChunk, value, roundingType);

//            //Assert
//            Assert.AreEqual(expected.Start, actual.Start);
//            Assert.AreEqual(expected.Stop, actual.Stop);
//        }

//        /// <summary>
//        ///A test for RoundEvaluatedTimeChunk
//        ///</summary>
//        [TestMethod()]
//        [DeploymentItem("SoftOne.Soe.Business.dll")]
//        public void RoundEvaluatedTimeChunk_WhenPositiveRoundingAndEven_NoRounding()
//        {
//            //Arrange
//            ParameterObject parameterObject = null;
//            TimeRuleManager_Accessor target = new TimeRuleManager_Accessor(parameterObject);
//            TimeChunk timeChunk = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 24, 0),
//            };
//            int value = 6;
//            TermGroup_TimeCodeRoundingType roundingType = TermGroup_TimeCodeRoundingType.RoundUp;
//            TimeChunk expected = new TimeChunk()
//            {
//                Start = new TimeSpan(0, 0, 0),
//                Stop = new TimeSpan(0, 24, 0),
//            };

//            //Act
//            TimeChunk actual = target.RoundEvaluatedTimeChunk(timeChunk, value, roundingType);

//            //Assert
//            Assert.AreEqual(expected.Start, actual.Start);
//            Assert.AreEqual(expected.Stop, actual.Stop);
//        }

//        #endregion

//        #endregion


//    }
//}
