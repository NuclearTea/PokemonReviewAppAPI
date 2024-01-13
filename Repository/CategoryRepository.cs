using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context)
        {
            _context = context;
        }

        public bool CategoryExists(int categoryId)
        {
            bool category = _context.Categories.Any(c => c.Id == categoryId);
            return category;
        }

        public bool CreateCategory(Category category)
        {
            // Change Tracker
            // add, updating, modifying
            // connected vs disconnected (most of the time its connected)
            // diconnected: EntityState.Added = ...
            _context.Add(category);
            
            return Save();
            
        }

        public bool DeleteCategory(Category category)
        {
            _context.Remove(category);
            return Save();
        }

        public ICollection<Category> GetCategories()
        {
            var categories = _context.Categories.ToList();
            return categories;
        }

        public Category GetCategory(int id)
        {
            return _context.Categories.FirstOrDefault(c => c.Id == id);
        }

        public ICollection<Pokemon> GetPokemonByCategoryId(int categoryId)
        {
            var pokemonInCategory = _context.PokemonCategories.Where(pc =>  pc.CategoryId == categoryId).Select(c => c.Pokemon).ToList();
            return pokemonInCategory;
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        public bool UpdateCategory(Category category)
        {
            _context.Update(category);
            return Save();
        }
    }
}
