using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieProject.Logic.DTO.MovieProject.Logic.Proxy.DTO;
using MovieProject.Logic.Extensions;
using MovieProject.Logic.Service;
using Newtonsoft.Json;

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

        [HttpGet("searchUsers")]
        public async Task<List<string>> GetSearchUsers(string jsonSearchModel)
        {
            var proxyDtoSearchModel = JsonConvert.DeserializeObject<Logic.Proxy.DTO.UserSearchModel>(jsonSearchModel);

            
            // second controller with separate dependency used for testing SystemTestingTools
            return await _userService.GetSearchUsers(proxyDtoSearchModel);
        }      
    }
}
