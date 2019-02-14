﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WebApiContrib.Core.Tests
{
    public class ActionResultsTests
    {
        private TestServer _server;

        public ActionResultsTests()
        {
            _server = new TestServer(new WebHostBuilder()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<ActionResultsStartup>());
        }

        [Theory]
        [InlineData("/ok", 200)]
        [InlineData("/accepted", 202)]
        [InlineData("/badrequest", 400)]
        [InlineData("/unauthorized", 401)]
        [InlineData("/forbidden", 403)]
        [InlineData("/notfound", 404)]
        public async Task StatusCode(string route, int httpStatusCode)
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, route);
            var result = await client.SendAsync(request);

            Assert.Equal(httpStatusCode, (int)result.StatusCode);
        }

        [Theory]
        [InlineData("/ok-with-object", 200)]
        [InlineData("/accepted-with-object", 202)]
        [InlineData("/badrequest-with-object", 400)]
        [InlineData("/notfound-with-object", 404)]
        [InlineData("/unprocessable", 422)]
        public async Task StatusCodeWithObject(string route, int httpStatusCode)
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, route);
            var result = await client.SendAsync(request);
            var objectResult = JsonConvert.DeserializeObject<Item>(await result.Content.ReadAsStringAsync());

            Assert.Equal(httpStatusCode, (int)result.StatusCode);
            Assert.Equal("test", objectResult.Name);
        }

        [Fact]
        public async Task CreatedWithObject()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/created");
            var result = await client.SendAsync(request);
            var objectResult = JsonConvert.DeserializeObject<Item>(await result.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal("https://foo.bar/", result.Headers.Location.ToString());
            Assert.Equal("test", objectResult.Name);
        }
    }
}
