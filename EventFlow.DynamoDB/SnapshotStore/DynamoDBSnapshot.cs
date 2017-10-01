using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB.SnapshotStore
{
    [DynamoDBTable("Snapshots")]
    public class DynamoDBSnapshot
    {
        [DynamoDBHashKey]
        public string AggregateId {get;set;}

        [DynamoDBRangeKey]
        public string AggregateName { get; set; }
        
        [DynamoDBLocalSecondaryIndexRangeKey]
        public int AggregateSequenceNumber { get; set; }

        public string Data { get; set; }

        public string Metadata { get; set; }
    }
}
