using Amazon.DynamoDBv2.DataModel;
using EventFlow.EventStores;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB.EventStores
{
    [DynamoDBTable("Events")]
    public class DynamoDbEvent : ICommittedDomainEvent
    {
        [DynamoDBHashKey]
        public string AggregateId { get; set; }

        [DynamoDBRangeKey]
        public int AggregateSequenceNumber { get; set; }

        public string AggregateName { get; set; }
        
        public long GlobalSequenceNumber { get; set; }

        public string BatchId { get; set; }

        public string Data { get; set; }

        public string Metadata { get; set; }
    }
}
