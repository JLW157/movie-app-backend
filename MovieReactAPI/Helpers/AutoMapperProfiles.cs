using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MovieReactAPI.DTO_s;
using MovieReactAPI.Entities;
using NetTopologySuite.Geometries;

namespace MovieReactAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<GenreDTO, Genre>().ReverseMap();
            CreateMap<CreateGenreDTO, Genre>().ReverseMap();

            CreateMap<ActorDTO, Actor>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>().ReverseMap();

            CreateMap<MovieTheater, MovieTheaterDTO>()
                .ForMember(x => x.Latitude, dto => dto.MapFrom(prop => prop.Loaction.Y))
                .ForMember(x => x.Longtiude, dto => dto.MapFrom(prop => prop.Loaction.X));

            CreateMap<MovieTheaterCreationDTO, MovieTheater>()
                .ForMember(x => x.Loaction, x => x.MapFrom(dto =>
                    geometryFactory.CreatePoint(new Coordinate(dto.Longtiude, dto.Latitude))
                ));

            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(x => x.Poster, opt => opt.Ignore())
                .ForMember(x => x.ReleaseDate, opt => opt.MapFrom(dto => dto.ReleaseDate))
                .ForMember(x => x.MoviesGenres, opt => opt.MapFrom(MapMoviesGenres))
                .ForMember(x => x.MovieTheatersMovies, opt => opt.MapFrom(MapMoviesMovieTheaters))
                .ForMember(x => x.MoviesActors, opt => opt.MapFrom(MapMoviesActors));

            CreateMap<Movie, MovieDTO>()
                .ForMember(x => x.Genres, opt => opt.MapFrom(MapMoviesGenres))
                .ForMember(x => x.MovieTheaters, opt => opt.MapFrom(MapMoviesMovieTheaters))
                .ForMember(x => x.Actors, opt => opt.MapFrom(MapMoviesActors));

            CreateMap<IdentityUser, UserDTO>().ReverseMap();
        }

        private List<MovieActorsDTO> MapMoviesActors(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieActorsDTO>();

            if (movie.MoviesActors != null)
            {
                foreach (var actor in movie.MoviesActors)
                {
                    result.Add(new MovieActorsDTO()
                    {
                        Id = actor.ActorId,
                        Name = actor.Actor.Name,
                        Character = actor.Character,
                        Picture = actor.Actor.Picture,
                        Order = actor.Order
                    });
                }
            }

            return result;
        }

        private List<MovieTheaterDTO> MapMoviesMovieTheaters(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieTheaterDTO>();

            if (movie.MovieTheatersMovies != null)
            {
                foreach (var movieTheater in movie.MovieTheatersMovies)
                {
                    result.Add(new MovieTheaterDTO()
                    {
                        Id = movieTheater.MovieTheaterId,
                        Name = movieTheater.MovieTheater.Name,
                        Latitude = movieTheater.MovieTheater.Loaction.Y,
                        Longtiude = movieTheater.MovieTheater.Loaction.X
                    });
                }
            }

            return result;
        }

        private List<GenreDTO> MapMoviesGenres(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<GenreDTO>();

            if (movie.MoviesGenres != null)
            {
                foreach (var genre in movie.MoviesGenres)
                {
                    result.Add(new GenreDTO() { Id = genre.GenreId, Name = genre.Genre.Name });
                }
            }

            return result;
        }

        private List<MovieTheatersMovies> MapMoviesMovieTheaters(MovieCreationDTO movieCreationDTO,
            Movie movie)
        {
            var result = new List<MovieTheatersMovies>();

            if (movieCreationDTO.MovieTheatersIds == null)
            {
                return result;
            }

            foreach (var id in movieCreationDTO.MovieTheatersIds)
            {
                result.Add(new MovieTheatersMovies() { MovieTheaterId = id });
            }

            return result;
        }

        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO,
            Movie movie)
        {
            var result = new List<MoviesActors>();

            if (movieCreationDTO.Actors == null)
            {
                return result;
            }

            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors() { ActorId = actor.Id, Character = actor.Character });
            }

            return result;
        }

        private List<MoviesGenres> MapMoviesGenres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGenres>();

            if (movieCreationDTO.GenresIds == null)
            {
                return result;
            }

            foreach (var id in movieCreationDTO.GenresIds)
            {
                result.Add(new MoviesGenres() { GenreId = id });
            }

            return result;
        }
    }
}