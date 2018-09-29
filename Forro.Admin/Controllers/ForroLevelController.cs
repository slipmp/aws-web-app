using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forro.Data;
using Forro.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forro.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForroLevelController : ControllerBase
    {
        // GET: api/ForroLevel
        [HttpGet]
        public IEnumerable<ForroLevel> Get()
        {
            var forroRepository = new ForroLevelRepository();

            var result = forroRepository.GetAll();

            return result;
        }

        // GET: api/ForroLevel/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ForroLevel
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/ForroLevel/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
