using AutoMapper;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Helper
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            CreateMap<Pokemon, PokemonDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Owner, OwnerDto>().ReverseMap();
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Reviewer, ReviewerDto>().ReverseMap();
            //CreateMap<CategoryDto, Category>();
            //CreateMap<CountryDto, Country>();
            //CreateMap<OwnerDto, Owner>();
            //CreateMap<PokemonDto, Pokemon>();
            //CreateMap<ReviewDto, Review>();
            //CreateMap<ReviewerDto, Reviewer>();
        }
    }
}
