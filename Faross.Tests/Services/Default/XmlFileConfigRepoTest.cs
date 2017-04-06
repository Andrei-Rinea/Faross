using System.IO;
using System.Text;
using Faross.Services;
using NSubstitute;
using Xunit;

namespace Faross.Tests.Services.Default
{
    public class XmlFileConfigRepo
    {
        private const string Path = "some/path/to/config.xml";
        private const string Xml =
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <config>
                    <environments>
                        <environment id=""1"" name=""SharedHosting""/>
                    </environments>

                    <services>
                        <service id=""1"" name=""Tech Blog"">
                            <runsOn>
                                <envRef id=""1""/>
                            </runsOn>
                        </service>
                    </services>

                    <checks>
                        <check id=""1"" type=""HttpCall"" method=""Get"" envRef=""1"" serviceRef=""1"" url=""http://blog.andrei.rinea.ro"" interval=""00:05:00"">
                            <conditions>
                                <httpCheckCondition type=""Status"" operator=""Equals"">200</httpCheckCondition>
                                <httpCheckCondition type=""Content"" operator=""DoesNotContain"" arguments=""IgnoreCase"">error</httpCheckCondition>
                            </conditions>
                        </check>
                    </checks>
                </config>";

        [Fact]
        public void ReadsConfigurationCorrectly()
        {
            var fileService = Substitute.For<IFileService>();

            var content = new MemoryStream(Encoding.UTF8.GetBytes(Xml));
            fileService.Open(Path).Returns(content);
            var systemUnderTest = new Faross.Services.Default.XmlFileConfigRepo(fileService, Path);

            var configuration = systemUnderTest.GetConfiguration();

            Assert.Equal(1, configuration.Environments.Count);
            Assert.Equal(1, configuration.Services.Count);
            Assert.Equal(1, configuration.Checks.Count);
        }
    }
}