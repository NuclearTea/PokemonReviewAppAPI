using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IPokemonRepository _pokeRepository;
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewController(IReviewRepository reviewRepository, IPokemonRepository pokemonRespository, 
            IReviewerRepository reviewerRespository,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;   
            _mapper = mapper;
            _pokeRepository = pokemonRespository;
            _reviewerRepository = reviewerRespository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviews() { 
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviews());
            return new OkObjectResult(reviews);
        }

        [HttpGet("{reviewId}", Name = nameof(GetReivew))]
        [ProducesResponseType(200, Type=typeof(Review))]
        [ProducesResponseType(400)]
        public IActionResult GetReivew(int reviewId)
        {
            bool reviewExists = _reviewRepository.ReviewExists(reviewId);
            if (!reviewExists)
            {
                return new BadRequestObjectResult($"Review with id: {reviewId} was not found");
            }

            var reviews = _reviewRepository.GetReview(reviewId);
            return new OkObjectResult(reviews);
        }

        [HttpGet("pokemon/{pokeId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsForAPokemon(int pokeId)
        {
            bool pokeExists = _pokeRepository.PokemonExists(pokeId);
            if (!pokeExists)
            {
                return new BadRequestObjectResult($"No pokemon with id: {pokeId} found");
            }

            var reviewsForAPokemon = _mapper.Map<List<Review>>(_reviewRepository.GetReviewsForPokemon(pokeId));
            return new OkObjectResult(reviewsForAPokemon);
        }

        [HttpGet("reviewer/{reviewerId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviewsForAReviewer(int reviewerId)
        {
            var reviewerExists = _reviewerRepository.ReviewerExists(reviewerId);
            if (!reviewerExists)
            {
                return new NotFoundObjectResult($"Reviewer with id: {reviewerId} not found");
            }
            
            var reviewsForReviewer = _mapper.Map<List<Review>>(_reviewRepository.GetReviewsFromReviewer(reviewerId));
            return new OkObjectResult(reviewsForReviewer);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateReview([FromQuery] int reviewerId, [FromQuery] int pokemonId, [FromBody] ReviewDto reviewCreate) {

            if (reviewCreate is null) { return new BadRequestObjectResult($"DTO cannot be null, review passed in:{reviewCreate}"); }

            // could check if a review already exists by some delimeter but I don't think any delimter is appropriate

            bool pokemonExists = _pokeRepository.PokemonExists(pokemonId);
            if (!pokemonExists) { return new BadRequestObjectResult($"Pokemon with id: {pokemonId} does not exist"); }

            bool reviewerExists = _reviewerRepository.ReviewerExists(reviewerId);
            if (!reviewerExists) { return new BadRequestObjectResult($"Reviewer with id: {reviewerId} does not exist"); }

            Pokemon pokemonBeingReviewed = _pokeRepository.GetPokemon(pokemonId); 

            Reviewer reviewerMakingReview = _reviewerRepository.GetReviewer(reviewerId);

            Review newReview = _mapper.Map<Review>(reviewCreate);

            newReview.Pokemon = pokemonBeingReviewed;

            newReview.Reviewer = reviewerMakingReview;

            if (!_reviewRepository.CreateReview(newReview)) 
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong saving review");

            return CreatedAtRoute(routeName: nameof(GetReivew),
                                  routeValues: new { reviewId = newReview.Id},
                                  value: newReview);
        
        }

        [HttpPut("{reviewId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateReviewer(int reviewId, [FromBody] ReviewDto updatedReview)
        {
            if (updatedReview is null) return new BadRequestObjectResult($"Body is null: {updatedReview}");

            if (reviewId != updatedReview.Id) return new BadRequestObjectResult($"Passed in id: {reviewId} does not match body id: {updatedReview.Id}");
            Review newReviewer = _mapper.Map<Review>(updatedReview);

            if (!_reviewRepository.UpdateReview(newReviewer))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong updating Review");

            return NoContent();
        }

        [HttpDelete("{reviewId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteReview(int reviewId) {

            bool reviewExists = _reviewRepository.ReviewExists(reviewId);
            if (!reviewExists ) return new BadRequestObjectResult($"Review given does not exist: {reviewId}");

            Review reviewToDelete = _reviewRepository.GetReview(reviewId);
            if (!_reviewRepository.DeleteReview(reviewToDelete))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong deleting review");

            return NoContent();

        }
    }
}
