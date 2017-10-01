using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB.Configuration
{
    public interface IAWSConfiguration
    {
        /// <summary>
        /// Configures how many events are stored in a single S3 object
        /// </summary>
        int EventBucketSize { get; set; }

        /// <summary>
        /// Configures the name of the bucket the events are stored in
        /// </summary>
        string EventBucketName { get; set; }

        /// <summary>
        /// Configures the Dynamo client for most calls to the service
        /// </summary>
        DynamoDBContextConfig ContextConfig { get; set; }

    }
}
