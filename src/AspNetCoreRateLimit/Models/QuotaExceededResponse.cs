namespace AspNetCoreRateLimit
{
    public class QuotaExceededResponse
    {
        public string ContentType { get; set; }

        public string Content { get; set; }

        public int? StatusCode { get; set; }

        public string Url { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public ResponseType ResponseType { get; set; }

        public QuotaExceededResponse()
        {
            StatusCode = 429;
            ResponseType = ResponseType.WriteContent;
        }
    }

    public enum ResponseType
    {
        WriteContent,
        RedirectToAction,
        RedirectToUrl
    }
}
