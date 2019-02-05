using Amazon.S3;
using Amazon.S3.Model;
using Forro.Data;
using Forro.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Forro.Services
{
    public interface IForroLevelService
    {
        Task<IList<ForroLevel>> GetAll();
        void Insert(ForroLevel forroLevel);
        void Insert(ForroLevel forroLevel, Stream forroLogoStream);
        void Delete(int id);
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
        
        public ForroLevelService(IForroLevelRepository repository, 
            IAmazonS3 amazonS3,
            ILoggerManager loggerManager,
            string bucketName, 
            string regionString)
        {
            _repository = repository;
            _amazonS3 = amazonS3;
            _loggerManager = loggerManager;
            _bucketName = bucketName;
            _bucketFullUrl = $"https://s3.{regionString}.amazonaws.com/{_bucketName}/";
        }
        public async Task<IList<ForroLevel>> GetAll()
        {
            _loggerManager.LogInfo("Method GetAll() was called");

            var result = await _repository.GetAll();

            foreach(var forroLevel in result)
            {
                if (!string.IsNullOrWhiteSpace(forroLevel.ImageUrl))
                    forroLevel.ImageUrl = _bucketFullUrl + forroLevel.ImageUrl;
            }
            
            return result;
        }

        public void Insert(ForroLevel forroLevel)
        {
            this.Insert(forroLevel, null);
        }

        public void Insert(ForroLevel forroLevel, Stream fileLogoStream)
        {
            //If there is a file being uploaded
            if (fileLogoStream != null)
            {
                var objectKey = GetNewObjectKey(forroLevel);

                //var stream = new FileStream();
                var request = new PutObjectRequest()
                {
                    Key = objectKey,
                    BucketName = _bucketName,
                    InputStream = fileLogoStream
                };

                var result = _amazonS3.PutObjectAsync(request).Result;
                forroLevel.ImageUrl = objectKey;
            }
            _repository.Insert(forroLevel);
        }

        public void Delete(int id)
        {
            var forroLevel = _repository.Get(id);
            _repository.Delete(id);

            //Delete file from S3 Bucket
            //Run synchronously. Not ideal I know, but my goal is to be straightforward and learn AWS, so this is faster development 
            //TODO: This logic is seriously fragile, I would never approve it during a code-review lol

            if(!string.IsNullOrWhiteSpace(forroLevel.ImageUrl))
            {
                _amazonS3.DeleteAsync(_bucketName, forroLevel.ImageUrl, null).Wait();
            }
        }

        private string GetNewObjectKey(ForroLevel forroLevel)
        {
            var result = Regex.Replace(forroLevel.Name.Trim(), "[^a-zA-Z0-9_.]+", "-", RegexOptions.Compiled);
            result = forroLevel.ForroLevelId + "-" + result.ToLower();
            result = _forroLevelFolder + result;

            //Add extension
            result = result + Path.GetExtension(forroLevel.ImageUrl);
            return result;
        }
    }
}