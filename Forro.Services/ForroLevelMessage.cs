using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forro.Services
{
    public interface IForroLevelMessage
    {
        Task EnsureSQSForroLevelCreation();
    }

    public class ForroLevelMessage: IForroLevelMessage
    {
        private readonly string _forroLevelQueueName;
        private readonly RegionEndpoint _regionEndpoint;

        public ForroLevelMessage(string forroLevelQueueName, RegionEndpoint regionEndpoint)
        {
            _forroLevelQueueName = forroLevelQueueName;
            _regionEndpoint = regionEndpoint;
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

            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.RegionEndpoint = _regionEndpoint;

            var sqsClient = new AmazonSQSClient(sqsConfig);

            var createQueueResponse = await sqsClient.CreateQueueAsync(createQueueRequest);
        }
    }
}
