using MovieReactAPI.DTO_s;
using System.Net.NetworkInformation;

namespace MovieReactAPI.Helpers
{
    public static class IQueriableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO paginationDTO)
        {
            return queryable.Skip((paginationDTO.Page - 1) * paginationDTO.RecordsPerPage)
                .Take(paginationDTO.RecordsPerPage);
        }
    }
}
