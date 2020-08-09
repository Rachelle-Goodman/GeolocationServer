using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Geolocation.Constants;
using Geolocation.Utilities.Encryption;
using Geoloocation.DB;
using System;
using System.Collections.Generic;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    internal static class DynamoDbUtil
    {
        private static Dictionary<Type, (string tableName, string hashKeyName, string rangeKeyName)> _ddbTypeToMetadata;

        internal static (AmazonDynamoDBClient client, DynamoDBContext context) BuildDynamoDbAccessObjects()
        {
            _ddbTypeToMetadata = new Dictionary<Type, (string tableName, string hashKeyName, string rangeKeyName)>();

            var awsAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_ACCESS_KEY));
            var awsSecretAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_SECRET_ACCESS_KEY));

            var client = new AmazonDynamoDBClient(awsAccessKey, awsSecretAccessKey, RegionEndpoint.USEast1);
            var context = new DynamoDBContext(client);

            return (client, context);
        }

        internal static Dictionary<string, AttributeValue> GetKey<TEntity>(object hashKey, object rangeKey) where TEntity: DbEntityBase
        {
            var hashKeyName = GetHashKeyName<TEntity>();
            var keys = new Dictionary<string, AttributeValue>
            {
                [hashKeyName] = KeyToAttributeValue(hashKey),
            };

            if (rangeKey != null)
            {
                var rangeKeyName = GetRangeKeyName<TEntity>();
                keys[rangeKeyName] = KeyToAttributeValue(rangeKey);
            }

            return keys;
        }

        private static AttributeValue KeyToAttributeValue(object key)
            => key is string ? new AttributeValue { S = key.ToString() } : new AttributeValue { N = key.ToString() };

        internal static string GetTableName<TEntity>() where TEntity: DbEntityBase
        {
            var type = typeof(TEntity);
            AddTypeToDdbTypeToMetadataIfNotExists(type);
            return _ddbTypeToMetadata[type].tableName;
        }

        internal static string GetHashKeyName<TEntity>() where TEntity: DbEntityBase
        {
            var type = typeof(TEntity);
            AddTypeToDdbTypeToMetadataIfNotExists(type);
            return _ddbTypeToMetadata[type].hashKeyName;
        }

        internal static string GetRangeKeyName<TEntity>() where TEntity: DbEntityBase
        {
            var type = typeof(TEntity);
            AddTypeToDdbTypeToMetadataIfNotExists(type);
            return _ddbTypeToMetadata[type].rangeKeyName;
        }

        private static void AddTypeToDdbTypeToMetadataIfNotExists(Type type)
        {
            if (!_ddbTypeToMetadata.ContainsKey(type))
            {
                AddKeyToDdbTypeToMetadataDict(type);
            }
        }

        private static void AddKeyToDdbTypeToMetadataDict(Type type)
        {
            (string hashKeyName, string rangeKeyName) = FindKeysNamesOfType(type);
            string tableName = FindTableNameOfType(type);

            _ddbTypeToMetadata[type] = (tableName, hashKeyName, rangeKeyName);
        }

        private static (string hashKeyName, string rangeKeyName) FindKeysNamesOfType(Type type)
        {
            string hashKeyName = null, rangeKeyName = null;

            foreach (var prop in type.GetProperties())
            {
                var hashkeyAttributes = prop.GetCustomAttributes(typeof(DynamoDBHashKeyAttribute), true);
                var rangeKeyAttributes = prop.GetCustomAttributes(typeof(DynamoDBRangeKeyAttribute), true);

                if (hashkeyAttributes != null && hashkeyAttributes.Length > 0)
                {
                    hashKeyName = prop.Name;
                } else if (rangeKeyAttributes != null && rangeKeyAttributes.Length > 0)
                {
                    rangeKeyName = prop.Name;
                }
            }

            return (hashKeyName, rangeKeyName);
        }

        private static string FindTableNameOfType(Type type)
            => (type.GetCustomAttributes(typeof(DynamoDBTableAttribute), true)[0] as DynamoDBTableAttribute).TableName;

        internal static AttributeValue ToAttributeValue<TEntity>(DynamoDBContext context, TEntity item, DbType type)
        {
            switch (type)
            {
                case DbType.Bool:
                    return new AttributeValue { BOOL = Convert.ToBoolean(item) };

                case DbType.Number:
                    return new AttributeValue { N = item.ToString() };

                case DbType.String:
                    return new AttributeValue { S = item.ToString() };

                case DbType.Enum:
                    return new AttributeValue { N = item.ToString() };

                case DbType.DateTime:
                    return new AttributeValue { S = item.ToString() };

                case DbType.Null:
                    return new AttributeValue { NULL = true };

                case DbType.Map:
                    return new AttributeValue { M = context.ToDocument(item).ToAttributeMap() };

                case DbType.List:
                    return new AttributeValue { L = context.ToDocument(new AttributeValueConverterHelper<TEntity>(item)).ToAttributeMap()[nameof(AttributeValueConverterHelper<TEntity>.List)].L };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class AttributeValueConverterHelper<TEntity>
        {
            public AttributeValueConverterHelper() { } // ctor needed for dynamodb sdk

            public AttributeValueConverterHelper(TEntity list)
            {
                List = list;
            }

            [DynamoDBProperty]
            public TEntity List { get; set; }
        }
    }
}
