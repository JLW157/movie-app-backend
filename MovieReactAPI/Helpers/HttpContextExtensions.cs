using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MovieReactAPI.Helpers
{
    public static class HttpContextExtensions
    {
        public async static Task InsertParametersPaginationInHeader<T>(this HttpContext context, 
            IQueryable<T> quariable)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            double count = await quariable.CountAsync();

            context.Response.Headers.Append("totalAmountOfRecords", count.ToString());
        }
    }
}
