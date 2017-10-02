using EventFlow.DynamoDB.EventStores;
using EventFlow.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB
{
    public static class EventFlowOptionsAWSEventStoreExtensions
    {
        public static IEventFlowOptions UseAWSEventStore(this IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions.UseEventStore<DynamoDBEventStore>();
        }
    }
}
