using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using MovieProject.Logic.DTO;
using System.Threading.Tasks;

namespace MovieProject.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private ISearchService _searchService { get; }

        public MediaController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("{type}/{name}")]
        public async Task<ActionResult<Logic.DTO.Media>> Get(MediaType type, string name)
        {
            var media = await _searchService.GetMovieOrTvSeries(type, name);

            if (media == null) return NotFound($"Search terms didn't match any {type}");

            return Ok(media);
        }

        [HttpPost("{type}")]
        public async Task<ActionResult<Logic.DTO.Media>> AddToResearchQueue(MediaType type, [FromQuery]string imdb)
        {
            // fake method that doesn't exist downstream, we pretend to add a new movie / tv series
            // to a queue to be researched later

            await _searchService.AddToResearchQueue(type, imdb);

            return Ok();
        }
    }
}
