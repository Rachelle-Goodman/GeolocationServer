using Amazon.DynamoDBv2;
using Geolocation.DependencyInjection;
using Geolocation.Entities;
using Geoloocation.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Aws.DynamoDB
{
    [DependencyInjection(DependencyInjectionType.Singleton)]
    public class DynamoDbHealthTester: IDbHealthTester
    {
        public async Task HealthCheck()
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
                var method = typeof(DynamoDbHealthTester).GetMethod(nameof(DynamoDbHealthTester.CheckTableHealth), BindingFlags.Static | BindingFlags.NonPublic);
                object checkTableHealthTask = method.MakeGenericMethod(type).Invoke(obj: null, parameters: new object[] { client });
                checkTablesHealthTasks.Add((Task)checkTableHealthTask);
            }

            await Task.WhenAll(checkTablesHealthTasks);
        }

        private static List<Type> GetDynamoDbEntities() =>
            Assembly.GetAssembly(typeof(DynamoDbEntityBase)).GetTypes().Where(type => type.IsSubclassOf(typeof(DynamoDbEntityBase))).ToList();

        private static async Task CheckTableHealth<TEntity>(AmazonDynamoDBClient client) where TEntity: DynamoDbEntityBase
            => await client.DescribeTableAsync(DynamoDbUtil.GetTableName<TEntity>());
    }
}
