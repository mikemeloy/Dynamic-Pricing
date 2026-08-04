using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Infrastructure;

public class NopStartup : INopStartup
{
    public int Order => 0;

    public void Configure(IApplicationBuilder application)
    {

    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDynamicShoppingCartRepository, DynamicShoppingCartRepository>();
        services.AddScoped<IDynamicPricingRepository, DynamicPricingRepository>();
        services.AddScoped<IDynamicPriceService, DynamicPriceService>();
        services.AddScoped<IDynamicPriceViewModelFactory, ViewModelFactory>();
        services.AddScoped<IMetalsService, MetalsService>();
    }
}
