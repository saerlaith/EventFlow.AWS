using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EventFlow.Aggregates;
using EventFlow.DynamoDB.Configuration;
using EventFlow.Logs;
using EventFlow.ReadStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventFlow.DynamoDB.ReadStore
{
    public class DynamoDBReadStore<TReadModel> : ReadModelStore<TReadModel>
        where TReadModel : class, IReadModel, new()
    {
        private readonly ILog _log;
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly DynamoDBContext _dynamoDBContext;
        private readonly IAWSConfiguration _configuration;

        public DynamoDBReadStore(ILog log, IAmazonDynamoDB dynamoDB, IAWSConfiguration configuration) : base(log)
        {
            _log = log;
            _dynamoDBClient = dynamoDB;
            _dynamoDBContext = new DynamoDBContext(dynamoDB, configuration.ContextConfig ?? new DynamoDBContextConfig());
            _configuration = configuration;
        }

        public async Task Initialize(bool createTables = false, ProvisionedThroughput provisionedThroughput = null, StreamSpecification streamSpecification = null)
        {
            if (createTables)
            {
                Table table = _dynamoDBContext.GetTargetTable<TReadModel>();

                var createTableRequest = new CreateTableRequest()
                {
                    ProvisionedThroughput = provisionedThroughput,
                    TableName = table.TableName,
                    LocalSecondaryIndexes = table.LocalSecondaryIndexes
                    .Select(kv => new LocalSecondaryIndex()
                    {
                        KeySchema = kv.Value.KeySchema,
                        IndexName = kv.Value.IndexName,
                        Projection = kv.Value.Projection
                    }).ToList(),
                    GlobalSecondaryIndexes = table.GlobalSecondaryIndexes.Select(kv => new GlobalSecondaryIndex()
                    {
                        KeySchema = kv.Value.KeySchema,
                        Projection = kv.Value.Projection,
                        IndexName = kv.Value.IndexName,
                        ProvisionedThroughput = new ProvisionedThroughput(kv.Value.ProvisionedThroughput.ReadCapacityUnits, kv.Value.ProvisionedThroughput.WriteCapacityUnits)
                    }).ToList(),
                    AttributeDefinitions = table.Attributes,
                    StreamSpecification = streamSpecification,
                    KeySchema = table.Keys.Select(kv => new KeySchemaElement(kv.Key, kv.Value.IsHash ? KeyType.HASH : KeyType.RANGE)).ToList()
                };

                await _dynamoDBClient.CreateTableAsync(createTableRequest).ConfigureAwait(false);
            }
        }

        public override async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            Table table = _dynamoDBContext.GetTargetTable<TReadModel>();
            await _dynamoDBClient.DeleteTableAsync(table.TableName, cancellationToken)
                .ContinueWith(async response =>
                {
                    var deleteTableResponse = response.Result.TableDescription;
                    await _dynamoDBClient.CreateTableAsync(
                        deleteTableResponse.TableName,
                        deleteTableResponse.KeySchema,
                        deleteTableResponse.AttributeDefinitions,
                        new ProvisionedThroughput(
                            deleteTableResponse.ProvisionedThroughput.ReadCapacityUnits,
                            deleteTableResponse.ProvisionedThroughput.WriteCapacityUnits),
                        cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            await _dynamoDBContext.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            TReadModel response = await _dynamoDBContext.LoadAsync<TReadModel>(id, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                return ReadModelEnvelope<TReadModel>.Empty(id);
            }

            return ReadModelEnvelope<TReadModel>.With(id, response);
        }

        public override async Task UpdateAsync(IReadOnlyCollection<ReadModelUpdate> readModelUpdates, IReadModelContext readModelContext, Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken, Task<ReadModelEnvelope<TReadModel>>> updateReadModel, CancellationToken cancellationToken)
        {
            foreach (var update in readModelUpdates)
            {
                var envelope = await GetAsync(update.ReadModelId, cancellationToken).ConfigureAwait(false);

                var updatedModel = await updateReadModel(readModelContext, update.DomainEvents, envelope, cancellationToken).ConfigureAwait(false);

                await _dynamoDBContext.SaveAsync(updatedModel.ReadModel, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}