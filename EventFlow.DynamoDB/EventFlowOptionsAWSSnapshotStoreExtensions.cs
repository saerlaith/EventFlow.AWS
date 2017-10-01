using EventFlow.DynamoDB.SnapshotStore;
using EventFlow.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB
{
    public static class EventFlowOptionsAWSSnapshotStoreExtensions
    {
        public static IEventFlowOptions UseAWSSnapshotStore(this IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions.UseSnapshotStore<DynamoDBSnapshotStore>();
        }
    }
}
