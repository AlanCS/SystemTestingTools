using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic.Proxy;
using MovieProject.Logic.Proxy.DTO;
using System.Threading.Tasks;

namespace MovieProject.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private IPostProxy _postProxy { get; }

        public PostController(IPostProxy postProxy)
        {
            _postProxy = postProxy;
        }

        [HttpPost("post")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<CreatedAtActionResult> CreatePost(Post post)
        {
            var result = await _postProxy.CreatePost(post);
            return CreatedAtAction(nameof(CreatePost), result); 
        }
    }
}
