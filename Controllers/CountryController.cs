using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : Controller
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMapper _mapper;

        public CountryController(ICountryRepository countryRepository, IMapper mapper, IOwnerRepository ownerRepository)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
            _ownerRepository = ownerRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Country>))]
        public IActionResult GetCountries()
        {
            var mappedCountries = _mapper.Map<List<CountryDto>>(_countryRepository.GetCountries());
            return new OkObjectResult(mappedCountries);
        }

        [HttpGet("{id}", Name = nameof(GetCountry))]
        [ProducesResponseType(200, Type = typeof(Country))]
        [ProducesResponseType(400)]
        public IActionResult GetCountry(int id)
        {
            bool countryExists = _countryRepository.CountryExists(id);
            if (!countryExists) { return new BadRequestObjectResult($"Country with id: {id} not found"); }

            var country = _mapper.Map<CountryDto>(_countryRepository.GetCountry(id));
            return new OkObjectResult(country);
        }

        [HttpGet("owners/{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Country))]
        [ProducesResponseType(400)]
        public IActionResult GetCountryOfOwner(int ownerId)
        {
            bool ownerIsValid = _ownerRepository.OwnerExists(ownerId);
            if (!ownerIsValid) { return new BadRequestObjectResult($"Owner id: {ownerId} is not valid"); }
            var country = _mapper.Map<CountryDto>(_countryRepository.GetCountryByOwner(ownerId));
            return new OkObjectResult(country);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateCountry([FromBody] CountryDto countryCreate)
        {
            if (countryCreate == null) { return new BadRequestObjectResult("Country is null"); }

            bool countryExists = _countryRepository.GetCountries().Any(c => c.Name.Trim().ToUpper() == countryCreate.Name.Trim().ToUpper());
            if (countryExists) { return new BadRequestObjectResult($"{countryCreate.Name} Already Exists"); }

            var newCountry = _mapper.Map<Country>(countryCreate);

            
            if (!_countryRepository.CreateCountry(newCountry))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong saving country");
            }

            return CreatedAtAction(actionName: nameof(GetCountry),
                                   routeValues: new { id = newCountry.Id },
                                   value: newCountry);
        }

        [HttpPut("{countryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCountry(int countryId, [FromBody] CountryDto updatedCountry) {
            if (updatedCountry is null) return new BadRequestObjectResult($"Body is null: {updatedCountry}");

            if (countryId != updatedCountry.Id) 
                return new BadRequestObjectResult($"Country Id passed in: {countryId} does not match body Id: {updatedCountry.Id}");

            bool countryExists = _countryRepository.CountryExists(countryId);
            if (!countryExists) return new BadRequestObjectResult($"Country Does not exist");

            Country newCountry = _mapper.Map<Country>(updatedCountry);

            if (!_countryRepository.UpdateCountry(newCountry))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went went wrong updating country");

            return NoContent();
        }

        [HttpDelete("{countryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCountry (int countryId)
        {
            bool countryExists = (_countryRepository.CountryExists(countryId));

            if (!countryExists)
                return new BadRequestObjectResult($"Country Id passed in: {countryId} does not exist");

            Country countryToDelete = _countryRepository.GetCountry(countryId);

            if (!_countryRepository.DeleteCountry(countryToDelete))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong deleting country");
            
            return NoContent();
        
        }

    }
}
