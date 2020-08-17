using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;

namespace MovieProject.Web.Controllers
{
    [Route("api/math")]
    [ApiController]
    public class MathController : Controller
    {
        private IMathService _mathService { get; }

        public MathController(IMathService mathService)
        {
            _mathService = mathService;
        }

        [HttpGet("add")]
        public async Task<int> Add(int firstNumber)
        {
            return await _mathService.Add(firstNumber);
        }

        [HttpGet("minus")]
        public async Task<int> Minus(int firstNumber)
        {
            return await _mathService.Minus(firstNumber);
        }
    }
}