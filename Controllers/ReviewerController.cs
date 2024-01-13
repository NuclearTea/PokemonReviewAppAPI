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
    public class ReviewerController : Controller
    {
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewerController(IReviewerRepository reviewerRespository, IMapper mapper)
        {
            _reviewerRepository = reviewerRespository;
            _mapper = mapper;
        }

        [HttpGet("Reviewers")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Reviewer>))]
        public IActionResult GetReviewers()
        {
            var mappedReviewers = _mapper.Map<List<ReviewerDto>>(_reviewerRepository.GetReviewers());
            return new OkObjectResult(mappedReviewers);
        }

        [HttpGet("{reviewerId}", Name = nameof(GetReiviewer))]
        [ProducesResponseType(200, Type = typeof(Reviewer))]
        public IActionResult GetReiviewer(int reviewerId)
        {
            bool reviwerExists = _reviewerRepository.ReviewerExists(reviewerId);
            if (!reviwerExists)
            {
                return new NotFoundObjectResult($"Reviwer with id: {reviewerId} was not found");
            }

            var reviewer = _mapper.Map<ReviewerDto>(_reviewerRepository.GetReviewer(reviewerId));
            return new OkObjectResult(reviewer);
        }

        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            bool reviwerExists = _reviewerRepository.ReviewerExists(reviewerId);
            if (!reviwerExists)
            {
                return new NotFoundObjectResult($"Reviwer with id: {reviewerId} was not found");
            }

            var reviews = _mapper.Map<List<ReviewDto>>(_reviewerRepository.GetReviewsByReviewer(reviewerId));
            return new OkObjectResult(reviews);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateReviewer([FromBody] ReviewerDto reviewerCreate)
        {
            if (reviewerCreate is null) return new BadRequestObjectResult($"Reviewer Object is null, {reviewerCreate}");

            // could check if passed in reviewer info is similar to an existing one but its ok to have
            // multiple people with the same name

            Reviewer newReviewer = _mapper.Map<Reviewer>(reviewerCreate);

            if (!_reviewerRepository.CreateReviewer(newReviewer)) 
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong saving reviewer");

            return CreatedAtRoute(routeName: nameof(GetReiviewer),
                                  routeValues: new { reviewerId = newReviewer.Id },
                                  value: newReviewer);
        }

        [HttpPut("{reviewerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateReviewer(int reviewerId, [FromBody] ReviewerDto updatedReviewer)
        {
            if (updatedReviewer is null) return new BadRequestObjectResult($"Body is null: {updatedReviewer}");

            if (reviewerId != updatedReviewer.Id) return new BadRequestObjectResult($"Passed in id: {reviewerId} does not match body id: {updatedReviewer.Id}");
            Reviewer newReviewer = _mapper.Map<Reviewer>(updatedReviewer);

            if (!_reviewerRepository.UpdateReviewer(newReviewer))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong updating Reviewer");

            return NoContent();
        }

        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteReviewer(int reviewerId)
        {

            bool reviewerExists = _reviewerRepository.ReviewerExists(reviewerId);
            if (!reviewerExists) return new BadRequestObjectResult($"Reviewer given does not exist: {reviewerId}");

            Reviewer reviewerToDelete = _reviewerRepository.GetReviewer(reviewerId);
            if (!_reviewerRepository.DeleteReviewer(reviewerToDelete))
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong deleting reviewer");

            return NoContent();

        }


    }
}
