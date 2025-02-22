using System.Text.Json.Serialization;

namespace URLUtil
{
    [JsonSerializable(typeof(UriInfo))]
    internal partial class UriInfoJsonContext : JsonSerializerContext 
    {
    }

    class UriInfo
    {
        public string FullUrl { get; set; } = "";
        public string Scheme { get; set; } = "";
        public string Host { get; set; } = "";
        public string Port { get; set; } = "";
        public string Path { get; set; } = "";
        public string Fragment { get; set; } = "";
        public Dictionary<string, string> Queries { get; set; } = [];
    }
}
