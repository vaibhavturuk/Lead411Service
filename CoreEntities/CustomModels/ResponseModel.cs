namespace CoreEntities.CustomModels
{
    public class ResponseModel
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string AccessToken { get; set; }

        public object Content { get; set; }
        
    }
}
