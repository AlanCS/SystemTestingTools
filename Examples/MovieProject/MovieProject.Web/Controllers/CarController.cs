using Microsoft.AspNetCore.Mvc;
using MovieProject.Logic;
using MovieProject.Logic.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieProject.Web.Controllers
{
    [Route("api/cars")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private ICarService _carService { get; }

        public CarController(ICarService userService)
        {
            _carService = userService;
        }

        [HttpPut]
        public async Task Add(Car car)
        {
            await _carService.Add(car);
        }

        [HttpGet]
        public async Task<List<Car>> List()
        {
            return await _carService.List();
        }
    }
}
