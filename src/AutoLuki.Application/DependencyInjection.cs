using AutoLuki.Application.Brands;
using AutoLuki.Application.Cars;
using AutoLuki.Application.Cars.Photos;
using AutoLuki.Application.Dashboard;
using AutoLuki.Application.Inquiries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AutoLuki.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICarService, CarService>();
        services.AddScoped<ICarPhotoService, CarPhotoService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IInquiryService, InquiryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
