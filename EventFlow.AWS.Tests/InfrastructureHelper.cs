using Amazon;
using Amazon.DynamoDBv2;

namespace EventFlow.AWS.Tests
{
    public static class InfrastructureHelper
    {
        public static IAmazonDynamoDB CreateLocalDynamoDbClient()
        {
            AmazonDynamoDBConfig amazonDynamoDBConfig = new AmazonDynamoDBConfig();
            amazonDynamoDBConfig.RegionEndpoint = RegionEndpoint.EUWest1;
            amazonDynamoDBConfig.ServiceURL = "http://localhost:8000";
            return new AmazonDynamoDBClient(amazonDynamoDBConfig);
        }
    }
}