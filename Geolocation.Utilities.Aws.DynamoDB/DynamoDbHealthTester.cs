using Amazon.DynamoDBv2;
using Geolocation.Utilities.Aws.DynamoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    public static class DynamoDbHealthTester
    {
        public static async Task HealthCheck()
        {
            (AmazonDynamoDBClient client, _) = DynamoDbUtil.BuildDynamoDbAccessObjects();
            List<Type> dynamoDbEntities = GetDynamoDbEntities();
            await CheckTablesHealth(client, dynamoDbEntities);
        }

        private static async Task CheckTablesHealth(AmazonDynamoDBClient client, List<Type> dynamoDbEntities)
        {
            List<Task> checkTablesHealthTasks = new List<Task>();

            foreach (var type in dynamoDbEntities)
            {
                var method = typeof(DynamoDbHealthTester).GetMethod(nameof(DynamoDbHealthTester.CheckTableHealth));
                object checkTableHealthTask = method.MakeGenericMethod(type).Invoke(obj: null, parameters: new object[] { client });
                checkTablesHealthTasks.Add((Task)checkTableHealthTask);
            }

            await Task.WhenAll(checkTablesHealthTasks);
        }

        private static List<Type> GetDynamoDbEntities()
        {
            var dynamoDbEntities = new List<Type>();

            foreach (var domain_assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assembly_types = domain_assembly.GetTypes()
                  .Where(type => type.IsSubclassOf(typeof(DynamoDbEntityBase)) && !type.IsAbstract);

                dynamoDbEntities.AddRange(assembly_types);
            }

            return dynamoDbEntities;
        }

        public static async Task CheckTableHealth<T>(AmazonDynamoDBClient client) where T : DynamoDbEntityBase
            => await client.DescribeTableAsync(DynamoDbUtil.GetTableName<T>());
    }
}
