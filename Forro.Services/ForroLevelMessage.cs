using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Forro.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Forro.Services
{
    public interface IForroLevelMessage
    {
        Task EnsureSQSForroLevelCreation();
        Task SendMessageToForroLevelSQS(ForroLevel forroLevel);
    }

    public class ForroLevelMessage: IForroLevelMessage
    {
        private readonly string _forroLevelQueueName;
        private readonly RegionEndpoint _regionEndpoint;
        private readonly AmazonSQSClient _amazonSQSClient;

        public ForroLevelMessage(string forroLevelQueueName, RegionEndpoint regionEndpoint)
        {
            _forroLevelQueueName = forroLevelQueueName;
            _regionEndpoint = regionEndpoint;

            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = _regionEndpoint
            };

            //Setup AWS SQS Client to be used within class
            _amazonSQSClient = new AmazonSQSClient(sqsConfig);
        }
        /// <summary>
        /// It shoudl be ran just maximum once per app life cycle. 
        /// </summary>
        /// <returns></returns>
        public async Task EnsureSQSForroLevelCreation()
        {
            var createQueueRequest = new CreateQueueRequest();

            createQueueRequest.QueueName = _forroLevelQueueName;
            var attrs = new Dictionary<string, string>();
            attrs.Add(QueueAttributeName.VisibilityTimeout, "30");
            
            createQueueRequest.Attributes = attrs;

            var createQueueResponse = await _amazonSQSClient.CreateQueueAsync(createQueueRequest);
        }

        public async Task SendMessageToForroLevelSQS(ForroLevel forroLevel)
        {
            var request = new GetQueueUrlRequest(_forroLevelQueueName);

            var response = await _amazonSQSClient.GetQueueUrlAsync(request);

            var newForroLevelJson = JsonConvert.SerializeObject(forroLevel);

            var sendMessageRequest = new SendMessageRequest(response.QueueUrl, newForroLevelJson);
            var result = await _amazonSQSClient.SendMessageAsync(sendMessageRequest);
        }
    }
}
