using Aliencube.AzureFunctions.Extensions.DependencyInjection.Abstractions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Aliencube.XslMapper.FunctionApp.Functions
{
    /// <summary>
    /// This provides interfaces to the <see cref="XmlToXmlMapperFunction"/> class.
    /// </summary>
    public interface IXmlToXmlMapperFunction : IFunction<ILogger>
    {
        Task<HttpResponseData> InvokeAsync2(HttpRequestData input, FunctionOptionsBase options = null);
    }
   
}