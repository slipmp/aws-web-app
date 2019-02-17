using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Forro.Domain;
using Forro.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Forro.Lambda
{
    public class ForroLevelLambda
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IForroLevelService _forroLevelService;
        
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public ForroLevelLambda()
        {
            var forroAppConfig = new ForroAppConfig
            {
                AWSForroBucketName = Environment.GetEnvironmentVariable("AWSForroBucketName"),
                AWSRegionEndpoint = Environment.GetEnvironmentVariable("AWSRegionEndpoint"),
                ForroLevelSNSTopicArn = Environment.GetEnvironmentVariable("ForroLevelSNSTopicArn")
            };

            //Create Dependency Injection Container. Ideally I would use AutoFac, but this for now meets my expectations.
            _serviceCollection = new ServiceCollection();

            //_serviceCollection.
            var forroDependencyInjectionService = new ForroDependencyInjectionService(forroAppConfig);
            forroDependencyInjectionService.DeclareDependencies(_serviceCollection);

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            _forroLevelService = serviceProvider.GetService<IForroLevelService>();

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            ///One of the implications of using this feature is that you do not need to 
            ///delete SQS messages in Lambda function because AWS will do it for you, but only when the 
            ///Lambda completes successfully.
            /// When your function successfully processes a batch, Lambda deletes its messages from the queue.
            /// https://docs.aws.amazon.com/lambda/latest/dg/with-sqs.html

            if (evnt == null || evnt.Records == null)
            {
                context.Logger.LogLine("WARN: A SQS Event must be provided in order to Lambda be functional. Returning withough further processing.");
                return;
            }

            context.Logger.LogLine("SQSEvent.ToString() => " + evnt.ToString());
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Start processing Forró Level message and creating its ThumbNail!");
            context.Logger.LogLine($"Message Body: {message.Body}");

            var result = await _forroLevelService.GetAll();

            context.Logger.LogLine($"Quantity of Forro Level in DynamoDB: { result.Count}");
            var forroLevel = JsonConvert.DeserializeObject<ForroLevel>(message.Body);

            context.Logger.LogLine("Starting sending message to SNS Topic.");
            await _forroLevelService.NotifySubscribersAboutNewForroLevel(forroLevel);
            context.Logger.LogLine("SNS Topic is sent.");
        }
    }
}
