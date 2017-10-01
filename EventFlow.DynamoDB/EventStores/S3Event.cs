using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.DynamoDB.EventStores
{
    public class S3Event
    {
        public int GlobalSequenceNumber { get; set; }

        public string AggregateId { get; set; }
        
        public int SequenceNumber { get; set; }
    }
}
