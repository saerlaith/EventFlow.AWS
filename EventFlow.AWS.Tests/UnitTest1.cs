using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using EventFlow.DynamoDB.Configuration;
using EventFlow.DynamoDB.SnapshotStore;
using EventFlow.Logs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventFlow.AWS.Tests
{
    [TestClass]
    public class TestSnapshotStore
    {
        private static IAmazonDynamoDB dynamoDb { get; set; }
        private static ILog log;

        private static IAWSConfiguration configuration;

        private static DynamoDBSnapshotStore snapshotStore;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            try
            {
                log = new ConsoleLog();
                dynamoDb = InfrastructureHelper.CreateLocalDynamoDbClient();
                configuration = AWSConfiguration.GetDefaultConfiguration;
                snapshotStore = new DynamoDBSnapshotStore(log, dynamoDb, configuration);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            try {
            var tables = await dynamoDb.ListTablesAsync();

            Assert.AreEqual(tables.TableNames.Count, 0);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
