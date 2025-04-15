using Application.Interfaces;
using Application.Services;
using Common.Helpers;
using Domain.Validators;
using FluentValidation;

namespace Web.Configs
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ICsvService, CsvService>();

            services.AddValidatorsFromAssemblyContaining<CategoryValidator>();
            services.AddValidatorsFromAssemblyContaining<ClientValidator>();
            services.AddValidatorsFromAssemblyContaining<OrderItemValidator>();
            services.AddValidatorsFromAssemblyContaining<OrderValidator>();
            services.AddValidatorsFromAssemblyContaining<ProductValidator>();
            services.AddValidatorsFromAssemblyContaining<ImageValidator>();

            return services;
        }
    }
}
