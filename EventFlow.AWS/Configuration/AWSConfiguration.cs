using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace EventFlow.DynamoDB.Configuration
{
    public class AWSConfiguration : IAWSConfiguration
    {
        public int EventBucketSize { get; set; }
        public string EventBucketName { get; set; }
        public DynamoDBContextConfig ContextConfig { get; set; }

        public static AWSConfiguration GetDefaultConfiguration => new AWSConfiguration()
        {
            EventBucketSize = 1000,
            EventBucketName = "eventflow-events",
            ContextConfig = new DynamoDBContextConfig()
            {
                ConsistentRead = true
            }
        };
    }
}
