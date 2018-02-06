using CoreEntities.CustomModels;
using CoreEntities.enums;
using CoreEntities.Helper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Contacts;
using Google.GData.Client;
using RestSharp;
using ServiceLayer.Interfaces;
using System;
using System.IO;

namespace ServiceLayer.Services
{
    public class GoogleAuthentication : IAuthentication
    {
        /// <summary>
        /// It will request very first to get access and refresh token by passing authorize code
        /// For next login with same user will only provide access token
        /// </summary>
        /// <param name="authorizedCode"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        public object GetAccessTokenByAuthCode(string authorizedCode, RequestType reqType)
        {
            RestRequest restRequest = new RestRequest("", Method.POST);

            restRequest.AddParameter(GoogleReqestParameters.code.ToString(), authorizedCode);
            restRequest.AddParameter(GoogleReqestParameters.grant_type.ToString(), GoogleReqestParameters.authorization_code.ToString());

            return GoogleExecuteRequest(restRequest, reqType);
        }

        /// <summary>
        /// Common method to execute google request with paramentes.
        /// </summary>
        /// <param name="restRequest"></param>
        /// <param name="reqType"></param>
        /// <returns></returns>
        private GoogleValidationResponse GoogleExecuteRequest(RestRequest restRequest, RequestType reqType)
        {
            RestClient restClient = new RestClient(GoogleParameters.GoogleValidateV4);

            switch (reqType)
            {
                case RequestType.mobile:
                    restRequest.AddParameter(GoogleReqestParameters.redirect_uri.ToString(), GeneralParameters.RedirectUrlMobile);
                    break;

                case RequestType.web:
                    restRequest.AddParameter(GoogleReqestParameters.redirect_uri.ToString(), GeneralParameters.RedirectUrlWeb);
                    break;

                case RequestType.extension:
                    restRequest.AddParameter(GoogleReqestParameters.redirect_uri.ToString(), GeneralParameters.RedirectUrlExtension);
                    break;

                case RequestType.application:
                    restRequest.AddParameter(GoogleReqestParameters.redirect_uri.ToString(), GeneralParameters.RedirectUrlApplication);
                    break;

                default:
                    restRequest.AddParameter(GoogleReqestParameters.redirect_uri.ToString(), GeneralParameters.RedirectUrlApplication);
                    break;
            }

            restRequest.AddParameter(GoogleReqestParameters.client_id.ToString(), GoogleParameters.ClientId);
            restRequest.AddParameter(GoogleReqestParameters.client_secret.ToString(), GoogleParameters.ClientSecret);
            restRequest.AddParameter(GoogleReqestParameters.scope.ToString(), GoogleParameters.Scope);

            var gValidationResponse = restClient.Execute<GoogleValidationResponse>(restRequest);

            return gValidationResponse.Data;
        }

        public object GetAccessTokenByRefreshToken(string refreshToken, RequestType reqType)
        {
            var restRequest = new RestRequest("", Method.POST);

            restRequest.AddParameter(GoogleReqestParameters.refresh_token.ToString(), refreshToken);
            restRequest.AddParameter(GoogleReqestParameters.grant_type.ToString(), GoogleReqestParameters.refresh_token.ToString());

            return GoogleExecuteRequest(restRequest, reqType);
        }

        /// <summary>
        /// To get user information on the basis of access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public object GetUserProfile(string accessToken)
        {
            RestClient restClient = new RestClient(GoogleParameters.UserInfoV1 + "&" + GoogleReqestParameters.access_token.ToString() + "=" + accessToken);

            var request = new RestRequest("", Method.GET) { RequestFormat = DataFormat.Json };
            var userInfoV1 = restClient.Execute<UserDetail>(request);

            return userInfoV1.Data;
        }

        /// <summary>
        /// Token request object initilization to retreive user information
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public object GetTokenInfo(string accessToken)
        {
            Oauth2Service.TokeninfoRequest tokeninfoRequest = new Oauth2Service().Tokeninfo();
            Google.Apis.Discovery.Parameter accessType = new Google.Apis.Discovery.Parameter { DefaultValue = GoogleReqestParameters.offline.ToString(), Name = GoogleReqestParameters.access_type.ToString() };
            tokeninfoRequest.RequestParameters.Add("AccessType", accessType);
            tokeninfoRequest.AccessToken = accessToken;
            Tokeninfo tokeninfo = tokeninfoRequest.Execute();
            return tokeninfo;
        }

        /// <summary>
        /// Is token expired or not
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public bool IsTokenExpired(string accessToken)
        {
            Tokeninfo tokeninfo = null;
            Oauth2Service.TokeninfoRequest tokeninfoRequest = new Oauth2Service().Tokeninfo();
            Google.Apis.Discovery.Parameter accessType = new Google.Apis.Discovery.Parameter { DefaultValue = GoogleReqestParameters.offline.ToString(), Name = GoogleReqestParameters.access_type.ToString() };
            tokeninfoRequest.RequestParameters.Add("AccessType", accessType);
            tokeninfoRequest.AccessToken = accessToken;
            try
            {
                tokeninfo = tokeninfoRequest.Execute();
            }
            catch (Exception)
            {
                // ignored
            }
            return tokeninfo == null;
        }

