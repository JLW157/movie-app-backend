using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Azure;
using MovieReactAPI.DTO_s;
using MovieReactAPI.Entities;
using MovieReactAPI.Helpers;
using System.Runtime;

namespace MovieReactAPI.Controllers
{
    [ApiController]
    [Route("api/movies")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string container = "movies";
        public MoviesController(ApplicationDbContext context,
            IMapper mapper, 
            IFileStorageService fileStorageService,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
            this.userManager = userManager;
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<MoviePostGetDTO>> PostGet()
        {
            var movieTheaters = await context.MovieTheaters.ToListAsync();
            var genres = await context.Genres.ToListAsync();

            var movieTheatersDTO = mapper.Map<List<MovieTheaterDTO>>(movieTheaters);
            var genresDTO = mapper.Map<List<GenreDTO>>(genres);

            return new MoviePostGetDTO() { Genres = genresDTO, MovieTheaters = movieTheatersDTO };
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDTO>> Get(int id)

        {
            var movie = await context.Movies
                .Include(x => x.MoviesGenres).ThenInclude(x => x.Genre)
                .Include(x => x.MovieTheatersMovies).ThenInclude(x => x.MovieTheater)
                .Include(x => x.MoviesActors).ThenInclude(x => x.Actor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var avarageVote = 0.0;
            var userVote = 0;

            if (await context.Ratings.AnyAsync(x => x.MovieId == id))
            {
                avarageVote = await context.Ratings.Where(x => x.MovieId == id)
                    .AverageAsync(x => x.Rate);

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var email = HttpContext.User.Claims
                        .FirstOrDefault(x => x.Type == ClaimsConstans.Email).Value;

                    var user = await userManager.FindByEmailAsync(email);
                    var userId = user.Id;

                    var ratingDb = await context.Ratings
                        .FirstOrDefaultAsync(x => x.MovieId == id
                        && x.UserId == userId);

                    if (ratingDb != null)
                    {
                        userVote = ratingDb.Rate;
                    }
                }
            }

            var dto = mapper.Map<MovieDTO>(movie);
            dto.Actors = dto.Actors.OrderBy(x => x.Order).ToList();
            dto.UserVote = userVote;
            dto.AvarageVote = avarageVote;

            return dto;
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMovies)
        {
            var moviesQuaryable = context.Movies.AsQueryable();
            if (!string.IsNullOrEmpty(filterMovies.Title))
            {
                moviesQuaryable = moviesQuaryable.Where(x => x.Title.Contains(filterMovies.Title));
            }

            if (filterMovies.InTheaters)
            {
                moviesQuaryable = moviesQuaryable.Where(x => x.InTheaters == true);
            }

            if (filterMovies.UpcomingReleases)
            {
                moviesQuaryable = moviesQuaryable.Where(x => x.ReleaseDate > DateTime.Today);
            }

            if (filterMovies.GenreId != 0)
            {
                moviesQuaryable = moviesQuaryable.Where(x =>
                x.MoviesGenres.Select(y => y.GenreId)
                .Contains(filterMovies.GenreId));
            }

            await HttpContext.InsertParametersPaginationInHeader(moviesQuaryable);

            var movies = new List<Movie>();

            if (filterMovies.SortByAsc)
            {
                 movies = await moviesQuaryable.OrderBy(x => x.Title).Paginate(filterMovies.PaginationDTO)
                .ToListAsync();
            }
            else
            {
                movies = await moviesQuaryable.OrderByDescending(x => x.Title).Paginate(filterMovies.PaginationDTO)
                    .ToListAsync();
            }

            return mapper.Map<List<MovieDTO>>(movies);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var today = DateTime.Today;

            var upcomingReleases = await context.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var inTheaters = await context.Movies
                .Where(x => x.InTheaters)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var landingPageDTO = new LandingPageDTO();
            landingPageDTO.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);
            landingPageDTO.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);

            return landingPageDTO;
        }

        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviePutGetDTO>> PutGet(int id)
        {
            var movieActionResut = await Get(id);
            if (movieActionResut.Result is NotFoundResult)
            {
                return NotFound();
            }

            var movie = movieActionResut.Value;

            var genresSelectedIds = movie!.Genres.Select(x => x.Id).ToList();
            var nonSelectedGenres = await context.Genres.Where(x => !genresSelectedIds.Contains(x.Id))
                .ToListAsync();

            var movieTheatersIds = movie.MovieTheaters.Select(x => x.Id).ToList();
            var nonSelectedMovieTheaters = await context.MovieTheaters.Where(x => !movieTheatersIds.Contains(x.Id))
                .ToListAsync();

            var nonSelectedGenresDTO = mapper.Map<List<GenreDTO>>(nonSelectedMovieTheaters);
            var nonSelectedMovieTheatersDTO = mapper.Map<List<MovieTheaterDTO>>(nonSelectedMovieTheaters);

            var response = new MoviePutGetDTO();
            response.Movie = movie;
            response.SelectedGenres = movie.Genres;
            response.NonSelectedGenres = nonSelectedGenresDTO;
            response.SelectedMovieTheaters = movie.MovieTheaters;
            response.NonSelectedMovieTheaters = nonSelectedMovieTheatersDTO;
            response.Actors = movie.Actors;

            return response;
        }

        [HttpPut("putget/{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = await context.Movies.Include(x => x.MoviesActors)
                .Include(x => x.MoviesGenres)
                .Include(x => x.MovieTheatersMovies)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            movie = mapper.Map(movieCreationDTO, movie);

            if (movieCreationDTO.Poster != null)
            {
                await fileStorageService.EditFile(container, movieCreationDTO.Poster,
                    movie.Poster);
            }

            AnnotateActorsOrder(movie);
            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(container, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movie);
            try
            {

                await context.AddAsync<Movie>(movie);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return movie.Id;
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            context.Remove(movie);
            await context.SaveChangesAsync();
            await fileStorageService.DeleteFile(movie.Poster, container);

            return NoContent();
        }

        private void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
    }
}
