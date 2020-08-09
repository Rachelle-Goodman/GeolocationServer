using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Geolocation.DependencyInjection;
using Geoloocation.DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    [DependencyInjection(DependencyInjectionType.Singleton)]
    public class DynamoDbAdapter: IDB
    {
        private static readonly AmazonDynamoDBClient _client;
        private static readonly DynamoDBContext _context;

        static DynamoDbAdapter()
        {
            (_client, _context) = DynamoDbUtil.BuildDynamoDbAccessObjects();
        }

        public async Task Insert<TEntity>(TEntity item) where TEntity: DbEntityBase
        {
            var request = BuildPutItemRequest(item);
            await _client.PutItemAsync(request);
        }

        public async Task Update<TEntity>(TEntity item, object hashKey, object rangeKey = null) where TEntity: DbEntityBase
        {
            var request = BuildUpdateItemRequest(item, hashKey, rangeKey);
            await _client.UpdateItemAsync(request);
        }

        private PutItemRequest BuildPutItemRequest<TEntity>(TEntity item) where TEntity: DbEntityBase
        {
            return new PutItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Item = DynamoDbUtil.ToAttributeValue(_context, item, DbType.Map).M,
            };
        }

        private UpdateItemRequest BuildUpdateItemRequest<TEntity>(TEntity item, object hashKey, object rangeKey) where TEntity: DbEntityBase
        {
            var hashKeyName = DynamoDbUtil.GetHashKeyName<TEntity>();
            var updateExpression = "SET ";
            var expressionAttributeNames = new Dictionary<string, string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();

            int counter = 0;
            foreach (var newValue in _context.ToDocument(item).ToAttributeMap())
            {
                if (hashKeyName == newValue.Key)
                {
                    continue;
                }

                string attributeName = $"val{counter++}";
                string expressionAttributeName = $"#{attributeName}";
                string expressionAttributeValue = $":{attributeName}";

                updateExpression += $"{expressionAttributeName} = {expressionAttributeValue}, ";
                expressionAttributeNames[expressionAttributeName] = newValue.Key;
                expressionAttributeValues[expressionAttributeValue] = newValue.Value;
            }

            // Remove last comma and space from end of string
            updateExpression = updateExpression.Substring(0, updateExpression.Length - 2);

            var request = new UpdateItemRequest
            {
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues,
                UpdateExpression = updateExpression,
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
            };
            return request;
        }

        public async Task Update<TEntity>(Dictionary<string, (object value, DbType type)> attributesToUpdate, object hashKey, object rangeKey = null) where TEntity: DbEntityBase
        {
            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
                AttributeUpdates = attributesToUpdate.ToDictionary(
                    x => x.Key,
                    x => new AttributeValueUpdate(
                        DynamoDbUtil.ToAttributeValue(_context, x.Value.value, x.Value.type),
                        AttributeAction.PUT)),
            };

            await _client.UpdateItemAsync(request);
        }

        public async Task UpdateList<TEntity, TObject>(string attributeName, List<TObject> list, object hashKey, object rangeKey = null) where TEntity: DbEntityBase
        {
            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
                AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                {
                    [attributeName] = new AttributeValueUpdate(DynamoDbUtil.ToAttributeValue(_context, list, DbType.List), AttributeAction.PUT),
                },
            };

            await _client.UpdateItemAsync(request);
        }

        public async Task Remove<TEntity>(object hashKey, object rangeKey = null) where TEntity: DbEntityBase
        {
            var request = new DeleteItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
            };

            await _client.DeleteItemAsync(request);
        }

        public async Task<TEntity> Get<TEntity>(object hashKey, object rangeKey = null, List<string> attributesToGet = null) where TEntity: DbEntityBase
        {
            var request = new GetItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
                AttributesToGet = null,
            };

            var response = await _client.GetItemAsync(request);

            if (response.Item.Count == 0)
            {
                return default;
            }

            return _context.FromDocument<TEntity>(Document.FromAttributeMap(response.Item));
        }

        public async Task<IEnumerable<TEntity>> GetAll<TEntity>(params ScanCondition[] conditions) where TEntity: DbEntityBase
        {
            return await _context.ScanAsync<TEntity>(conditions).GetRemainingAsync();
        }

        public async Task AddToList<TEntity, TObject>(TObject item, string attributeName, object hashKey, object rangeKey = null) where TEntity : DbEntityBase
        {
            const string itemName = ":item";
            const string emptyListName = ":emptyList";

            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<TEntity>(),
                Key = DynamoDbUtil.GetKey<TEntity>(hashKey, rangeKey),
                UpdateExpression = $"SET {attributeName} = list_append(if_not_exists({attributeName}, {emptyListName}), {itemName})",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [itemName] = DynamoDbUtil.ToAttributeValue(_context, new List<TObject> { item }, DbType.List),
                    [emptyListName] = new AttributeValue { IsLSet = true },
                },
                ReturnValues = "UPDATED_NEW"
            };

            await _client.UpdateItemAsync(request);
        }
    }
}
