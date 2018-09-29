using Forro.Domain;
using System.Collections.Generic;

namespace Forro.Data
{
    public interface IForroLevelRepository
    {
        IList<ForroLevel> GetAll();
    }

    public class ForroLevelRepository : IForroLevelRepository
    {
        public IList<ForroLevel> GetAll()
        {
            var result = new List<ForroLevel>
            {
                new ForroLevel{ Name="Beginner"},
                new ForroLevel{ Name="Intermediate"}
            };

            return result;
        }
    }
}