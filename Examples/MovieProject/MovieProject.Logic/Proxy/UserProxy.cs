using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MovieProject.Logic.Proxy.DTO;

namespace MovieProject.Logic.Proxy
{
    public interface IUserProxy
    {
        Task<DTO.User[]> GetUsers();
        Task<DTO.User[]> GetSearchUsers(UserSearchModel searchModel);
    }

    public class UserProxy  : BaseProxy, IUserProxy
    {

        public UserProxy(HttpClient client,
                                  ILogger<UserProxy> logger) : base(client)
        {
        }        

        public async Task<DTO.User[]> GetUsers()
        {
            string route = "users";

            var result = await Send(HttpMethod.Get, route, (DTO.User[] users, HttpStatusCode status) =>
            {
                // any exceptions throwm in here will be wrapped inside a DownstreamException, will all the details of the request / response for investigation
                // also applies the concept of anti-corruption layer, meaning we parse the external DTO inside the proxy, and only return valid / clean internal DTOs

                if(status != HttpStatusCode.OK || users == null)
                    throw new Exception("Unexpected response");

                return users;
            });

            return result;
        }
        
        public async Task<DTO.User[]> GetSearchUsers(UserSearchModel searchModel)
        {
            string serialisedQueryParameter = JsonSerializer.Serialize(searchModel);
            string route = $"searchUsers?userSearchModel={serialisedQueryParameter}";

            var result = await Send(HttpMethod.Get, route, (DTO.User[] users, HttpStatusCode status) =>
            {
                if(status != HttpStatusCode.OK || users == null)
                    throw new Exception("Unexpected response");

                return users;
            });

            return result;
        }
    }
}
