using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IApplicationDbContext db, IPasswordService passwords)
    {
        // Idempotent: skip if any roles already exist
        if (await db.Roles.AnyAsync()) return;

        // ── Roles ──────────────────────────────────────────────────────────
        var admin   = new Role { Name = "admin",   Description = "Administrator" };
        var student = new Role { Name = "student", Description = "Student" };
        var teacher = new Role { Name = "teacher", Description = "Teacher" };
        db.Roles.AddRange(admin, student, teacher);
        await db.SaveChangesAsync();

        // ── Admin user ─────────────────────────────────────────────────────
        db.Users.Add(new User
        {
            Username     = "admin",
            PasswordHash = passwords.Hash("Admin@123"),
            RoleId       = admin.Id,
            Name         = "System Administrator",
            IsActive     = true,
        });

        // ── Grades (Primary 1–5, Middle 6–8, Secondary 9–10, Higher 11–12, Entry) ──
        var grades = new[]
        {
            new Grade { Code = "1",  Label = "Grade 1",  Band = "Primary",   SortOrder = 1 },
            new Grade { Code = "2",  Label = "Grade 2",  Band = "Primary",   SortOrder = 2 },
            new Grade { Code = "3",  Label = "Grade 3",  Band = "Primary",   SortOrder = 3 },
            new Grade { Code = "4",  Label = "Grade 4",  Band = "Primary",   SortOrder = 4 },
            new Grade { Code = "5",  Label = "Grade 5",  Band = "Primary",   SortOrder = 5 },
            new Grade { Code = "6",  Label = "Grade 6",  Band = "Middle",    SortOrder = 6 },
            new Grade { Code = "7",  Label = "Grade 7",  Band = "Middle",    SortOrder = 7 },
            new Grade { Code = "8",  Label = "Grade 8",  Band = "Middle",    SortOrder = 8 },
            new Grade { Code = "9",  Label = "Grade 9",  Band = "Secondary", SortOrder = 9 },
            new Grade { Code = "10", Label = "Grade 10", Band = "Secondary",  SortOrder = 10 },
            new Grade { Code = "11", Label = "Grade 11", Band = "Higher",    SortOrder = 11 },
            new Grade { Code = "12", Label = "Grade 12", Band = "Higher",    SortOrder = 12 },
            new Grade { Code = "entry", Label = "Entry Test", Band = "Entrance", SortOrder = 13 },
        };
        db.Grades.AddRange(grades);

        // ── Subjects ───────────────────────────────────────────────────────
        var subjects = new[]
        {
            new Subject { Name = "Mathematics",      NameUr = "ریاضی",      Icon = "calculator",    Color = "#3B82F6" },
            new Subject { Name = "Physics",          NameUr = "طبیعیات",    Icon = "atom",          Color = "#8B5CF6" },
            new Subject { Name = "Chemistry",        NameUr = "کیمیا",      Icon = "flask",         Color = "#10B981" },
            new Subject { Name = "Biology",          NameUr = "حیاتیات",    Icon = "leaf",          Color = "#22C55E" },
            new Subject { Name = "English",          NameUr = "انگریزی",    Icon = "book-open",     Color = "#F59E0B" },
            new Subject { Name = "Urdu",             NameUr = "اردو",       Icon = "pen",           Color = "#EF4444" },
            new Subject { Name = "Islamiat",         NameUr = "اسلامیات",   Icon = "moon",          Color = "#14B8A6" },
            new Subject { Name = "Pakistan Studies", NameUr = "مطالعہ پاکستان", Icon = "map",      Color = "#F97316" },
            new Subject { Name = "Computer Science", NameUr = "کمپیوٹر",   Icon = "monitor",       Color = "#6366F1" },
            new Subject { Name = "General Science",  NameUr = "عمومی سائنس", Icon = "microscope",  Color = "#0EA5E9" },
        };
        db.Subjects.AddRange(subjects);

        // ── System settings ────────────────────────────────────────────────
        var settings = new[]
        {
            new SystemSetting { Key = "site_name",        Value = "Balochistan Academy Portal", Description = "Application display name" },
            new SystemSetting { Key = "coin_rate_pkr",    Value = "0.10",   Description = "PKR value per coin" },
            new SystemSetting { Key = "min_withdrawal",   Value = "500",    Description = "Minimum coins to withdraw" },
            new SystemSetting { Key = "ai_model",         Value = "gemini-2.5-flash-lite", Description = "Default AI model" },
            new SystemSetting { Key = "registration_open",Value = "true",   Description = "Allow new student registrations" },
        };
        db.SystemSettings.AddRange(settings);

        await db.SaveChangesAsync();
    }
}
