using API.DTOs;
using Domain.Entities;
using AutoMapper;

namespace API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings here
            CreateMap<Product, ProductReadDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            CreateMap<Product, ProductReadDto>()
                 .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            // Category mappings here
            CreateMap<Category, CategoryReadDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // User mappings here
            CreateMap<User, UserReadDto>();
            CreateMap<UserRegisterDto, User>();
        }
    }
}
