using MovieProject.Logic.Exceptions;
using MovieProject.Logic.Proxy;
using System.Threading.Tasks;

namespace MovieProject.Logic
{
    public interface ISearchService
    {
        Task<DTO.Media> GetMovieOrTvSeries(DTO.MediaType type, string movieName);
    }

    public class SearchService : ISearchService
    {
        private readonly IMovieDatabaseProxy _movieDatabaseProxy;
        private readonly ICacheWrapper _cacheWrapper;

        public SearchService(IMovieDatabaseProxy movieDatabaseProxy, ICacheWrapper cacheWrapper)
        {
            this._movieDatabaseProxy = movieDatabaseProxy;
            this._cacheWrapper = cacheWrapper;
        }

        public async Task<DTO.Media> GetMovieOrTvSeries(DTO.MediaType type, string name)
        {
            if (name == null) throw new BadRequestException("name is empty", name);
            name = name.Trim();
            if (name.Length < 2) throw new BadRequestException("name has too few characters", name);

            string cacheKey = $"{type}_{name.ToLower().Replace(" ","")}";

            var result = await _cacheWrapper.GetOrSetFromCache(cacheKey, async () =>
                {
                    return await _movieDatabaseProxy.GetMovieOrTvSeries(type.ToString(), name);
                });

            return result;
        }
    }
}
