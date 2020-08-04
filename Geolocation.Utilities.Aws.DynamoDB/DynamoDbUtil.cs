using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Geolocation.Constants;
using Geolocation.Utilities.Aws.DynamoDB.Entities;
using Geolocation.Utilities.Encryption;
using System;
using System.Collections.Generic;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    internal static class DynamoDbUtil
    {
        internal static (AmazonDynamoDBClient client, DynamoDBContext context) BuildDynamoDbAccessObjects()
        {
            var awsAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_ACCESS_KEY));
            var awsSecretAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_SECRET_ACCESS_KEY));

            var client = new AmazonDynamoDBClient(awsAccessKey, awsSecretAccessKey, RegionEndpoint.USEast1);
            var context = new DynamoDBContext(client);

            return (client, context);
        }

        internal static Dictionary<string, AttributeValue> GetKey<T>(object hashKey, object rangeKey) where T: DynamoDbEntityBase
        {
            var hashKeyName = GetHashKeyName<T>();
            var keys = new Dictionary<string, AttributeValue>
            {
                [hashKeyName] = KeyToAttributeValue(hashKey),
            };

            if (rangeKey != null)
            {
                var rangeKeyName = GetRangeKeyName<T>();
                keys[rangeKeyName] = KeyToAttributeValue(rangeKey);
            }

            return keys;
        }

        private static AttributeValue KeyToAttributeValue(object key)
            => key is string ? new AttributeValue { S = key.ToString() } : new AttributeValue { N = key.ToString() };

        internal static string GetTableName<T>() where T : DynamoDbEntityBase
        {
            var type = typeof(T);
            AddTypeToDdbTypeToMetadataIfNotExists(type);
            return _ddbTypeToMetadata[type].tableName;
        }

        internal static string GetHashKeyName<T>() where T : DynamoDbEntityBase
        {
            var type = typeof(T);
            AddTypeToDdbTypeToMetadataIfNotExists(type);
            return _ddbTypeToMetadata[type].hashKeyName;
        }

        internal static string GetRangeKeyName<T>() where T : DynamoDbEntityBase
        {
            var type = typeof(T);
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

        private static Dictionary<Type, (string tableName, string hashKeyName, string rangeKeyName)> _ddbTypeToMetadata = new Dictionary<Type, (string tableName, string hashKeyName, string rangeKeyName)>();

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

        internal static AttributeValue ToAttributeValue<T>(DynamoDBContext context, T item, DynamoDbType type)
        {
            switch (type)
            {
                case DynamoDbType.Bool:
                    return new AttributeValue { BOOL = Convert.ToBoolean(item) };

                case DynamoDbType.Number:
                    return new AttributeValue { N = item.ToString() };

                case DynamoDbType.String:
                    return new AttributeValue { S = item.ToString() };

                case DynamoDbType.Enum:
                    return new AttributeValue { N = item.ToString() };

                case DynamoDbType.DateTime:
                    return new AttributeValue { S = item.ToString() };

                case DynamoDbType.Null:
                    return new AttributeValue { NULL = true };

                case DynamoDbType.Map:
                    return new AttributeValue { M = context.ToDocument(item).ToAttributeMap() };

                case DynamoDbType.List:
                    return new AttributeValue { L = context.ToDocument(new AttributeValueConverterHelper<T>(item)).ToAttributeMap()[nameof(AttributeValueConverterHelper<T>.List)].L };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class AttributeValueConverterHelper<T>
        {
            public AttributeValueConverterHelper() { } // ctor needed for dynamodb sdk

            public AttributeValueConverterHelper(T list)
            {
                List = list;
            }

            [DynamoDBProperty]
            public T List { get; set; }
        }
    }

    public enum DynamoDbType
    {
        Bool,
        Number,
        String,
        Enum,
        DateTime,
        Null,
        List,
        Map,
    }
}
