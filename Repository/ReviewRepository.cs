using AutoMapper;
using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Repository
{
    public class ReviewRepository : IReviewRepository
    {

        private readonly DataContext _context;

        public ReviewRepository(DataContext dataContext)
        {
            _context = dataContext;
        }

        public bool CreateReview(Review review)
        {
            _context.Add(review);
            return Save();
        }

        public bool DeleteReview(Review review)
        {
            _context.Remove(review);
            return Save();
        }

        public bool DeleteReviews(List<Review> reviews)
        {
            _context.RemoveRange(reviews);
            return Save();
        }

        public Review GetReview(int id)
        {
            return _context.Reviews.FirstOrDefault(r => r.Id == id);
        }


        public ICollection<Review> GetReviews()
        {
            return _context.Reviews.ToList();
        }

        public ICollection<Review> GetReviewsForPokemon(int pokemonId)
        {
            //return _context.Pokemon.Where(p => p.Id == pokemonId).SelectMany(p => p.Reviews).ToList();
            return _context.Reviews.Where(r => r.Pokemon.Id == pokemonId).ToList(); 
        }

        public ICollection<Review> GetReviewsFromReviewer(int reviewerId)
        {
            //return _context.Reviewers.Where(r => r.Id == reviewerId).SelectMany(r => r.Reviews).ToList();
            return _context.Reviews.Where(r => r.Reviewer.Id == reviewerId).ToList();
        }

        public bool ReviewExists(int id)
        {
            return _context.Reviews.Any(r => r.Id == id);
        }

        public bool Save()
        {
            var savedChanges = _context.SaveChanges();
            return savedChanges > 0;
        }

        public bool UpdateReview(Review review)
        {
            _context.Update(review);
            return Save();
        }
    }
}
