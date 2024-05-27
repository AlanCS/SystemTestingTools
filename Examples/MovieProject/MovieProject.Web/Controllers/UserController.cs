using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieProject.Logic.DTO.MovieProject.Logic.Proxy.DTO;
using MovieProject.Logic.Extensions;
using MovieProject.Logic.Service;

namespace MovieProject.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService { get; }

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<List<string>> GetUsers()
        {
            // second controller with separate dependency used for testing SystemTestingTools
            return await _userService.GetUsers();
        }      

        [HttpPost("searchUsers")]
        public async Task<List<string>> GetSearchUsers([FromBody] UserSearchModel searchModel)
        {
            var proxyDtoSearchModel = searchModel.MapUserSearchModelDtoToProxyDto();
            // second controller with separate dependency used for testing SystemTestingTools
            return await _userService.GetSearchUsers(proxyDtoSearchModel);
        }      
    }
}
