using CoreEntities.enums;

namespace ServiceLayer.Interfaces
{
    public interface IAuthentication
    {
        bool IsTokenExpired(string accessToken);
        object GetUserProfile(string accessToken);
        object GetAccessTokenByAuthCode(string authorizedCode, RequestType reqType);
        object GetAccessTokenByRefreshToken(string refreshToken, RequestType reqType);
        object GetTokenInfo(string accessToken);
        object CreateAutherizationServiceObject(string scope, int? expiresIn, string userId, string accessToken, string refreshToken);
        object CreateAutherizationObject(string scope, int? expiresIn, string userId, string accessToken, string refreshToken);
        object ContactsRequest(string accessToken, string refreshToken);
    }
}
