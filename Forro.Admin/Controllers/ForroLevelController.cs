using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Forro.Data;
using Forro.Domain;
using Forro.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forro.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForroLevelController : ControllerBase
    {
        private readonly ForroLevelService _forroLevelService;

        public ForroLevelController()
        {
            var config = new AmazonDynamoDBConfig();
            config.RegionEndpoint = RegionEndpoint.USEast2;
            var client = new AmazonDynamoDBClient(config);

            var forroLevelRepository = new ForroLevelRepository(client);
            _forroLevelService = new ForroLevelService(forroLevelRepository);
        }

        // GET: api/ForroLevel
        [HttpGet]
        public IEnumerable<ForroLevel> Get()
        {
            var result = _forroLevelService.GetAll();

            return result.OrderBy(x => x.ForroLevelId);
        }

        // GET: api/ForroLevel/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ForroLevel
        [HttpPost]
        public void Post([FromBody] ForroLevel value)
        {
            _forroLevelService.Insert(value);
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
            _forroLevelService.Delete(id);
        }
    }
}
