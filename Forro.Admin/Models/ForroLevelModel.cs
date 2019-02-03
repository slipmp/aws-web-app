using Forro.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forro.Admin.Models
{
    public class ForroLevelModel
    {
        public IEnumerable<ForroLevel> ForroLevelList { get; set; }
        public ForroLevel ForroLevelDomain { get; set; }
        public string ErrorMessage;
    }
}
