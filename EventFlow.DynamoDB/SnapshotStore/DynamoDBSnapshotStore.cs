using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using EventFlow.Logs;
using EventFlow.Snapshots.Stores;
using System;
using System.Collections.Generic;
using System.Text;
using EventFlow.Core;
using EventFlow.Snapshots;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using EventFlow.Extensions;
using EventFlow.DynamoDB.Configuration;

namespace EventFlow.DynamoDB.SnapshotStore
{
    public class DynamoDBSnapshotStore : ISnapshotPersistence
    {
        private readonly ILog _log;
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly DynamoDBContext _dynamoDBContext;
        private readonly IAWSConfiguration _configuration;

        public DynamoDBSnapshotStore(
            ILog log,
            IAmazonDynamoDB dynamoDB,
            IAWSConfiguration configuration)
        {
            _log = log;
            _dynamoDBClient = dynamoDB;
            _dynamoDBContext = new DynamoDBContext(dynamoDB, configuration.ContextConfig ?? new DynamoDBContextConfig());
            _configuration = configuration;
        }

        public Task DeleteSnapshotAsync(Type aggregateType, IIdentity identity, CancellationToken cancellationToken)
        {
            return _dynamoDBContext.DeleteAsync<DynamoDBSnapshot>(identity.Value, aggregateType.GetAggregateName().Value, cancellationToken);
        }

        public async Task<CommittedSnapshot> GetSnapshotAsync(Type aggregateType, IIdentity identity, CancellationToken cancellationToken)
        {
            var snapshot = await _dynamoDBContext.LoadAsync<DynamoDBSnapshot>(identity.Value, aggregateType.GetAggregateName().Value, cancellationToken);

            return new CommittedSnapshot(snapshot.Metadata, snapshot.Data);
        }

        public Task PurgeSnapshotsAsync(Type aggregateType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task PurgeSnapshotsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetSnapshotAsync(Type aggregateType, IIdentity identity, SerializedSnapshot serializedSnapshot, CancellationToken cancellationToken)
        {
            var snapshot = new DynamoDBSnapshot()
            {
                AggregateId = identity.Value,
                AggregateName = aggregateType.GetAggregateName().Value,
                AggregateSequenceNumber = serializedSnapshot.Metadata.AggregateSequenceNumber,
                Metadata = serializedSnapshot.SerializedMetadata,
                Data = serializedSnapshot.SerializedData
            };

            return _dynamoDBContext.SaveAsync(snapshot, cancellationToken);
        }
    }
}
