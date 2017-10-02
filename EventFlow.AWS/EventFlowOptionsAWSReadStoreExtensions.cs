using EventFlow.DynamoDB.ReadStore;
using EventFlow.Extensions;
using EventFlow.ReadStores;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB
{
    public static class EventFlowOptionsAWSReadStoreExtensions
    {
        public static IEventFlowOptions UseAWSReadModel<TReadModel, TReadModelLocator>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IReadModel, new()
            where TReadModelLocator : IReadModelLocator
        {
            return eventFlowOptions
                .UseReadStoreFor<DynamoDBReadStore<TReadModel>, TReadModel, TReadModelLocator>();
        }

        public static IEventFlowOptions UseAWSReadModel<TReadModel>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IReadModel, new()
        {
            return eventFlowOptions
                .UseReadStoreFor<DynamoDBReadStore<TReadModel>, TReadModel>();
        }
    }
}
