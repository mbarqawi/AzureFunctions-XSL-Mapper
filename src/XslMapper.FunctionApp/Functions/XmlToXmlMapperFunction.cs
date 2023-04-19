using System;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

using Aliencube.AzureFunctions.Extensions.DependencyInjection.Abstractions;
using Aliencube.XslMapper.FunctionApp.Configurations;
using Aliencube.XslMapper.FunctionApp.Exceptions;
using Aliencube.XslMapper.FunctionApp.Extensions;
using Aliencube.XslMapper.FunctionApp.Helpers;
using Aliencube.XslMapper.FunctionApp.Models;

using Microsoft.Extensions.Logging;


namespace Aliencube.XslMapper.FunctionApp.Functions
{
    /// <summary>
    /// This represents the function entity for the <see cref="XmlToXmlMapperHttpTrigger"/> class.
    /// </summary>
    public class XmlToXmlMapperFunction : FunctionBase<ILogger>, IXmlToXmlMapperFunction
    {
        private readonly AppSettings _settings;
        private readonly IXmlTransformHelper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlToXmlMapperFunction"/> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings"/> instance.</param>
        /// <param name="helper"><see cref="IXmlTransformHelper"/> instance.</param>
        public XmlToXmlMapperFunction(AppSettings settings, IXmlTransformHelper helper)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public override Task<TOutput> InvokeAsync<TInput, TOutput>(TInput input, FunctionOptionsBase options = null)
        {
            return base.InvokeAsync<TInput, TOutput>(input, options);
        }

        public  async Task<HttpResponseData> InvokeAsync2(HttpRequestData input, FunctionOptionsBase options = null)
        {
            this.Log.LogInformation("C# HTTP trigger function processed a request.");

            var req = input as HttpRequestData;
            var request = (XmlToXmlMapperRequest)null;
            var response = (HttpResponseData)null;
            try
            {
                //var text = req.ReadAsString(System.Text.Encoding.UTF8);
                request = await req.ReadFromJsonAsync<XmlToXmlMapperRequest>();
                            
            }
            catch (Exception ex)
            {
                var statusCode = HttpStatusCode.BadRequest;
                var result = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                response = req.CreateResponse(HttpStatusCode.BadRequest);
                _ = response.WriteAsJsonAsync(result).ConfigureAwait(false); 

                this.Log.LogError($"Request payload was invalid.");
                this.Log.LogError(ex.Message);
                this.Log.LogError(ex.StackTrace);

                return response;
                   // (TOutput)Convert.ChangeType(, typeof(TOutput));
            }

            try
            {
                var xmlcontent="";
                if (request.InputXml != null)
                    xmlcontent = request.InputXml;
                else if (request.Inputxmlfile != null)
                    xmlcontent =await  this._helper.LoadXmlAsync(this._settings.Containers.XMLcontainer, request.Inputxmlfile.Directory, request.Inputxmlfile.Name);

                var content = await this._helper
                                        .LoadXslAsync(this._settings.Containers.Mappers, request.Mapper.Directory, request.Mapper.Name)
                                        .AddArgumentsAsync(request.ExtensionObjects)
                                        .TransformAsync(xmlcontent,true, this._settings.Containers.XMLcontainer, request.Outputxmlfile.Directory, request.Outputxmlfile.Name)
                                        .ToStringAsync(this._settings.EncodeBase64Output);

                var result = new XmlToXmlMapperResponse() { Content = content };

                //response = req.CreateResponse(HttpStatusCode.OK, result, this._settings.JsonFormatter);

                response = req.CreateResponse(HttpStatusCode.OK);
                _ = response.WriteAsJsonAsync<XmlToXmlMapperResponse>(result).ConfigureAwait(false);
            }
            catch (CloudStorageNotFoundException ex)
            {
                var statusCode = HttpStatusCode.InternalServerError;
                var err = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                response = req.CreateResponse(statusCode);
                _ = response.WriteAsJsonAsync(err).ConfigureAwait(false);
            }
            catch (BlobContainerNotFoundException ex)
            {
                var statusCode = HttpStatusCode.BadRequest;
                var err = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                this.Log.LogError($"Request payload was invalid.");
                this.Log.LogError($"XSL mapper not found");

                response = req.CreateResponse(statusCode);
                _ = response.WriteAsJsonAsync(err).ConfigureAwait(false);
            }
            catch (BlobNotFoundException ex)
            {
                var statusCode = HttpStatusCode.BadRequest;
                var err = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                this.Log.LogError($"Request payload was invalid.");
                this.Log.LogError($"XSL mapper not found");

                response = req.CreateResponse(statusCode);
                _ = response.WriteAsJsonAsync(err).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var statusCode = HttpStatusCode.InternalServerError;
                var err = new ErrorResponse((int)statusCode, ex.Message, ex.StackTrace);

                response = req.CreateResponse(statusCode);
                _ = response.WriteAsJsonAsync(err).ConfigureAwait(false);
            }

            //return (TOutput)Convert.ChangeType(response, typeof(HttpResponseData));
            return response;
        }
    }
}