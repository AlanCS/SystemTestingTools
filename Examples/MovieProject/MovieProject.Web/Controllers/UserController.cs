using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
