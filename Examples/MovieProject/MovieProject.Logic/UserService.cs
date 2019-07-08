using MovieProject.Logic.Proxy;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProject.Logic
{
    public interface IUserService
    {
        Task<List<string>> GetUsers();
    }

    public class UserService : IUserService
    {
        private readonly IUserProxy _userProxy;

        public UserService(IUserProxy userProxy)
        {
            this._userProxy = userProxy;
        }

        public async Task<List<string>> GetUsers()
        {
            var users = await _userProxy.GetUsers();

            return users.Select(c=> c.Name).OrderBy(c=> c).ToList();
        }
    }
}
