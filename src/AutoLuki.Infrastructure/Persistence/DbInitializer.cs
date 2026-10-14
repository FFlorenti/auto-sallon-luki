using AutoLuki.Application.Common;
using AutoLuki.Domain;
using AutoLuki.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoLuki.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

        if (configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            logger.LogInformation("Applying database migrations");
            await db.Database.MigrateAsync();
        }

        await SeedRolesAsync(sp.GetRequiredService<RoleManager<IdentityRole>>());
        await SeedAdminAsync(sp.GetRequiredService<UserManager<AdminUser>>(), configuration, logger);
        await SeedBrandsAsync(db);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<AdminUser> userManager, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["AdminSeed:Email"]?.Trim();
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new AdminUser { UserName = email, Email = email, EmailConfirmed = true };
            var created = await userManager.CreateAsync(admin, password);
            if (!created.Succeeded)
            {
                LogRejected(logger, created.Errors);
                return;
            }

            logger.LogInformation("Admin account from configuration created");
        }
        else if (!await userManager.CheckPasswordAsync(admin, password))
        {
            var errors = new List<IdentityError>();
            foreach (var validator in userManager.PasswordValidators)
            {
                errors.AddRange((await validator.ValidateAsync(userManager, admin, password)).Errors);
            }

            if (errors.Count > 0)
            {
                LogRejected(logger, errors);
                return;
            }

            admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, password);
            await userManager.UpdateSecurityStampAsync(admin);
            await userManager.SetLockoutEndDateAsync(admin, null);
            await userManager.ResetAccessFailedCountAsync(admin);
            logger.LogInformation("Admin account password updated from configuration");
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        foreach (var other in await userManager.GetUsersInRoleAsync(Roles.Admin))
        {
            if (other.Id != admin.Id)
            {
                await userManager.DeleteAsync(other);
                logger.LogWarning("Removed admin account {UserId}: it is not the admin from configuration", other.Id);
            }
        }
    }

    private static void LogRejected(ILogger logger, IEnumerable<IdentityError> errors) =>
        logger.LogError(
            "AdminSeed:Password was rejected, so the admin account was NOT created or updated: {Errors}",
            string.Join(" ", errors.Select(e => e.Description)));

    private static async Task SeedBrandsAsync(AppDbContext db)
    {
        if (await db.Brands.AnyAsync())
        {
            return;
        }

        foreach (var (brand, models) in BrandSeedData)
        {
            db.Brands.Add(new Brand
            {
                Name = brand,
                Models = models.Select(m => new Model { Name = m }).ToList(),
            });
        }

        await db.SaveChangesAsync();
    }

    private static readonly Dictionary<string, string[]> BrandSeedData = new()
    {
        ["Audi"] = ["A3", "A4", "A5", "A6", "A8", "Q3", "Q5", "Q7"],
        ["BMW"] = ["Seria 1", "Seria 3", "Seria 5", "Seria 7", "X1", "X3", "X5", "X6"],
        ["Mercedes-Benz"] = ["A-Class", "C-Class", "E-Class", "S-Class", "CLA", "GLC", "GLE", "Vito", "Sprinter"],
        ["Volkswagen"] = ["Golf", "Passat", "Polo", "Jetta", "Tiguan", "Touareg", "Touran", "Caddy", "Arteon"],
        ["Škoda"] = ["Octavia", "Superb", "Fabia", "Kodiaq", "Karoq"],
        ["Opel"] = ["Astra", "Corsa", "Insignia", "Zafira", "Mokka"],
        ["Renault"] = ["Clio", "Megane", "Captur", "Kadjar", "Talisman"],
        ["Peugeot"] = ["208", "308", "508", "2008", "3008", "5008"],
        ["Ford"] = ["Fiesta", "Focus", "Mondeo", "Kuga", "Transit"],
        ["Toyota"] = ["Yaris", "Corolla", "Auris", "RAV4", "C-HR", "Land Cruiser"],
        ["Fiat"] = ["Punto", "500", "Tipo", "Doblo"],
        ["Seat"] = ["Ibiza", "Leon", "Ateca", "Arona"],
        ["Hyundai"] = ["i20", "i30", "Tucson", "Santa Fe"],
        ["Kia"] = ["Ceed", "Sportage", "Sorento", "Rio"],
        ["Dacia"] = ["Sandero", "Logan", "Duster"],
    };
}
