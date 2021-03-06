﻿using System;
using System.IO;
using Octokit.Internal;
using Xunit;

namespace Octokit.Tests.Http
{
    public class JsonHttpPipelineTests
    {
        public class TheConstructor
        {
            [Fact]
            public void ThrowsForBadArguments()
            {
                Assert.Throws<ArgumentNullException>(() => new JsonHttpPipeline(null));
            }
        }

        public class TheSerializeRequestMethod
        {
            [Fact]
            public void SetsRequestAcceptHeader()
            {
                var request = new Request();
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Contains("Accept", request.Headers.Keys);
                Assert.Equal("application/vnd.github.v3+json; charset=utf-8", request.Headers["Accept"]);
            }

            [Fact]
            public void DoesNotChangeExistingAcceptsHeader()
            {
                var request = new Request();
                request.Headers.Add("Accept", "application/vnd.github.manifold-preview; charset=utf-8");
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Contains("Accept", request.Headers.Keys);
                Assert.Equal("application/vnd.github.manifold-preview; charset=utf-8", request.Headers["Accept"]);
            }

            [Fact]
            public void LeavesStringBodyAlone()
            {
                const string json = "just some string data";
                var request = new Request { Body = json };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Equal(json, request.Body);
            }

            [Fact]
            public void LeavesStreamBodyAlone()
            {
                var stream = new MemoryStream();
                var request = new Request { Body = stream };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Same(stream, request.Body);
            }

            [Fact]
            public void LeavesNullBodyAlone()
            {
                var request = new Request { Body = null };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Null(request.Body);
            }

            [Fact]
            public void EncodesObjectBody()
            {
                var request = new Request { Body = new { test = "value" } };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.SerializeRequest(request);

                Assert.Equal("{\"test\":\"value\"}", request.Body);
            }

            [Fact]
            public void EnsuresArguments()
            {
                var jsonPipeline = new JsonHttpPipeline();

                Assert.Throws<ArgumentNullException>(() => jsonPipeline.SerializeRequest(null));
            }
        }

        public class TheDeserializeResponseMethod
        {
            [Fact]
            public void DeserializesResponse()
            {
                const string data = "works";
                var response = new ApiResponse<string>
                {
                    Body = SimpleJson.SerializeObject(data),
                    ContentType = "application/json"
                };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.DeserializeResponse(response);

                Assert.NotNull(response.BodyAsObject);
                Assert.Equal(data, response.BodyAsObject);
            }

            [Fact]
            public void IgnoresResponsesNotIdentifiedAsJson()
            {
                const string data = "works";
                var response = new ApiResponse<string>
                {
                    Body = SimpleJson.SerializeObject(data),
                    ContentType = "text/html"
                };
                var jsonPipeline = new JsonHttpPipeline();

                jsonPipeline.DeserializeResponse(response);

                Assert.Null(response.BodyAsObject);
            }
        }
    }
}
