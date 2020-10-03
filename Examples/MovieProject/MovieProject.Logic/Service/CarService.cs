using MovieProject.Logic.DTO;
using MovieProject.Logic.Proxy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieProject.Logic
{
    public interface ICarService
    {
        Task Add(Car car);
        Task<List<Car>> List();
    }

    public class CarService : ICarService
    {
        private readonly ICarThriftProxy _carThriftProxy;

        public CarService(ICarThriftProxy userProxy)
        {
            this._carThriftProxy = userProxy;
        }

        public Task Add(Car car)
        {
            return _carThriftProxy.Add(car);
        }

        public Task<List<Car>> List()
        {
            return _carThriftProxy.List();
        }
    }
}
