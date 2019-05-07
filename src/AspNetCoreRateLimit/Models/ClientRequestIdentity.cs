namespace AspNetCoreRateLimit
{
    /// <summary>
    /// Stores the client IP, ID, endpoint and verb
    /// </summary>
    public class ClientRequestIdentity
    {
        public string ClientIp { get; set; }

        public string ClientId { get; set; }

        public long MPValue { get; set; }

        public string Path { get; set; }

        public string HttpVerb { get; set; }
    }
}