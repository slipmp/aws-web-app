﻿using Amazon;
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
        void Insert(ForroLevel forroLevel);

        void Delete(int id);
    }

    public class ForroLevelRepository : IForroLevelRepository
    {
        private const string ForroLevelTableName = "ForroLevel";
        private readonly IAmazonDynamoDB _client;

        public ForroLevelRepository(IAmazonDynamoDB client)
        {
            _client = client;
        }

        #region Interfaces
        public IList<ForroLevel> GetAll()
        {
            var request = new ScanRequest()
            {
                TableName = ForroLevelTableName
            };

            var queryResult = _client.ScanAsync(request).Result;

            var resultList = queryResult.Items.Select(MapForroLevel).ToList();

            return resultList;
        }

        public void Insert(ForroLevel forroLevel)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                [nameof(ForroLevel.ForroLevelId)] = new AttributeValue() { N = forroLevel.ForroLevelId.ToString() },
                [nameof(ForroLevel.Name)] = new AttributeValue() { S = forroLevel.Name }
            };

            var request = new PutItemRequest()
            {
                TableName = ForroLevelTableName,
                Item = attributes
            };
            var result = _client.PutItemAsync(request).Result;
        }
        public void Delete(int id)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                [nameof(ForroLevel.ForroLevelId)] = new AttributeValue() { N = id.ToString() }
            };

            var request = new DeleteItemRequest()
            {
                TableName = ForroLevelTableName,
                Key = attributes
            };

            var result = _client.DeleteItemAsync(request).Result;
        }

        #endregion Interfaces

        private ForroLevel MapForroLevel(Dictionary<string, AttributeValue> result)
        {
            var forroLevel = new ForroLevel
            {
                ForroLevelId = Convert.ToInt32(result["ForroLevelId"].N),
                Name = result["Name"].S
            };

            if (result.ContainsKey("ImageUrl"))
                forroLevel.ImageUrl = result["ImageUrl"].S;

            return forroLevel;
        }
    }
}