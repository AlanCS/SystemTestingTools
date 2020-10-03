using Microsoft.Extensions.Logging;
using MovieProject.Logic.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieProject.Logic.Proxy
{
    public interface ICarThriftProxy
    {
        Task Add(Car car);
        Task<List<Car>> List();
    }

    public class CarThriftProxy : ICarThriftProxy
    {
        private readonly IThriftServiceForCarManagement thriftServiceForCarManagement;

        public CarThriftProxy(IThriftServiceForCarManagement thriftServiceForCarManagement, ILogger<CarThriftProxy> logger) 
        {
            this.thriftServiceForCarManagement = thriftServiceForCarManagement;
        }        

        public Task Add(Car car)
        {
            return thriftServiceForCarManagement.Add(car);
        }

        public Task<List<Car>> List()
        {
            return thriftServiceForCarManagement.List();
        }
    }

    public interface IThriftServiceForCarManagement
    {
        Task Add(Car car);
        Task<List<Car>> List();
    }

    /// <summary>
    ///  we pretend this is a class using a protocal non HTTP based, so we can't use the standard SystemTestingTools interception
    /// </summary>
    public class ThriftServiceForCarManagement : IThriftServiceForCarManagement
    {
        public Task Add(Car car)
        {
            return Task.FromResult(0); 
        }

        public Task<List<Car>> List()
        {
            return Task.FromResult(new List<Car>());
        }
    }
}
