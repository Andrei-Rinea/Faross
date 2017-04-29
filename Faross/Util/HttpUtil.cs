using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Faross.Util
{
    public static class HttpUtil
    {
        private class RequestState
        {
            private volatile HttpWebResponse _response;

            public HttpWebRequest Request { get; set; }
            public HttpWebResponse Response => _response;

            public void SetResponse(HttpWebResponse response)
            {
                if (_response != null) throw new InvalidOperationException();
                _response = response;
            }
        }

        public enum GetContentOutcome
        {
            Undefined,
            Ok,
            ConnectTimeout,
            ReadTimeout,
            ReadError,
            UnknownError
        }

        public class GetContentResult
        {
            private const string CharacterSetStart = "charset=";
            private const string CharacterSetStop = ";";

            private GetContentResult()
            {
                Content = new byte[] { };
                Exception = null;
                Headers = new ReadOnlyCollection<KeyValuePair<string, string>>(new List<KeyValuePair<string, string>>());
            }

            public GetContentResult(
                GetContentOutcome outcome,
                int? status = null,
                IReadOnlyCollection<KeyValuePair<string, string>> headers = null,
                Exception exception = null) : this()
            {
                // TODO : Checks
                Outcome = outcome;
                Headers = headers ?? Headers;
                Exception = exception;
                Status = status;
            }

            public GetContentResult(
                IReadOnlyCollection<KeyValuePair<string, string>> headers,
                int status,
                byte[] content) : this(GetContentOutcome.Ok, status, headers)
            {
                if (Content == null) throw new ArgumentNullException(nameof(content));
                Content = content;
            }

            public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; }
            public byte[] Content { get; }
            public Exception Exception { get; }
            public GetContentOutcome Outcome { get; }
            public int? Status { get; }

            public bool Ok => Outcome == GetContentOutcome.Ok;

            public Encoding GetEncoding()
            {
                var contentTypeHeader = Headers.SingleOrDefault(h => h.Key == "Content-Type");
                if (Equals(contentTypeHeader, default(KeyValuePair<string, string>))) return null;
                var value = contentTypeHeader.Value;
                var startTokenIndex = value.IndexOf(CharacterSetStart, StringComparison.OrdinalIgnoreCase);
                if (startTokenIndex == -1) return null;
                var firstPos = startTokenIndex + CharacterSetStart.Length;
                var endTokenIndex = value.IndexOf(CharacterSetStop, firstPos, StringComparison.OrdinalIgnoreCase);
                var encodingName = endTokenIndex == -1 ? value.Substring(firstPos) : value.Substring(firstPos, endTokenIndex - firstPos + 1);
                try
                {
                    return Encoding.GetEncoding(encodingName);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static GetContentResult GetContentFromUrl(Uri url, TimeSpan connectTimeout, TimeSpan readTimeout,
            int bufferSize, int connectSleepMilliseconds, int? maxContentLength = null)
        {
            #region arguments check

            if (url == null) throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri) throw new ArgumentException("uri must be absolute");
            if (!string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("url must be http(s)");
            if (connectTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(connectTimeout));
            if (readTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(readTimeout));
            if (bufferSize < 16)
                throw new ArgumentOutOfRangeException(nameof(bufferSize),
                    "bufferSize must be equal or larger than 16");
            if (connectSleepMilliseconds < 10)
                throw new ArgumentOutOfRangeException(nameof(connectSleepMilliseconds),
                    "connectSleepMilliseconds must be equal or larger than 10 (milliseconds)");
            if (maxContentLength.HasValue && maxContentLength.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxContentLength), "maxContentLength must be at least 1");

            #endregion

            var httpWebRequest = WebRequest.CreateHttp(url);
            var requestState = new RequestState {Request = httpWebRequest};
            var connectTimer = Stopwatch.StartNew();

            httpWebRequest.BeginGetResponse(ar =>
            {
                var state = (RequestState) ar.AsyncState;
                var request = state.Request;
                state.SetResponse((HttpWebResponse) request.EndGetResponse(ar));
            }, requestState);

            while (requestState.Response == null && connectTimer.Elapsed < connectTimeout)
                Thread.Sleep(connectSleepMilliseconds);

            if (!httpWebRequest.HaveResponse)
            {
                httpWebRequest.Abort();
                return new GetContentResult(GetContentOutcome.ConnectTimeout);
            }
            else
            {
                var response = requestState.Response;
                var status = (int) response.StatusCode;

                using (var memoryStream = new MemoryStream())
                using (var responseStream = response.GetResponseStream())
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;

                    Exception contentReadException = null;
                    var readTask = Task.Run(() =>
                    {
                        try
                        {
                            // ReSharper disable AccessToDisposedClosure
                            var buffer = new byte[bufferSize];
                            int read;
                            while ((read = responseStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                if (cancellationToken.IsCancellationRequested) return;
                                memoryStream.Write(buffer, 0, read);
                                if (cancellationToken.IsCancellationRequested) return;
                                if (maxContentLength.HasValue && memoryStream.Length >= maxContentLength.Value)
                                    break;
                            }
                            // ReSharper restore AccessToDisposedClosure
                        }
                        catch (Exception e)
                        {
                            contentReadException = e;
                        }
                    }, cancellationToken);

                    bool contentReadInTime;
                    contentReadInTime = readTask.Wait(readTimeout);
                    var headers = ExtractHeadersFromResponse(response);
                    if (!contentReadInTime)
                    {
                        cancellationTokenSource.Cancel();
                        return new GetContentResult(GetContentOutcome.ReadTimeout, status, headers);
                    }
                    return contentReadException != null
                        ? new GetContentResult(GetContentOutcome.ReadError, status, headers, contentReadException)
                        : new GetContentResult(headers, status, memoryStream.ToArray());
                }
            }
        }

        private static IReadOnlyCollection<KeyValuePair<string, string>> ExtractHeadersFromResponse(
            HttpWebResponse response)
        {
            var headersList = response.Headers.AllKeys
                .Select(key => new KeyValuePair<string, string>(key, response.Headers[key]))
                .ToList();
            headersList.Add(new KeyValuePair<string, string>("Status", ((int) response.StatusCode).ToString()));
            return headersList.AsReadOnly();
        }
    }
}