using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Faross.Util
{
    public static class HttpUtil
    {
        private class RequestState
        {
            public HttpWebRequest Request { get; set; }
            public HttpWebResponse Response { get; set; }
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
            private GetContentResult()
            {
                Content = new byte[] { };
                Exception = null;
                Headers = new ReadOnlyCollection<KeyValuePair<string, string>>(new List<KeyValuePair<string, string>>());
            }

            public GetContentResult(
                GetContentOutcome outcome,
                IReadOnlyCollection<KeyValuePair<string, string>> headers = null,
                Exception exception = null) : this()
            {
                Outcome = outcome;
                Headers = headers ?? Headers;
                Exception = exception;
            }

            public GetContentResult(IReadOnlyCollection<KeyValuePair<string, string>> headers, byte[] content)
                : this(GetContentOutcome.Ok, headers)
            {
                Content = content;
            }

            public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; }
            public byte[] Content { get; }
            public Exception Exception { get; }
            public GetContentOutcome Outcome { get; }

            public bool Ok => Outcome == GetContentOutcome.Ok;
        }

        public static GetContentResult GetContentFromUrl(Uri url, TimeSpan connectTimeout, TimeSpan readTimeout,
            int bufferSize, int connectSleepMilliseconds)
        {
            #region arguments check

            if (url == null) throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri) throw new ArgumentException("uri must be absolute");
            if (!string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("url must be http(s)");
            if (connectTimeout < TimeSpan.MinValue) throw new ArgumentOutOfRangeException(nameof(connectTimeout));
            if (readTimeout < TimeSpan.MinValue) throw new ArgumentOutOfRangeException(nameof(readTimeout));
            if (bufferSize < 16)
                throw new ArgumentOutOfRangeException(nameof(bufferSize),
                    "bufferSize must be equal or larger than 16");
            if (connectSleepMilliseconds < 10)
                throw new ArgumentOutOfRangeException(nameof(connectSleepMilliseconds),
                    "connectSleepMilliseconds must be equal or larger than 10 (milliseconds)");

            #endregion

            var httpWebRequest = WebRequest.CreateHttp(url);
            var requestState = new RequestState {Request = httpWebRequest};
            var connectTimer = Stopwatch.StartNew();
            httpWebRequest.BeginGetResponse(ar =>
            {
                var state = (RequestState) ar.AsyncState;
                var request = state.Request;
                state.Response = (HttpWebResponse) request.EndGetResponse(ar);
            }, requestState);

            while (!httpWebRequest.HaveResponse && connectTimer.Elapsed < connectTimeout)
                Thread.Sleep(connectSleepMilliseconds);

            if (!httpWebRequest.HaveResponse)
            {
                httpWebRequest.Abort();
                return new GetContentResult(GetContentOutcome.ConnectTimeout);
            }
            else
            {
                var response = requestState.Response;

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
                        return new GetContentResult(GetContentOutcome.ReadTimeout, headers);
                    }
                    return contentReadException != null
                        ? new GetContentResult(GetContentOutcome.ReadError, headers, contentReadException)
                        : new GetContentResult(headers, memoryStream.ToArray());
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