using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;


        public PokemonController(IPokemonRepository pokemonRepository,
                                 ICategoryRepository categoryRepository, 
                                 IOwnerRepository ownerRepository,
                                 IReviewRepository reviewRepository,
                                 IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _ownerRepository = ownerRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonRepository.GetPokemons());

            return new OkObjectResult(pokemons);

        }

        [HttpGet("{pokeId}", Name = nameof(GetPokemon))]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int pokeId)
        {
            if (!_pokemonRepository.PokemonExists(pokeId)) return new NotFoundObjectResult($"No pokemon with pokeId: {pokeId}");


            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(pokeId));
            return new OkObjectResult(pokemon);
        }

        //[HttpGet("{name}")]
        //[ProducesResponseType(200, Type = typeof(Pokemon))]
        //[ProducesResponseType(400)]
        //public IActionResult GetPokemonByName(string name)
        //{
        //    var pokemon = _pokemonRepository.GetPokemonByName(name);
        //    if (pokemon == null) return new NotFoundResult();
        //    return new OkObjectResult(pokemon);
        //}


        [HttpGet("{pokeId}/rating")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonRating(int pokeId) {

            if (!_pokemonRepository.PokemonExists(pokeId))
                return new NotFoundResult();
             
            return new OkObjectResult(_pokemonRepository.GetPokemonRating(pokeId));
    
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int categoryId, [FromBody] PokemonDto pokemonCreate) {
            if (pokemonCreate is null) return new BadRequestObjectResult("Pokemon Object is null");
            
            bool ownerExists = _ownerRepository.OwnerExists(ownerId);
            if (!ownerExists) return new BadRequestObjectResult($"Owner with id:{ownerId} does not exist");

            bool categoryExists = _categoryRepository.CategoryExists(categoryId);
            if (!categoryExists) return new BadRequestObjectResult($"Category with id:{categoryId} does not exist");

            bool pokemonAlreadyExists = _pokemonRepository.GetPokemons().Any(p => p.Name.Trim() == pokemonCreate.Name.Trim());
            if (pokemonAlreadyExists) return new BadRequestObjectResult($"Pokemon with name:{pokemonCreate.Name} already exists");

            var newPokemon = _mapper.Map<Pokemon>(pokemonCreate);

            if (!_pokemonRepository.CreatePokemon(ownerId, categoryId, newPokemon))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong saving country");
            }

            return CreatedAtRoute(routeName: nameof(GetPokemon), 
                                  routeValues: new {pokeId = newPokemon.Id},
                                  value: newPokemon);

        }

        [HttpPut("{pokemonId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePokemon(int pokemonId, 
                                           [FromQuery] int ownerId, 
                                           [FromQuery] int categoryId, 
                                           [FromBody] PokemonDto updatedPokemon)
        {

            if (updatedPokemon is null) return new BadRequestObjectResult($"Body is null: {updatedPokemon}");

            bool ownerExists = _ownerRepository.OwnerExists(ownerId);
            if (!ownerExists) return new BadRequestObjectResult($"Owner Specified does not exist: {ownerId}");

            bool categoryExists = _categoryRepository.CategoryExists(categoryId);
            if (!categoryExists) return new BadRequestObjectResult($"Category Specified does not exist: {categoryId}");
            
            
            bool pokemonExists = _pokemonRepository.PokemonExists(pokemonId);
            if (!pokemonExists) return new BadRequestObjectResult($"Pokemon id specified: {pokemonId} does not exist");

            if (updatedPokemon.Id != pokemonId) return new BadRequestObjectResult($"Id specifed: {pokemonId} does not match body Id: {updatedPokemon.Id}");
            


            Pokemon newPokemon = _mapper.Map<Pokemon>(updatedPokemon);

            if (!_pokemonRepository.UpdatePokemon(ownerId, categoryId, newPokemon))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong updating pokemon");
            }

            return NoContent();

        }

        [HttpDelete("{pokemonId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeletePokemon(int pokemonId)
        {
            bool pokemonExists = _pokemonRepository.PokemonExists(pokemonId);
            if (!pokemonExists)
                return new BadRequestObjectResult($"Pokemon Id supplied:{pokemonId} does not exist");
            
            
            ICollection<Review> reviewsToDelete = _reviewRepository.GetReviewsForPokemon(pokemonId);
            if (!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong deleting reviews for a pokemon");
            
            Pokemon pokemonToDelete = _pokemonRepository.GetPokemon(pokemonId);

            
            if (!_pokemonRepository.DeletePokemon(pokemonToDelete))
                return StatusCode(500, "Something went wrong deleting category");

            return NoContent();
        }


    }
}
