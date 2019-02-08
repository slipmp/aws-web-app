using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Forro.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forro.Data
{
    public interface IForroLevelRepository
    {
        ForroLevel Get(int id);
        Task<IList<ForroLevel>> GetAll();
        Task<ForroLevel> Insert(ForroLevel forroLevel);
        Task<ForroLevel> Update(ForroLevel forroLevel);
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
        public ForroLevel Get(int id)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                [nameof(ForroLevel.ForroLevelId)] = new AttributeValue() { N = id.ToString() }
            };

            var request = new GetItemRequest()
            {
                TableName = ForroLevelTableName,
                Key = attributes
            };

            var queryResult = _client.GetItemAsync(request).Result;
            var result = MapForroLevel(queryResult.Item);
            return result;
        }
        public async Task<IList<ForroLevel>> GetAll()
        {
            var request = new ScanRequest()
            {
                TableName = ForroLevelTableName
            };
            
            var queryResult = await _client.ScanAsync(request);

            var resultList = queryResult.Items.Select(MapForroLevel).ToList();

            return resultList;
        }

        public async Task<ForroLevel> Insert(ForroLevel forroLevel)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                [nameof(ForroLevel.ForroLevelId)] = new AttributeValue() { N = forroLevel.ForroLevelId.ToString() },
                [nameof(ForroLevel.Name)] = new AttributeValue() { S = forroLevel.Name }
            };

            if (!string.IsNullOrWhiteSpace(forroLevel.ImageUrl))
                attributes.Add(nameof(ForroLevel.ImageUrl), new AttributeValue() { S = forroLevel.ImageUrl });

            if (!string.IsNullOrWhiteSpace(forroLevel.ThumbNailImageUrl))
                attributes.Add(nameof(ForroLevel.ThumbNailImageUrl), new AttributeValue() { S = forroLevel.ThumbNailImageUrl });

            var request = new PutItemRequest()
            {
                TableName = ForroLevelTableName,
                Item = attributes
            };
            var result = await _client.PutItemAsync(request);
            
            return forroLevel;

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

        public async Task<ForroLevel> Update(ForroLevel forroLevel)
        {
            //The PutItem operation also can perform an update. For more information, see Putting an Item. 
            ///For example, if you call PutItem to upload an item and the primary key exists, 
            ///the PutItem operation replaces the entire item. Note that, 
            ///if there are attributes in the existing item and those attributes are not specified in the input, 
            ///the PutItem operation deletes those attributes. However, UpdateItem only updates the specified input attributes, 
            ///any other existing attributes of that item remain unchanged.
            
            await Insert(forroLevel);

            return forroLevel;
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

            if (result.ContainsKey("ThumbNailImageUrl"))
                forroLevel.ThumbNailImageUrl = result["ThumbNailImageUrl"].S;

            return forroLevel;
        }
    }
}