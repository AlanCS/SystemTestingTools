using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using MovieProject.Logic.DTO;
using System.Threading.Tasks;

namespace MovieProject.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private ISearchService _searchService { get; }

        public SearchController(ISearchService searchService)
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
    }
}
