using BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // đảm bảo DB đã migrate
        await db.Database.MigrateAsync();

        const string adminEmail = "admin@xungxinh.io.vn";
        const string adminPassword = "123456";

        // 1. đảm bảo có role ADMIN
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new Roles { RoleName = "ADMIN" };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
        }

        // 2. kiểm tra admin đã tồn tại chưa
        var adminUser = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (adminUser != null) return;

        // 3. tạo user admin
        var hasher = new PasswordHasher<Users>();

        adminUser = new Users
        {
            Email = adminEmail,
            FullName = "System Admin",
            Status = "ACTIVE"
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

        adminUser.Role.Add(adminRole);

        db.Users.Add(adminUser);
        await db.SaveChangesAsync();
    }
}
