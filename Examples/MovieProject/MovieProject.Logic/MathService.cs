using MovieProject.Logic.Proxy;
using System.Threading.Tasks;

namespace MovieProject.Logic
{
    public interface IMathService
    {
        Task<int> Add(int firstNumber);
        Task<int> Minus(int firstNumber);
    }

    public class MathService : IMathService
    {
        private readonly IMathProxy _mathProxy;

        public MathService(IMathProxy mathProxy)
        {
            this._mathProxy = mathProxy;
        }

        public Task<int> Add(int firstNumber)
        {
            return _mathProxy.Add(firstNumber, 3);
        }

        public Task<int> Minus(int firstNumber)
        {
            return _mathProxy.Minus(firstNumber, 3);
        }
    }
}
