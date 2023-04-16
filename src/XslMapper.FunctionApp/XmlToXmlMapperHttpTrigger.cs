using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Aliencube.AzureFunctions.Extensions.DependencyInjection;
using Aliencube.AzureFunctions.Extensions.DependencyInjection.Abstractions;
using Aliencube.XslMapper.FunctionApp.Configurations;
using Aliencube.XslMapper.FunctionApp.Functions;
using Aliencube.XslMapper.FunctionApp.Helpers;
using Aliencube.XslMapper.FunctionApp.Models;
using Aliencube.XslMapper.FunctionApp.Modules;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aliencube.XslMapper.FunctionApp
{
    /// <summary>
    /// This represents the HTTP trigger entity for XML to XML transformation.
    /// </summary>
    public  class XmlToXmlMapperHttpTrigger
    {
        public static IFunctionFactory Factory { get; set; } = new FunctionFactory(new AppModule());
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
        private readonly ILogger _logger;
        public XmlToXmlMapperHttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<XmlToXmlMapperHttpTrigger>();
        }
        /// <summary>
        /// Gets the <see cref="IFunctionFactory"/> instance.
        /// </summary>

        /// <summary>
        /// Invokes the HTTP trigger.
        /// </summary>
        /// <param name="req"><see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="log"><see cref="ILogger"/> instance.</param>
        /// <returns><see cref="HttpResponseData"/> instance.</returns>
        [Function(nameof(XmlToXmlMapperHttpTrigger))]
        public  async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mappers/xml/xml")]
        HttpRequestData req)
        {
            var result = (HttpResponseData)null;
            try
            {


                AppSettings mya = new AppSettings();
                BlobStorageHelper myBlobStorageHelper = new BlobStorageHelper(mya);
                XmlTransformHelper myXmlTransformHelper = new XmlTransformHelper(mya, myBlobStorageHelper);
                XmlToXmlMapperFunction myXmlToXmlMapperFunction = new XmlToXmlMapperFunction(mya, myXmlTransformHelper);

                //result = await myXmlToXmlMapperFunction.InvokeAsync<HttpRequestData, HttpResponseData>(req).ConfigureAwait(false);
               
                result = await Factory.Create<IXmlToXmlMapperFunction, ILogger>(_logger)
                                       .InvokeAsync<HttpRequestData, HttpResponseData>(req)
.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var statusCode = HttpStatusCode.InternalServerError;
                var response = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                result = req.CreateResponse(statusCode);
                _ = result.WriteAsJsonAsync(response).ConfigureAwait(false);
            }

            return result;
        }
    }
}