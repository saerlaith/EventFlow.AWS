using EventFlow.EventStores;
using System;
using System.Collections.Generic;
using System.Text;
using EventFlow.Core;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Logs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using System.Linq;
using EventFlow.Aggregates;
using System.IO;
using Newtonsoft.Json;
using EventFlow.Configuration;
using EventFlow.DynamoDB.Configuration;

namespace EventFlow.DynamoDB.EventStores
{
    public class DynamoDBEventStore : IEventPersistence
    {
        private readonly ILog _log;
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly DynamoDBContext _dynamoDBContext;
        private readonly IAmazonS3 _amazons3;
        private readonly IAWSConfiguration _configuration;

        private string EventBucketName => _configuration.EventBucketName;
        private int EventBucketSize => _configuration.EventBucketSize;

        public DynamoDBEventStore(
            ILog log,
            IAmazonS3 amazonS3,
            IAmazonDynamoDB dynamoDB,
            IAWSConfiguration configuration)
        {
            _log = log;
            _dynamoDBClient = dynamoDB;
            _amazons3 = amazonS3;
            _configuration = configuration;
            _dynamoDBContext = new DynamoDBContext(dynamoDB, configuration.ContextConfig ?? new DynamoDBContextConfig());
        }

        public async Task<IReadOnlyCollection<ICommittedDomainEvent>> CommitEventsAsync(IIdentity id, IReadOnlyCollection<SerializedEvent> serializedEvents, CancellationToken cancellationToken)
        {
            var events = await GetCurrentEventObject(cancellationToken);
            var currentGlobalSequenceNumber = events?.Max(e => e.GlobalSequenceNumber) ?? 0;
            var committedEvents = new List<ICommittedDomainEvent>();

            foreach (var evt in serializedEvents)
            {
                var dynamoDbEvent = new DynamoDbEvent
                {
                    AggregateId = id.Value,
                    AggregateName = evt.Metadata[MetadataKeys.AggregateName],
                    BatchId = evt.Metadata[MetadataKeys.BatchId],
                    Data = evt.SerializedData,
                    Metadata = evt.SerializedMetadata,
                    AggregateSequenceNumber = evt.AggregateSequenceNumber,
                    GlobalSequenceNumber = currentGlobalSequenceNumber
                };

                await _dynamoDBContext.SaveAsync(dynamoDbEvent, cancellationToken).ConfigureAwait(false);

                var s3event = new S3Event()
                {
                    AggregateId = id.Value,
                    SequenceNumber = evt.AggregateSequenceNumber,
                    GlobalSequenceNumber = currentGlobalSequenceNumber
                };

                if(currentGlobalSequenceNumber % EventBucketSize == 0)
                {
                    events.Clear();
                }

                events.Add(s3event);

                await _amazons3.PutObjectAsync(new PutObjectRequest()
                {
                    ContentBody = JsonConvert.SerializeObject(events),
                    Key = GetS3Key(currentGlobalSequenceNumber).ToString(),
                    BucketName = EventBucketName
                }, cancellationToken).ConfigureAwait(false);

                committedEvents.Add(dynamoDbEvent);
                currentGlobalSequenceNumber++;
            }

            return committedEvents;
        }

        public Task DeleteEventsAsync(IIdentity id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<AllCommittedEventsPage> LoadAllCommittedEvents(GlobalPosition globalPosition, int pageSize, CancellationToken cancellationToken)
        {
            int gPos = int.Parse(globalPosition.Value);
            var key = globalPosition.IsStart ? "0" : GetS3Key(gPos).ToString();

            var events = await GetS3Events(key, cancellationToken).ConfigureAwait(false);

            var toGet = events.OrderBy(e => e.GlobalSequenceNumber).Skip(gPos).Take(pageSize).ToList();

            var result = new List<ICommittedDomainEvent>();

            foreach (var e in toGet)
            {
                var dynamoEvent = await _dynamoDBContext.LoadAsync<DynamoDbEvent>(
                    hashKey: e.AggregateId,
                    rangeKey: e.SequenceNumber,
                    cancellationToken: cancellationToken);

                result.Add(dynamoEvent);
            }

            return new AllCommittedEventsPage(new GlobalPosition((gPos + toGet.Count).ToString()), result);
        }

        public async Task<IReadOnlyCollection<ICommittedDomainEvent>> LoadCommittedEventsAsync(IIdentity id, int fromEventSequenceNumber, CancellationToken cancellationToken)
        {
            var search = _dynamoDBContext.QueryAsync<DynamoDbEvent>(id.Value, QueryOperator.GreaterThanOrEqual, new List<object>() { fromEventSequenceNumber });

            return await search.GetRemainingAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<IList<S3Event>> GetCurrentEventObject(CancellationToken cancellationToken)
        {
            var objects = await _amazons3.GetAllObjectKeysAsync(EventBucketName, "", new Dictionary<string, object>()).ConfigureAwait(false);

            if (objects.Count == 0)
            {
                return null;
            }

            var current = objects
                .Select(s => int.Parse(s))
                .Max()
                .ToString();

            return await GetS3Events(current, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IList<S3Event>> GetS3Events(string current, CancellationToken cancellationToken)
        {
            using (var response = await _amazons3.GetObjectStreamAsync(EventBucketName, current, new Dictionary<string, object>(), cancellationToken).ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response))
                {
                    string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<IList<S3Event>>(content);
                }
            }
        }

        private int GetS3Key(int gPos)
        {
            return gPos / EventBucketSize * EventBucketSize;
        }
    }
}
