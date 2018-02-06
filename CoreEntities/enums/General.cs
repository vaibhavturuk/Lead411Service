namespace CoreEntities.enums
{
    public enum ExceptionTypeEnum
    {
        Exception = 0,
        LogoutWithAjaxRequest = 1
    }

    public enum UserLoginMessage
    {
        NotExits = 0,
        InActive = 1,
        IsDeleted = 2,
        ValidUser = 3
    }

    public enum AccountType
    {
        Gmail = 0,
        Office365 = 1
    }

    public enum ProcessStatus
    {
        Success = 0,
        Failed = 1
    }

    public enum GoogleReqestParameters
    {
        code, redirect_uri, client_id, client_secret, grant_type, scope, authorization_code, refresh_token, offline, access_type, access_token
    }

    public enum Office365ReqestParameters
    {
        code, redirect_uri, client_id, client_secret, grant_type, scope, authorization_code, refresh_token, offline, access_type, access_token
    }

    public enum RequestType
    {
        mobile,web,extension,application
    }

    public enum SettingType
    {
        resetindexing, deleteaccount
    }
}
