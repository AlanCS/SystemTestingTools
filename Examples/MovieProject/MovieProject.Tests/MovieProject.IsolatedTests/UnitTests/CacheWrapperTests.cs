using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MovieProject.Logic;
using MovieProject.Logic.Option;

using System;
using System.Threading.Tasks;
using Xunit;

namespace IsolatedTests.UnitTests
{
    [Trait("Project", "MovieProject Unit Tests")]
    public class CacheWrapperTests
    {
        private readonly IMemoryCache memoryCache;

        public CacheWrapperTests()
        {
            // create a real memory cache
            var services = new ServiceCollection();
            services.AddMemoryCache();
            memoryCache = services.BuildServiceProvider().GetService<IMemoryCache>();
        }

        [Fact]
        public async Task GetOrSetFromCache_Caches_Properly()
        {
            var sut = new CacheWrapper(memoryCache, Options.Create(new Caching() { movieApiInSeconds = 10 }));

            int count = 0;

            Func<Task<int>> func = () => { return Task.FromResult(++count); };

            (await sut.GetOrSetFromCache("abc", func)).Should().Be(1);
            (await sut.GetOrSetFromCache("abc", func)).Should().Be(1);
            (await sut.GetOrSetFromCache("abc", func)).Should().Be(1);

            (await sut.GetOrSetFromCache("def", func)).Should().Be(2);
        }

        [Fact]
        public async Task GetOrSetFromCache_Doesnt_Cache_When_Required()
        {
            var sut = new CacheWrapper(memoryCache, Options.Create(new Caching() { movieApiInSeconds = -1 }));

            int count = 0;

            Func<Task<int>> func = () => { return Task.FromResult(++count); };

            (await sut.GetOrSetFromCache("abc", func)).Should().Be(1);
            (await sut.GetOrSetFromCache("abc", func)).Should().Be(2);
            (await sut.GetOrSetFromCache("abc", func)).Should().Be(3);

            (await sut.GetOrSetFromCache("def", func)).Should().Be(4);
        }
    }
}
