using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Forro.Data;
using Forro.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Forro.Services
{
    public interface IForroLevelService
    {
        Task<IList<ForroLevel>> GetAll();
        Task<ForroLevel> Insert(ForroLevel forroLevel);
        Task<ForroLevel> Insert(ForroLevel forroLevel, Stream forroLogoStream);
        void Delete(int id);

        /// <summary>
        /// It is meant to be used asynchronously via a combination of SQS and Lambda
        /// But since it is available via IForroLevelService, it will work anywhere :)
        /// </summary>
        /// <param name="forroLevel"></param>
        Task NotifySubscribersAboutNewForroLevel(ForroLevel forroLevel);
    }

    public class ForroLevelService : IForroLevelService
    {
        private readonly IForroLevelRepository _repository;
        private readonly IAmazonS3 _amazonS3;

        private readonly ILoggerManager _loggerManager;
        //Ideally, below configuration would be placed either on a configuration file, Database, CICD tool as a variable, etc. But not here! lol
        private readonly string _bucketName;
        private readonly string _bucketFullUrl;
        private const string _forroLevelFolder = "level/";
        private readonly string _forroLevelSNSTopicArn;
        private readonly RegionEndpoint _awsRegionEndpoint;

        private readonly IForroLevelMessage _forroLevelMessage;

        public ForroLevelService(IForroLevelRepository repository, 
            IAmazonS3 amazonS3,
            ILoggerManager loggerManager,
            IForroLevelMessage forroLevelMessage,
            IForroAppConfig forroAppConfig)
        {
            _repository = repository;
            _amazonS3 = amazonS3;
            _loggerManager = loggerManager;

            _bucketName = forroAppConfig.AWSForroBucketName;
            _bucketFullUrl = $"https://s3.{forroAppConfig.AWSRegionEndpoint}.amazonaws.com/{_bucketName}/";
            _forroLevelMessage = forroLevelMessage;
            _forroLevelSNSTopicArn = forroAppConfig.ForroLevelSNSTopicArn;
            _awsRegionEndpoint = RegionEndpoint.GetBySystemName(forroAppConfig.AWSRegionEndpoint);
        }
        #region IForroLevelService implementation
        public async Task<IList<ForroLevel>> GetAll()
        {
            _loggerManager.LogInfo("Method GetAll() was called");

            var result = await _repository.GetAll();

            foreach(var forroLevel in result)
            {
                if (!string.IsNullOrWhiteSpace(forroLevel.ImageUrl))
                    forroLevel.ImageUrl = _bucketFullUrl + forroLevel.ImageUrl;

                if (!string.IsNullOrWhiteSpace(forroLevel.ThumbNailImageUrl))
                    forroLevel.ThumbNailImageUrl = _bucketFullUrl + forroLevel.ThumbNailImageUrl;
            }
            
            return result;
        }

        public async Task<ForroLevel> Insert(ForroLevel forroLevel)
        {
            var result = await Insert(forroLevel, null);
            return result;
        }

        public async Task<ForroLevel> Insert(ForroLevel forroLevel, Stream fileLogoStream)
        {
            var imageUrl = await UploadFileToS3(fileLogoStream, forroLevel.ForroLevelId,
                forroLevel.Name, forroLevel.ImageUrl, UrlOrThumbNail.URL);
            
            //Update using actual S3 ObjectKey
            forroLevel.ImageUrl = imageUrl;
            var newForroLevel = await _repository.Insert(forroLevel);

            //Send a message to SQS notifying about the new Forró level
            await _forroLevelMessage.SendMessageToForroLevelSQS(newForroLevel);

            return newForroLevel;
        }

        public async Task<ForroLevel> Update(ForroLevel forroLevel, Stream fileLogoStream, Stream fileThumbNailStream)
        {
            var imageUrl = await UploadFileToS3(fileLogoStream, forroLevel.ForroLevelId,
                forroLevel.Name, forroLevel.ImageUrl, UrlOrThumbNail.URL);

            var thumbNailImageUrl = await UploadFileToS3(fileThumbNailStream, forroLevel.ForroLevelId,
                forroLevel.Name, forroLevel.ThumbNailImageUrl, UrlOrThumbNail.ThumbNail);

            //Update using actual S3 ObjectKey
            forroLevel.ImageUrl = imageUrl;
            forroLevel.ThumbNailImageUrl = thumbNailImageUrl;

            var newForroLevel = await _repository.Update(forroLevel);
            
            return newForroLevel;
        }

        public void Delete(int id)
        {
            var forroLevel = _repository.Get(id);
            _repository.Delete(id);

            //Delete file from S3 Bucket
            //Run synchronously. Not ideal I know, but my goal is to be straightforward and learn AWS, so this is faster development 
            //TODO: This logic is seriously fragile, I would never approve it during a code-review lol

            if (!string.IsNullOrWhiteSpace(forroLevel.ImageUrl))
            {
                _amazonS3.DeleteAsync(_bucketName, forroLevel.ImageUrl, null).Wait();
            }
            if (!string.IsNullOrWhiteSpace(forroLevel.ThumbNailImageUrl))
            {
                _amazonS3.DeleteAsync(_bucketName, forroLevel.ThumbNailImageUrl, null).Wait();
            }
        }
        #endregion IForroLevelService implementation

        #region Private Methods

        private async Task<string> UploadFileToS3(Stream fileStream, int forroLevelId, string forroLevelName, string imageUrl,
            UrlOrThumbNail urlOrThumbNail)
        {
            //If there is a file being uploaded
            if (fileStream != null)
            {
                var fileExtension = Path.GetExtension(imageUrl);

                var objectKey = GetNewImageUrlObjectKey(forroLevelId, forroLevelName, urlOrThumbNail,
                    fileExtension);

                //var stream = new FileStream();
                var request = new PutObjectRequest()
                {
                    Key = objectKey,
                    BucketName = _bucketName,
                    InputStream = fileStream
                };

                await _amazonS3.PutObjectAsync(request);
                return objectKey;
            }
            return "";
        }
        private string GetNewImageUrlObjectKey(int forroLevelId, string forroLevelName, UrlOrThumbNail urlOrThumbNail, 
            string fileExtension)
        {
            var result = Regex.Replace(forroLevelName.Trim(), "[^a-zA-Z0-9_.]+", "-", RegexOptions.Compiled);

            result = forroLevelId + "-" + result.ToLower() + "-" + urlOrThumbNail.ToString().ToLower();
            result = result + fileExtension;

            result = _forroLevelFolder + result;
            return result;
        }

        public async Task NotifySubscribersAboutNewForroLevel(ForroLevel forroLevel)
        {
            var clientSNS = new AmazonSimpleNotificationServiceClient(_awsRegionEndpoint);

            var message = $"The name of new Forró Level is {forroLevel.Name} and ID {forroLevel.ForroLevelId}";

            var publish = new PublishRequest
            {
                TopicArn = _forroLevelSNSTopicArn,
                Subject = "New Forró Level has been created",
                Message = message
            };

            _loggerManager.LogInfo($"SNS Topic ARN is {_forroLevelSNSTopicArn}");

            var result = await clientSNS.PublishAsync(publish);
            if (IsSuccessStatusCode(result.HttpStatusCode))
                _loggerManager.LogInfo($"Successfully sent a message to SNS Topic {_forroLevelSNSTopicArn}");
            else
                throw new Exception($"Error trying to publish on SNS topic " +
                    $"{_forroLevelSNSTopicArn}. HttpStatus returned is {result.HttpStatusCode}");
                    
        }

        private bool IsSuccessStatusCode(HttpStatusCode httpStatusCode)
        {
            return ((int)httpStatusCode >= 200) && ((int)httpStatusCode <= 299); 
        }

        private enum UrlOrThumbNail
        {
            URL, ThumbNail
        }
        #endregion Private Methods
    }
}