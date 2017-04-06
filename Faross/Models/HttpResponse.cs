using System.Collections.Generic;

namespace Faross.Models
{
    public class HttpResponse
    {
        public HttpResponse(
            IReadOnlyCollection<KeyValuePair<string, string>> responseHeaders,
            string contentType,
            byte[] content,
            int status)
        {
            ResponseHeaders = responseHeaders;
            ContentType = contentType;
            Content = content;
            Status = status;
        }

        public IReadOnlyCollection<KeyValuePair<string, string>> ResponseHeaders { get; }
        public string ContentType { get; }
        public byte[] Content { get; }
        public int Status { get; }
    }
}