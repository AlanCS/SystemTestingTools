using ExternalDependencies.Calculator;
using System.Threading.Tasks;

namespace MovieProject.Logic.Proxy
{
    public interface IMathProxy
    {
        Task<int> Add(int firstNumber, int secondNumber);
        Task<int> Minus(int firstNumber, int secondNumber);
    }

    public class MathProxy : IMathProxy
    {
        private readonly ICalculatorSoap client;

        public MathProxy(ICalculatorSoap client)
        {
            this.client = client;
        }        

        public Task<int> Add(int firstNumber, int secondNumber)
        {
            return client.AddAsync(firstNumber, secondNumber);
        }

        public Task<int> Minus(int firstNumber, int secondNumber)
        {
            return client.SubtractAsync(firstNumber, secondNumber);
        }
    }
}
