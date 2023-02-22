using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReactAPI.DTO_s;
using MovieReactAPI.Entities;
using MovieReactAPI.Helpers;

namespace MovieReactAPI.Controllers
{
    [Route("api/actors")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
    public class ActorsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "actors";

        public ActorsController(ApplicationDbContext context, IMapper mapper,
            IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> GetActors([FromQuery] PaginationDTO paginationDTO)
        {
            var queriable = context.Actors.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(queriable);
            var actors = await queriable.OrderBy(x => x.Name).Paginate(paginationDTO).ToListAsync();

            return mapper.Map<List<ActorDTO>>(actors);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound($"Actor with id - {id} not found.");
            }

            return mapper.Map<ActorDTO>(actor);
        }

        [HttpGet("searchByName/{query}")]
        public async Task<ActionResult<List<MovieActorsDTO>>> SearchByName(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<MovieActorsDTO>();
            }

            return await context.Actors.Where(x => x.Name.Contains(query))
                .OrderBy(x => x.Name)
                .Select(x => new MovieActorsDTO {Id = x.Id, Name = x.Name, Picture = x.Picture})
                .Take(5).
                ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = mapper.Map<Actor>(actorCreationDTO);
            if (actorCreationDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.SaveFile(containerName, actorCreationDTO.Picture);
            }

            context.Add(actor);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            // here we update actor
            actor = mapper.Map(actorCreationDTO, actor);

            if (actorCreationDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.EditFile(containerName,
                    actorCreationDTO.Picture, actor.Picture);
            }

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            context.Remove(actor);

            await context.SaveChangesAsync();

            await fileStorageService.DeleteFile(actor.Picture, containerName);
            return NoContent();
        }

    }
}
