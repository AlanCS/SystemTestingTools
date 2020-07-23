using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (others)")]
    public class SystemTestingLoggerProviderTests
    {
        [Theory]
        [InlineData("MyFancyApp.Startup", true)]
        [InlineData("MyFancyApp.Whatever.Something", true)]
        [InlineData("MyFancyApp.NotSoInteresting", false)]
        public void CanStoreLogsFromNamespaceWithIncludeAndExclude(string fullNamespace, bool canLog)
        {
            var logger = new SystemTestingLoggerProvider(LogLevel.Critical, new[] { "MyFancyApp" }, new[] { "MyFancyApp.NotSoInteresting" });

            var result = logger.CreateLogger(fullNamespace);

            result.IsEnabled(LogLevel.Critical).ShouldBe(canLog);
        }

        [Theory]
        [InlineData("MyFancyApp.Startup", false)]
        [InlineData("MyFancyApp.SpecificArea1.Something", true)]
        [InlineData("MyFancyApp.SpecificArea2.Something", true)]
        public void CanStoreLogsFromNamespaceWithIncludeAndNoExclude(string fullNamespace, bool canLog)
        {
            var logger = new SystemTestingLoggerProvider(LogLevel.Critical, new[] { "MyFancyApp.SpecificArea1", "MyFancyApp.SpecificArea2" });

            var result = logger.CreateLogger(fullNamespace);

            result.IsEnabled(LogLevel.Critical).ShouldBe(canLog);
        }

        [Theory]
        [InlineData("MyFancyApp.Startup", true)]
        [InlineData("Microsoft.Owin.Whatever", true)]
        [InlineData("MyFancyApp.SpecificArea1", false)]
        [InlineData("MyFancyApp.SpecificArea1.Something", false)]
        [InlineData("MyFancyApp.SpecificArea2.Something", false)]
        public void CanStoreLogsFromNamespaceWithNoIncludeAndWithExclude(string fullNamespace, bool canLog)
        {
            var logger = new SystemTestingLoggerProvider(LogLevel.Critical, namespaceToExcludeStart: new[] { "MyFancyApp.SpecificArea1", "MyFancyApp.SpecificArea2" });

            var result = logger.CreateLogger(fullNamespace);

            result.IsEnabled(LogLevel.Critical).ShouldBe(canLog);
        }

        [Fact]
        public void CanStoreLogsFromNamespaceWithNoIncludeOrExclude()
        {
            var logger = new SystemTestingLoggerProvider(LogLevel.Critical);

            var result = logger.CreateLogger("MyFancyApp");

            result.IsEnabled(LogLevel.Critical).ShouldBe(true);
        }
    }
}
