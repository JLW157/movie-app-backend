using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;

namespace MovieReactAPI.Filters
{
    public class MyExeptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<MyExeptionFilter> logger;

        public MyExeptionFilter(ILogger<MyExeptionFilter> logger) 
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);
            base.OnException(context);
        }
    }
}
