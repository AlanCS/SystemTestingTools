using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieProject.Logic.Proxy;
using MovieProject.Logic.Proxy.DTO;

namespace MovieProject.Logic.Service
{
    public interface IUserService
    {
        Task<List<string>> GetUsers();
        Task<List<string>> GetSearchUsers(UserSearchModel searchModel);
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

        public async Task<List<string>> GetSearchUsers(UserSearchModel searchModel)
        {
            var users = await _userProxy.GetSearchUsers(searchModel);

            return users.Select(c=> c.Name).OrderBy(c=> c).ToList();
        }
    }
}
