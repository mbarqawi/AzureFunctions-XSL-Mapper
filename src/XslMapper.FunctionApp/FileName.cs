using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Aliencube.XslMapper.FunctionApp
{
   

  
        public class HttpFunction
        {
            private readonly ILogger _logger;

            public HttpFunction(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<HttpFunction>();
            }

            [Function(nameof(HttpFunction))]
            public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString("Welcome to Azure Functions - Isolated .NET Framework!");

                return response;
            }
        }
    }

