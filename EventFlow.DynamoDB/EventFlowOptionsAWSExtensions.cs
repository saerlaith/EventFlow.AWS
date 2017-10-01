using Amazon.DynamoDBv2;
using Amazon.S3;
using EventFlow.Configuration;
using EventFlow.DynamoDB.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB
{
    public static class EventFlowOptionsAWSExtensions
    {
        public static IEventFlowOptions ConfigureAWSPersistence(
            this IEventFlowOptions eventFlowOptions,
            IAmazonDynamoDB dynamoDB,
            IAmazonS3 s3,
            IAWSConfiguration configuration)
        {
            return eventFlowOptions
                .RegisterServices(f =>
                {
                    f.Register(_ => configuration, Lifetime.Singleton);
                    f.Register(_ => s3, Lifetime.Singleton);
                    f.Register(_ => dynamoDB, Lifetime.Singleton);
                });
        }
    }
}
