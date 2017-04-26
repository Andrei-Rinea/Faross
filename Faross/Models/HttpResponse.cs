using System;
using System.Collections.Generic;
using System.Linq;
using Faross.Util;

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
            ResponseHeaders = responseHeaders ?? throw new ArgumentNullException(nameof(responseHeaders));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Status = status;

            if (Status < 100 || Status > 599) throw new ArgumentOutOfRangeException(nameof(status), Status, "");
        }

        public IReadOnlyCollection<KeyValuePair<string, string>> ResponseHeaders { get; }
        public string ContentType { get; }
        public byte[] Content { get; }
        public int Status { get; }

        public override bool Equals(object obj)
        {
            var other = obj as HttpResponse;
            return other != null &&
                   other.Status == Status &&
                   other.Content.ArraysEqual(Content) &&
                   other.ContentType == ContentType &&
                   other.ResponseHeaders.Equivalent(ResponseHeaders);
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(ResponseHeaders, ContentType, Content, Status);
        }
    }
}