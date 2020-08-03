using Amazon;
using Amazon.DynamoDBv2;
using Geolocation.Constants;
using Geolocation.Utilities.Aws.DynamoDB.Entities;
using Geolocation.Utilities.Encryption;
using System;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    public static class DynamoDbHealthTester
    {
        public static async Task HealthCheck()
        {
            var awsAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_ACCESS_KEY));
            var awsSecretAccessKey = DesEncryptor.DecryptData(Environment.GetEnvironmentVariable(EnvironmentVariablesNames.ENCRYPTED_AWS_SECRET_ACCESS_KEY));

            var client = new AmazonDynamoDBClient(awsAccessKey, awsSecretAccessKey, RegionEndpoint.USEast1);

            await Task.WhenAll(
                CheckTableHealth<PlaceDdbDto>(client),
                CheckTableHealth<DistanceDdbDto>(client));
        }

        private static async Task CheckTableHealth<T>(AmazonDynamoDBClient client) where T : DynamoDbEntityBase
            => await client.DescribeTableAsync(DynamoDbUtil.GetTableName<T>());
    }
}
