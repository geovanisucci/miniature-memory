namespace Miniature.ES.Sample.Host.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Miniature.ES.Sample.Host.Models;
    using Nest;

    /// <summary>
    /// Log Endpoints that makes ElasticSearch communications.
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]

    public class LogController : ControllerBase
    {
        /// <summary>
        /// Create a log in Elastic Search.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Log/
        ///     {}
        ///
        /// </remarks>
        /// <param name="log"></param>
        /// <param name="elasticClient"></param>
        /// <returns>A log created.</returns>
        /// <response code="201">A log created.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IGetResponse<Log>), 201)]
        public async Task<IActionResult> Post([FromBody] Log log, [FromServices]IElasticClient elasticClient)
        {

            await elasticClient.IndexDocumentAsync(log);
            var result = await elasticClient.GetAsync<Log>(log.Id);
            return Created("Post:Log", result);
        }
        /// <summary>
        /// Get all logs from Elastic Search
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Log/
        ///     {}
        ///
        /// </remarks>
        /// <param name="elasticClient"></param>     
        /// <returns>List of logs.</returns>
        /// <response code="200">List of logs.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<Log>), 200)]
        public async Task<IActionResult> Get([FromServices]IElasticClient elasticClient)
        {
            var result = await elasticClient.SearchAsync<Log>(s => s.AllIndices());
            return Ok(result.Documents);
        }

        /// <summary>
        /// Delete a log from Elastic Search.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/Log/4a1546aa-04c3-4f43-be26-9defe6750525
        ///     {}
        ///
        /// </remarks>
        /// <param name="elasticClient"></param>
        /// <param name="id"></param>
        /// <response code="200">Ok, deleted.</response>
        /// <response code="401">Log not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Delete(string id, [FromServices]IElasticClient elasticClient)
        {
            var result = await elasticClient.GetAsync<Log>(id);

            if (result.Source == null)
            {
                return NotFound();
            }

            var request = new DeleteByQueryRequest<Log>
            {
                Query = new QueryContainer(new TermQuery
                {
                    Field = "id",
                    Value = id

                })
            };

            await elasticClient.DeleteByQueryAsync(request);
            return Ok();
        }

    }
}