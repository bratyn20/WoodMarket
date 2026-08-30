using AutoMapper;
using WoodMarket.Models;

namespace WoodMarket.Dto
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ========================================
            // Product -> ProductDto
            // ========================================
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src => src.Reviews.Any()
                        ? src.Reviews.Average(r => r.Rating)
                        : 0))
                .ForMember(dest => dest.ReviewCount,
                    opt => opt.MapFrom(src => src.Reviews.Count(r => r.IsApproved)))
                .ForMember(dest => dest.IsInStock,
                    opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.IsOnSale,
                    opt => opt.MapFrom(src => src.OldPrice.HasValue))
                .ForMember(dest => dest.MaterialName,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : null))
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<Product, AdminProductDto>()
            .IncludeBase<Product, ProductDto>();

            CreateMap<AdminProductUpdateDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsInStock,
                    opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.IsOnSale,
                    opt => opt.MapFrom(src => src.OldPrice.HasValue))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.MapFrom(src => DateTime.UtcNow));


            // Маппинг ProductUpdateWithImageDto -> Product
            CreateMap<ProductUpdateWithImageDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID не обновляем
            .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore()) // Обрабатываем отдельно
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Дата создания не меняется
            .ForMember(dest => dest.IsInStock,
                opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.IsOnSale,
                opt => opt.MapFrom(src => src.OldPrice.HasValue))
            .ForMember(dest => dest.UpdatedAt,
                opt => opt.MapFrom(src => DateTime.UtcNow));


            CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsInStock,
                opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.IsOnSale,
                opt => opt.MapFrom(src => src.OldPrice.HasValue))
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewCount, opt => opt.Ignore())
            .ForMember(dest => dest.Slug, opt => opt.Ignore()) // Генерируем отдельно
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.WishlistItems, opt => opt.Ignore())
            .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
            .ForMember(dest => dest.Specifications, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore());

            // ========================================
            // Product -> ProductFullDto (детальный)
            // ========================================
            CreateMap<Product, ProductFullDto>()
                .ForMember(dest => dest.Images,
                    opt => opt.MapFrom(src => src.Images.Select(i => i.ImageUrl)))
                .ForMember(dest => dest.Variants,
                    opt => opt.MapFrom(src => src.Variants))
                .ForMember(dest => dest.Specifications,
                    opt => opt.MapFrom(src => src.Specifications.ToDictionary(
                        ps => ps.Specification.Name,
                        ps => ps.Value)))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.ProductTags.Select(pt => pt.Tag.Name)))
                .ForMember(dest => dest.IsInStock,
                    opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.IsOnSale,
                    opt => opt.MapFrom(src => src.OldPrice.HasValue))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.MaterialName,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : null));

            // ========================================
            // ProductVariant -> VariantDto
            // ========================================
            CreateMap<ProductVariant, VariantDto>();

            // ========================================
            // Review -> ReviewDto
            // ========================================
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

            // ========================================
            // Category -> CategoryDto
            // ========================================
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive)));

            // ========================================
            // Order -> OrderResponse
            // ========================================
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems));

            // ========================================
            // OrderItem -> OrderItemResponse
            // ========================================
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src => src.Product.MainImageUrl))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.ProductName ?? src.Product.Name)); ;

            // ========================================
            // CartItem -> CartItemDto
            // ========================================
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UnitPrice,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.Quantity * src.Product.Price))
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src => src.Product.MainImageUrl))
                .ForMember(dest => dest.MaxQuantity,
                    opt => opt.MapFrom(src => src.Product.StockQuantity));

            // ========================================
            // Brand -> BrandDto
            // ========================================
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive)));

            CreateMap<CreateBrandDto, Brand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Slug, opt => opt.Ignore())
            .ForMember(dest => dest.LogoUrl, opt => opt.Ignore());

            CreateMap<UpdateBrandDto, Brand>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore());

            // ========================================
            // Material -> MaterialDto
            // ========================================
            CreateMap<Material, MaterialDto>()
                .ForMember(dest => dest.ProductCount,
                    opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive)));

            // ========================================
            // Discount -> DiscountDto
            // ========================================
            CreateMap<Discount, DiscountDto>();

            // ========================================
            // BlogPost -> BlogPostDto
            // ========================================
            CreateMap<BlogPost, BlogPostDto>()
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src => src.BlogTags != null
                    ? src.BlogTags.Select(bt => bt.Tag.Name).ToList()
                    : new List<string>()))
            .ForMember(dest => dest.AuthorName,
                opt => opt.MapFrom(src => src.Author != null
                    ? $"{src.Author.FirstName} {src.Author.LastName}"
                    : null));

            CreateMap<CreateBlogPostDto, BlogPost>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Slug, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FeaturedImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.BlogTags, opt => opt.Ignore());

            CreateMap<UpdateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.BlogTags, opt => opt.Ignore());

            // ========================================
            // ApplicationUser -> UserProfileDto
            // ========================================
            CreateMap<User, UserProfileDto>();

            // ========================================
            // CustomOrder -> CustomOrderDto
            // ========================================
            CreateMap<CustomOrder, CustomOrderDto>()
                .ForMember(dest => dest.StatusName,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.OldPrice,
                    opt => opt.MapFrom(src => src.Product.OldPrice))
                .ForMember(dest => dest.MainImageUrl,
                    opt => opt.MapFrom(src => src.Product.MainImageUrl))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src => src.Product.Reviews.Any()
                        ? src.Product.Reviews.Average(r => r.Rating)
                        : 0))
                .ForMember(dest => dest.ReviewCount,
                    opt => opt.MapFrom(src => src.Product.Reviews.Count(r => r.IsApproved)))
                .ForMember(dest => dest.IsInStock,
                    opt => opt.MapFrom(src => src.Product.StockQuantity > 0))
                .ForMember(dest => dest.Slug,
                    opt => opt.MapFrom(src => src.Product.Slug));

        }
    }
}
