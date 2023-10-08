using Microsoft.AspNetCore.Mvc;
using bc_schools_api.Services.Interfaces;
using bc_schools_api.Domain.Models.Entities;

namespace bc_schools_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService _schoolService;

        public SchoolController(ISchoolService schoolService) => _schoolService = schoolService;

        /// <summary>
        /// Search for nearest schools based on typed coordinates
        /// </summary>
        /// <param name="originCoordinate">Origin coordinate</param>
        /// <response code="200">Returns the list of schools sorted by distance</response>
        /// <response code="204">If no content</response>   
        /// <response code="400">If throws an error</response> 
        [HttpPost("GetSchoolsList")]
        public async Task<ActionResult<IEnumerable<School>>> GetSchoolsList([FromBody] Coordinate originCoordinate, int limitRange = 30)
        {
            try
            {
                var schools = await _schoolService.GetSchoolsList(originCoordinate, limitRange);

                if (schools.Count == 0)
                    return NoContent();

                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
