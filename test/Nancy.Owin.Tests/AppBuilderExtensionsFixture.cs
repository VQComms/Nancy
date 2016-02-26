namespace Nancy.Owin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using global::Owin;

    using Microsoft.Owin.Testing;

    using Nancy.Testing;

    using Xunit;

    using HttpStatusCode = Nancy.HttpStatusCode;

    public class AppBuilderExtensionsFixture
    {
#if !__MonoCS__
        [Fact]
        public void When_host_Nancy_via_IAppBuilder_then_should_handle_requests()
        {
            // Given
            var bootstrapper = new ConfigurableBootstrapper(config => config.Module<TestModule>());

            using (var server = TestServer.Create(app => app.UseNancy(opts => opts.Bootstrapper = bootstrapper)))
            {
                // When
                var response = server.HttpClient.GetAsync(new Uri("http://localhost/")).Result;

                // Then
                Assert.Equal(response.StatusCode, System.Net.HttpStatusCode.OK);
            }
        }

        [Fact]
        public void When_host_Nancy_via_IAppBuilder_should_read_X509Certificate2()
        {
            // Given
            var bootstrapper = new ConfigurableBootstrapper(config => config.Module<TestModule>());


            using (var server = TestServer.Create(app => app.UseNancy(opts =>
            {
                opts.Bootstrapper = bootstrapper;
                opts.EnableClientCertificates = true;
            })))
            {
                // When
                var cert = File.ReadAllBytes("Resources\\NancyCert.crt");

                var env = new Dictionary<string, object>()
                {
                    { "owin.RequestPath", "/ssl" },
                    { "owin.RequestScheme", "http" },
                    { "owin.RequestHeaders", new Dictionary<string, string[]>() { { "Host", new[] { "localhost" } } } },
                    { "owin.RequestMethod", "GET" },
                    {"ssl.ClientCertificate", new X509Certificate(cert) }
                };
                server.Invoke(env);


                // Then
                Assert.Equal(env["owin.ResponseStatusCode"], 200);
            }
        }
#endif

        public class TestModule : LegacyNancyModule
        {
            public TestModule()
            {
                Get["/"] = _ =>
                {
                    return HttpStatusCode.OK;
                };

                Get["/ssl"] = _ =>
                {
                    return this.Request.ClientCertificate != null ? 200 : 500;
                };
            }
        }
    }
}