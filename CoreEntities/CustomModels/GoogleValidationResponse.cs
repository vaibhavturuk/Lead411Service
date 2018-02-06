namespace CoreEntities.CustomModels
{
    /// <summary>
    /// We have kept response field as same as google response
    /// </summary>
    public class GoogleValidationResponse
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public long expires_in { get; set; }

        public string refresh_token { get; set; }

        public string id_token { get; set; }
    }
}
