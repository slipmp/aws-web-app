using Amazon.S3;
using Amazon.S3.Model;
using Forro.Data;
using Forro.Domain;
using System;
using System.Collections.Generic;

namespace Forro.Services
{
    public interface IForroLevelService
    {
        IList<ForroLevel> GetAll();
        void Insert(ForroLevel forroLevel);
        void Delete(int id);
    }

    public class ForroLevelService : IForroLevelService
    {
        private IForroLevelRepository _repository;
        private IAmazonS3 _amazonS3;
        private const string _bucketName="paulo-bucket";
        private const string _forroLevelFolder = "level/";

        public ForroLevelService(IForroLevelRepository repository, IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
            _repository = repository;
        }
        public IList<ForroLevel> GetAll()
        {
            return _repository.GetAll();
        }
        public void Insert(ForroLevel forroLevel)
        {
            _repository.Insert(forroLevel);

            //var request = new PutObjectRequest()
            //{
            //    Key=
            //};
            //_amazonS3.PutObjectAsync()
        }
        public void Delete(int id)
        {
            var forroLevel = _repository.Get(id);
            _repository.Delete(id);

            //Delete file from S3 Bucket
            //Run synchronously. Not ideal I know, but my goal is to be straightforward and learn AWS, so this is faster development 
            //TODO: This logic is seriously fragile, I would never approve it during a code-review lol
            var objectKey = _forroLevelFolder + GetFileName(forroLevel.ImageUrl);

            _amazonS3.DeleteAsync(_bucketName, objectKey, null).Wait();
        }

        /// <summary>
        /// Ideally we would save just relative path
        /// Think about it, you may have different environments which points to different S3 buckets
        /// Having relative path would be easier to use a different domain and reuse the same data
        /// But again, my purpose is not have perfect code, rather it is focus on AWS consumption
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetFileName(string url)
        {
            Uri uri = new Uri(url);
            var fileName = System.IO.Path.GetFileName(uri.LocalPath);
            return fileName;
        }
    }
}