using Shouldly;
using System.Text;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (others)")]
    public class JsonHelperTests
    {
        [Fact]
        public void FormatJsonTests()
        {
            string jsonString = @"{""status"":""OK"", ""results"":[ {""types"":[ ""locality"", ""political""], ""formatted_address"":""New York, NY, USA"", ""address_components"":[ {""long_name"":""New York"", ""short_name"":""New York"", ""types"":[ ""locality"", ""political""]}, {""long_name"":""New York"", ""short_name"":""New York"", ""types"":[ ""administrative_area_level_2"", ""political""]}, {""long_name"":""New York"", ""short_name"":""NY"", ""types"":[ ""administrative_area_level_1"", ""political""]}, {""long_name"":""United States"", ""short_name"":""US"", ""types"":[ ""country"", ""political""]}], ""geometry"":{""location"":{""lat"":40.7143528, ""lng"":-74.0059731}, ""location_type"":""APPROXIMATE"", ""viewport"":{""southwest"":{""lat"":40.5788964, ""lng"":-74.2620919}, ""northeast"":{""lat"":40.8495342, ""lng"":-73.7498543}}, ""bounds"":{""southwest"":{""lat"":40.4773990, ""lng"":-74.2590900}, ""northeast"":{""lat"":40.9175770, ""lng"":-73.7002720}}}}]}";

            var prettyJson = jsonString.FormatJson();

            prettyJson.ShouldBe(@"{
    ""status"": ""OK"",
     ""results"": [
         {
            ""types"": [
                 ""locality"",
                 ""political""
            ],
             ""formatted_address"": ""New York, NY, USA"",
             ""address_components"": [
                 {
                    ""long_name"": ""New York"",
                     ""short_name"": ""New York"",
                     ""types"": [
                         ""locality"",
                         ""political""
                    ]
                },
                 {
                    ""long_name"": ""New York"",
                     ""short_name"": ""New York"",
                     ""types"": [
                         ""administrative_area_level_2"",
                         ""political""
                    ]
                },
                 {
                    ""long_name"": ""New York"",
                     ""short_name"": ""NY"",
                     ""types"": [
                         ""administrative_area_level_1"",
                         ""political""
                    ]
                },
                 {
                    ""long_name"": ""United States"",
                     ""short_name"": ""US"",
                     ""types"": [
                         ""country"",
                         ""political""
                    ]
                }
            ],
             ""geometry"": {
                ""location"": {
                    ""lat"": 40.7143528,
                     ""lng"": -74.0059731
                },
                 ""location_type"": ""APPROXIMATE"",
                 ""viewport"": {
                    ""southwest"": {
                        ""lat"": 40.5788964,
                         ""lng"": -74.2620919
                    },
                     ""northeast"": {
                        ""lat"": 40.8495342,
                         ""lng"": -73.7498543
                    }
                },
                 ""bounds"": {
                    ""southwest"": {
                        ""lat"": 40.4773990,
                         ""lng"": -74.2590900
                    },
                     ""northeast"": {
                        ""lat"": 40.9175770,
                         ""lng"": -73.7002720
                    }
                }
            }
        }
    ]
}");
        }
    }
}