        /// <summary>
        /// Create request object with of google with required parameters.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <param name="scope"></param>
        /// <param name="expiresIn"></param>
        /// <returns></returns>
        public object CreateAutherizationServiceObject(string scope, int? expiresIn, string userId, string accessToken, string refreshToken)
        {
            UserCredential userCredential =(UserCredential) CreateAutherizationObject(scope, expiresIn, userId, accessToken, refreshToken);
            
            //Create gmail service object to request google api services
            GmailService gs = new GmailService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApplicationName = GoogleParameters.ClientId,
                HttpClientInitializer = userCredential
            });
            return gs;
        }

        /// <summary>
        /// Create google credentail object to help in request.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <param name="scope"></param>
        /// <param name="expiresIn"></param>
        /// <returns></returns>
        public object CreateAutherizationObject(string scope, int? expiresIn, string userId, string accessToken, string refreshToken)
        {
            //Flow to create credentials
            IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = GoogleGetClientConfiguration().Secrets,
                Scopes = scope.Split(' '),
                Clock = Google.Apis.Util.SystemClock.Default
            });

            //Credentails to initilize gmail service object
            UserCredential credential = new UserCredential(flow, userId, new TokenResponse()
            {
                AccessToken = accessToken,
                Scope = scope,
                ExpiresInSeconds = expiresIn,
                Issued = DateTime.Now.AddSeconds(Convert.ToDouble(expiresIn)),
                TokenType = "Bearer",
                RefreshToken = refreshToken
            });
            return credential;
        }

        /// <summary>
        /// Get client secrets provided by google api on app registration
        /// </summary>
        /// <returns></returns>
        public GoogleClientSecrets GoogleGetClientConfiguration()
        {
            //For Local
            //using (var stream= GenerateStreamFromString("{'web':{'client_id':'948838070491 - 91j41t1nilsuh9t759ivuer7tq19o94u.apps.googleusercontent.com','project_id':'emailtest - 169408','auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://accounts.google.com/o/oauth2/token','auth_provider_x509_cert_url':'https://www.googleapis.com/oauth2/v1/certs','client_secret':'OiRWLLB__hiYuvAGvT1hIImE','redirect_uris':['http://localhost:55555/test/index.html'],'javascript_origins':['http://localhost:55555']}}"))
            //ends////////
            //For 108
            //using (var stream = GenerateStreamFromString("{'web':{'client_id':'948838070491 - 6s1lve2m63m2ekmjj7np8tq9tfog1gjb.apps.googleusercontent.com','project_id':'emailtest - 169408','auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://accounts.google.com/o/oauth2/token','auth_provider_x509_cert_url':'https://www.googleapis.com/oauth2/v1/certs','client_secret':'bXyhvaiTyGWyO_yHuxhUH1u4','redirect_uris':['http://stagingwin.com/lead411/test/index.html'],'javascript_origins':['http://stagingwin.com']}}"))
            //ends////////
            //For Azure
           using (var stream = GenerateStreamFromString("{'web':{'client_id':'754756613373-8t8dhl75gc00qr4861loku9qq24o0crr.apps.googleusercontent.com','project_id':'lead411-staging','auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://accounts.google.com/o/oauth2/token','auth_provider_x509_cert_url':'https://www.googleapis.com/oauth2/v1/certs','client_secret':'RYtQnc2jBZkeiU9HU-AZAf_X','redirect_uris':['http://lead411chromeex.azurewebsites.net/test/index.html','https://lead411chromeex.azurewebsites.net/test/index.html'],'javascript_origins':['http://lead411chromeex.azurewebsites.net','https://lead411chromeex.azurewebsites.net']}}"))
            //ends//////////
            //For Azure Staging
            //using (var stream = GenerateStreamFromString("{'web':{'client_id':'754756613373-fm0nhitf82m368pm6953et6g6phrld6r.apps.googleusercontent.com','project_id':'lead411-staging','auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://accounts.google.com/o/oauth2/token','auth_provider_x509_cert_url':'https://www.googleapis.com/oauth2/v1/certs','client_secret':'1BV-RnRkWiJgLZUHj6RPiwCa','redirect_uris':['http://lead411testapp.azurewebsites.net/test/index.html','https://lead411testapp.azurewebsites.net/test/index.html'],'javascript_origins':['http://lead411testapp.azurewebsites.net','https://lead411testapp.azurewebsites.net']}}"))
            //ends//////////
            {
                return GoogleClientSecrets.Load(stream);
            }
        }


        /// <summary>
        /// Generic method to convert stream to string
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns></returns>
        public Stream GenerateStreamFromString(string dataString)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(dataString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Create contact request model for request
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public object ContactsRequest(string accessToken, string refreshToken)
        {
            OAuth2Parameters parameters = new OAuth2Parameters
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Scope = GoogleParameters.Scope,
                TokenType = "Bearer",
                ClientId = GoogleParameters.ClientId,
                ClientSecret = GoogleParameters.ClientSecret,
                TokenExpiry = DateTime.Now.AddMinutes(30)
            };
            RequestSettings settings = new RequestSettings(GoogleParameters.ClientId, parameters);
            return new ContactsRequest(settings);
        }
    }
}
