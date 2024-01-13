using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : Controller
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerRepository ownerRepository,  IMapper mapper, ICountryRepository countryRepository)
        {
            _ownerRepository = ownerRepository;
            _mapper = mapper;
            _countryRepository = countryRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        public IActionResult GetOwners() { 
        
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwners());
            return new OkObjectResult(owners);
        
        }


        [HttpGet("{ownerId}", Name = nameof(GetOwner))]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]
        public IActionResult GetOwner(int ownerId)
        {
            bool ownerExists = _ownerRepository.OwnerExists(ownerId);
            if (!ownerExists)
            {
                return new NotFoundObjectResult($"Owner with id: {ownerId} not found");
            }
            var owner = _ownerRepository.GetOwner(ownerId);
            return new OkObjectResult(owner);
        }

        [HttpGet("{ownerId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemonByOwner(int ownerId)
        {
            bool ownerExists = _ownerRepository.OwnerExists(ownerId);
            if (!ownerExists)
            {
                return new NotFoundObjectResult($"Owner with id: {ownerId} not found");
            }

            var pokemons = _mapper.Map<List<PokemonDto>>(_ownerRepository.GetPokemonByOwner(ownerId));
            return new OkObjectResult(pokemons);
        }

        [HttpPost("create/country/{countryId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        // could pass country as a query param using [FromQuery] but it makes more sense
        // to pass it as a path param
        public IActionResult CreateOwner([FromRoute] int countryId, [FromBody] OwnerDto ownerCreate) {
            
            if (ownerCreate == null) { return new BadRequestObjectResult("Owner is null"); }
            // checking by last name arbitrarily
            bool ownerExists = _ownerRepository.GetOwners().Any(o => o.LastName.Trim().ToUpper() == ownerCreate.LastName.Trim().ToUpper());
            
            if (ownerExists) { return new BadRequestObjectResult($"Owner with last name: {ownerCreate.LastName} already exists"); }

            Country country = _countryRepository.GetCountry(countryId);

            if (country == null) { return new BadRequestObjectResult($"CountryId:{countryId} does not exist"); }
            
            var newOwner = _mapper.Map<Owner>(ownerCreate);

            newOwner.Country = country;



            if (!_ownerRepository.CreateOwner(newOwner))
            {
                return StatusCode(500, "Something went wrong saving Owner");
            }

            return CreatedAtRoute(routeName: nameof(GetOwner), 
                                  routeValues: new {ownerId = newOwner.Id}, 
                                  value: newOwner);
        }


        [HttpPut("{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCountry(int ownerId, [FromBody] OwnerDto updatedOwner)
        {
            if (updatedOwner is null) return new BadRequestObjectResult($"Body is null: {updatedOwner}");

            if (ownerId != updatedOwner.Id)
                return new BadRequestObjectResult($"Owner Id passed in: {ownerId} does not match body Id: {updatedOwner.Id}");

            bool ownerExists = _ownerRepository.OwnerExists(ownerId);
            if (!ownerExists) return new BadRequestObjectResult($"Owner Does not exist");

            Owner newOwner = _mapper.Map<Owner>(updatedOwner);

            if (!_ownerRepository.UpdateOwner(newOwner))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went went wrong updating owner");

            return NoContent();
        }

        [HttpDelete("{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCountry(int ownerId)
        {
            bool ownerExists = (_ownerRepository.OwnerExists(ownerId));
            if (!ownerExists) return new BadRequestObjectResult($"Owner with id: {ownerId} not found");

            Owner ownerToDelete = _ownerRepository.GetOwner(ownerId);

            if (!_ownerRepository.DeleteOwner(ownerToDelete))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong deleting owner");

            return NoContent();

        }
    
    }
}
