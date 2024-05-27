using MovieProject.Logic.Proxy.DTO;

namespace MovieProject.Logic.Extensions
{
    public static class UserMapperExtensions
    {
        public static UserSearchModel MapUserSearchModelDtoToProxyDto(
            this DTO.MovieProject.Logic.Proxy.DTO.UserSearchModel searchModel)
        {
            return new UserSearchModel
            {
                Name = searchModel.Name,
                Username = searchModel.Username,
                Email = searchModel.Email,
                Phone = searchModel.Phone,
                Website = searchModel.Website
            };
        }
    }
}