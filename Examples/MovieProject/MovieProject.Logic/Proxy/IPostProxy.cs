using MovieProject.Logic.Proxy.DTO;
using Refit;
using System.Threading.Tasks;

namespace MovieProject.Logic.Proxy
{
    public interface IPostProxy
    {
        [Post("/post")]
        Task<SavedPost> CreatePost(Post request);
    }
}
