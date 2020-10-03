using Microsoft.AspNetCore.Http;
using MovieProject.Logic.DTO;
using MovieProject.Logic.Proxy;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemTestingTools;

namespace MovieProject.IsolatedTests.ComponentTesting.Mock
{
    public class MockThriftServiceForCarManagement : IThriftServiceForCarManagement
    {
        private readonly IHttpContextAccessor context;
        private List<Car> list = new List<Car>();

        public MockThriftServiceForCarManagement(IHttpContextAccessor context)
        {
            this.context = context;
        }

        public Task Add(Car car)
        {
            list.Add(car);

            context.AddSessionEvent($"new car added: {car.Model}");

            return Task.FromResult(0);            
        }

        public Task<List<Car>> List()
        {
            return Task.FromResult(list);
        }
    }
}
