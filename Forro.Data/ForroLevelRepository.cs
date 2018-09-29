using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Forro.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forro.Data
{
    public interface IForroLevelRepository
    {
        IList<ForroLevel> GetAll();
    }

    public class ForroLevelRepository : IForroLevelRepository
    {
        private const string ForroLevelTableName = "ForroLevel";

        public IList<ForroLevel> GetAll()
        {
            var config = new AmazonDynamoDBConfig();
            config.RegionEndpoint = RegionEndpoint.USEast2;

            var client = new AmazonDynamoDBClient(config);

            var request = new ScanRequest()
            {
                TableName=ForroLevelTableName
            };

            var queryResult = client.ScanAsync(request).Result;

            var resultList = queryResult.Items.Select(MapForroLevel).ToList();

            return resultList;
        }

        private ForroLevel MapForroLevel(Dictionary<string, AttributeValue> result)
        {
            return new ForroLevel
            {
                ForroLevelId = Convert.ToInt32(result["ForroLevelId"].N),
                Name = result["Name"].S
            };
        }
    }
}