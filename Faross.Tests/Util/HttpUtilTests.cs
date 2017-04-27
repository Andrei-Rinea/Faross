using System.Collections.Generic;
using System.Text;
using Faross.Util;
using Xunit;

namespace Faross.Tests.Util
{
    public class HttpUtilTests
    {
        public class GetContentResult
        {
            private const string ContentTypeClassic = "text/html; charset=UTF-8";
            private const string ContentTypeLong = "text/html; charset=UTF-8; what=ever";

            [Fact]
            public void GetEncoding_Returns_CorrectEncoding_ForClassicContentType()
            {
                var headers = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("Content-Type", ContentTypeClassic)};
                var systemUnderTest = new HttpUtil.GetContentResult(HttpUtil.GetContentOutcome.Ok, 200, headers);
                var encoding = systemUnderTest.GetEncoding();

                Assert.Equal(Encoding.UTF8, encoding);
            }
        }
    }
}