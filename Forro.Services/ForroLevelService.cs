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
        public ForroLevelService(IForroLevelRepository repository)
        {
            _repository = repository;
        }
        public IList<ForroLevel> GetAll()
        {
            return _repository.GetAll();
        }
        public void Insert(ForroLevel forroLevel)
        {
            _repository.Insert(forroLevel);
        }
        public void Delete(int id)
        {
            var forroLevel = _repository.Get(id);
            _repository.Delete(id);
        }
    }
}