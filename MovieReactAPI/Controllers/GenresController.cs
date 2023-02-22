using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReactAPI.DTO_s;
using MovieReactAPI.Entities;
using MovieReactAPI.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace MovieReactAPI.Controllers
{
    [Route("api/genres")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
    public class GenresController : ControllerBase
    {
        private readonly ILogger<GenresController> logger;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public GenresController(ILogger<GenresController> logger, 
            ApplicationDbContext context,
            IMapper mapper)
        {
            this.logger = logger;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<GenreDTO>>> GetAllGenres([FromQuery]PaginationDTO pagination)
        {
            var quariable = context.Genres.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(quariable);

            var genres = await quariable.OrderBy(x => x.Name).Paginate(pagination).ToListAsync();
            
            var genresDTO = new List<GenreDTO>();

            return mapper.Map<List<GenreDTO>>(genres);
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GenreDTO>>> GetAll()
        {
            var genres = await context.Genres.OrderBy(x => x.Name).ToListAsync();

            return mapper.Map<List<GenreDTO>>(genres);
        } 

        [HttpPost]
        public async Task<IActionResult> AddGenre([FromBody] CreateGenreDTO genreDTO)
        {
            var genre = mapper.Map<Genre>(genreDTO);
            // we are able to ommit Genres
            await context.AddAsync<Genre>(genre);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GenreDTO?>> GetGenre(int id)
        {
            var genre = await context.Genres.FirstOrDefaultAsync(x => x.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            return mapper.Map<GenreDTO>(genre);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutGenre(int id, [FromBody]CreateGenreDTO genreDTO)
        {
            var genre = await context.Genres.FirstOrDefaultAsync(x => x.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            genre = mapper.Map(genreDTO, genre);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var exists = await context.Genres.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return NotFound();
            }

            context.Remove(new Genre { Id = id });
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
