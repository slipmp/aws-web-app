using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Forro.Admin.Models;
using Forro.Data;
using Forro.Domain;
using Forro.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

            var s3Client = new AmazonS3Client(RegionEndpoint.USEast2);
            _forroLevelService = new ForroLevelService(forroLevelRepository, s3Client);
        }

        // GET: api/ForroLevel
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var forroLevelList = _forroLevelService.GetAll();

                forroLevelList = forroLevelList.OrderBy(x => x.ForroLevelId).ToList();

                return Ok(new ForroLevelModel()
                {
                    ForroLevelList = forroLevelList,
                    ErrorMessage = ""
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new ForroLevelModel()
                {
                    ErrorMessage = ex.ToString()
                });
            }
        }

        // GET: api/ForroLevel/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ForroLevel
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            var result = await Request.ReadFormAsync();

            var json = result["forroLevel"];
            var model = JsonConvert.DeserializeObject<ForroLevel>(json);

            Stream stream = null;
            if (result.Files.Count > 0)
            {
                //Getting only the first file, because as of know the UI is not uploading additional  files
                stream = result.Files[0].OpenReadStream();
                model.ImageUrl = result.Files[0].FileName;
            }

            _forroLevelService.Insert(model, stream);

            return Ok();
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
