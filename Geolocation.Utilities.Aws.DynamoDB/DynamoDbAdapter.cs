using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Geolocation.Constants;
using Geolocation.Utilities.Aws.DynamoDB.Entities;
using Geolocation.Utilities.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    public static class DynamoDbAdapter
    {
        private static readonly AmazonDynamoDBClient _client;
        private static readonly DynamoDBContext _context;

        static DynamoDbAdapter()
        {
            var awsAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_ACCESS_KEY));
            var awsSecretAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_SECRET_ACCESS_KEY));

            _client = new AmazonDynamoDBClient(awsAccessKey, awsSecretAccessKey, RegionEndpoint.USEast1);
            _context = new DynamoDBContext(_client);
        }

        public static async Task Insert<T>(T item) where T : DynamoDbEntityBase
        {
            var request = BuildPutItemRequest(item);
            await _client.PutItemAsync(request);
        }

        public static async Task Update<T>(T item, object hashKey, object rangeKey = null) where T : DynamoDbEntityBase
        {
            var request = BuildUpdateItemRequest(item, hashKey, rangeKey);
            await _client.UpdateItemAsync(request);
        }

        private static PutItemRequest BuildPutItemRequest<T>(T item) where T : DynamoDbEntityBase
        {
            return new PutItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<T>(),
                Item = DynamoDbUtil.ToAttributeValue(_context, item, DynamoDbType.Map).M,
            };
        }

        private static UpdateItemRequest BuildUpdateItemRequest<T>(T item, object hashKey, object rangeKey) where T : DynamoDbEntityBase
        {
            var hashKeyName = DynamoDbUtil.GetHashKeyName<T>();
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
                TableName = DynamoDbUtil.GetTableName<T>(),
                Key = DynamoDbUtil.GetKey<T>(hashKey, rangeKey),
            };
            return request;
        }

        public static async Task Update<T>(Dictionary<string, (object value, DynamoDbType type)> attributesToUpdate, object hashKey, object rangeKey = null) where T : DynamoDbEntityBase
        {
            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<T>(),
                Key = DynamoDbUtil.GetKey<T>(hashKey, rangeKey),
                AttributeUpdates = attributesToUpdate.ToDictionary(
                    x => x.Key,
                    x => new AttributeValueUpdate(
                        DynamoDbUtil.ToAttributeValue(_context, x.Value.value, x.Value.type),
                        AttributeAction.PUT)),
            };

            await _client.UpdateItemAsync(request);
        }

        public static async Task UpdateList<T, U>(string attributeName, List<U> list, object hashKey, object rangeKey = null) where T : DynamoDbEntityBase
        {
            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<T>(),
                Key = DynamoDbUtil.GetKey<T>(hashKey, rangeKey),
                AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                {
                    [attributeName] = new AttributeValueUpdate(DynamoDbUtil.ToAttributeValue(_context, list, DynamoDbType.List), AttributeAction.PUT),
                },
            };

            await _client.UpdateItemAsync(request);
        }

        public static async Task Remove<T>(object hashKey, object rangeKey = null) where T : DynamoDbEntityBase
        {
            var request = new DeleteItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<T>(),
                Key = DynamoDbUtil.GetKey<T>(hashKey, rangeKey),
            };

            await _client.DeleteItemAsync(request);
        }

        public static async Task<T> Get<T>(object hashKey, object rangeKey = null, List<string> attributesToGet = null) where T : DynamoDbEntityBase
        {
            var request = new GetItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<T>(),
                Key = DynamoDbUtil.GetKey<T>(hashKey, rangeKey),
                AttributesToGet = null,
            };

            var response = await _client.GetItemAsync(request);

            if (response.Item.Count == 0)
            {
                return null;
            }

            return _context.FromDocument<T>(Document.FromAttributeMap(response.Item));
        }

        public static async Task<IEnumerable<T>> GetAll<T>(params ScanCondition[] conditions) where T : DynamoDbEntityBase
        {
            return await _context.ScanAsync<T>(conditions).GetRemainingAsync();
        }

        public static async Task AddToList<T, U>(T item, DynamoDbType itemType, string attributeName, object hashKey, object rangeKey = null) where U : DynamoDbEntityBase
        {
            const string itemName = ":item";
            const string emptyListName = ":emptyList";

            var request = new UpdateItemRequest
            {
                TableName = DynamoDbUtil.GetTableName<U>(),
                Key = DynamoDbUtil.GetKey<U>(hashKey, rangeKey),
                UpdateExpression = $"SET {attributeName} = list_append(if_not_exists({attributeName}, {emptyListName}), {itemName})",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [itemName] = DynamoDbUtil.ToAttributeValue(_context, new List<T> { item }, DynamoDbType.List),
                    [emptyListName] = new AttributeValue { IsLSet = true },
                },
                ReturnValues = "UPDATED_NEW"
            };

            await _client.UpdateItemAsync(request);
        }
    }
}
