using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController: Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [ProducesResponseType(200, Type= typeof(IEnumerable<Category>))]
        [HttpGet]
        public ActionResult GetCategories()
        {
            var mappedCategories = _mapper.Map<List<CategoryDto>>(_categoryRepository.GetCategories());
            return new OkObjectResult(mappedCategories);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Category))]
        [ProducesResponseType(400)]
        public ActionResult GetCategoryById(int id) {
        
            if (!_categoryRepository.CategoryExists(id))
            {
                return new NotFoundObjectResult($"No Category with id: {id}");
            }

            var mappedCategory = _mapper.Map<CategoryDto>(_categoryRepository.GetCategory(id));
            return new OkObjectResult(mappedCategory);

        }

        [HttpGet("pokemon/{id}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public ActionResult GetPokemonByCategoryId(int id) { 
        
            if (!_categoryRepository.CategoryExists(id))
            {
                return new NotFoundObjectResult($"No Category with id: {id}");
            }

            var allPokemonInCategory = _mapper.Map<List<PokemonDto>>(_categoryRepository.GetPokemonByCategoryId(id));

            return new OkObjectResult(allPokemonInCategory);

        
        }


        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateCategory([FromBody] CategoryDto categoryCreate) { 
            if (categoryCreate == null) {
                return new BadRequestObjectResult($"Passed in : {categoryCreate}");
                    }        
            
            bool categoryExists = _categoryRepository.GetCategories().Any(c => c.Name.Trim().ToUpper() == categoryCreate.Name.Trim().ToUpper());

            if (categoryExists) { return new BadRequestObjectResult($"{categoryCreate.Name} Already Exists"); }

            var categoryMap = _mapper.Map<Category>(categoryCreate);
            
            if (!_categoryRepository.CreateCategory(categoryMap)) {

                return StatusCode(500, "Something went wrong while saving");
            }

            return new OkObjectResult(categoryMap);  
        }

        [HttpPut("{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryDto updatedCategory) { 
        
            if (updatedCategory is null) { return new BadRequestObjectResult($"Body cannot be null: {updatedCategory}"); }

            if (categoryId != updatedCategory.Id) return new BadRequestObjectResult($"Category Id: {categoryId} passed in does not match body id: {updatedCategory.Id}");

            bool categoryExists = _categoryRepository.CategoryExists(categoryId);
            if (!categoryExists) { return new BadRequestObjectResult($"{categoryId} does not exist"); }

            Category newCategory = _mapper.Map<Category>(updatedCategory);

            if (!_categoryRepository.UpdateCategory(newCategory))
            {
                return StatusCode(500, "Something went wrong updating category");
            }
            return NoContent();

        }

        [HttpDelete("{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // If action has been enacted and the response contains status update
        [ProducesResponseType(StatusCodes.Status202Accepted)] // if action will likely succeed but has not been enacted yet
        [ProducesResponseType(StatusCodes.Status204NoContent)] // if action has been enacted and no information will be returned
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        public IActionResult DeleteCategory(int categoryId) { 
            bool categoryExists = (_categoryRepository.CategoryExists(categoryId));
            if (!categoryExists) 
                return new BadRequestObjectResult($"Category Id supplied:{categoryId} does not exist"); 

            Category categoryToDelete = _categoryRepository.GetCategory(categoryId);

            // Generally speaking, need to check all data which is tied to whatever you are deleting
            // and make sure deleting does not corrupt the remaining data

            if (!_categoryRepository.DeleteCategory(categoryToDelete))
                return StatusCode(500, "Something went wrong deleting category");

            return NoContent();
        }



    }
}
