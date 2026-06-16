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

        // ── STEP 1: Roles ─────────────────────────────────────────────────────
        var admin   = new Role { Name = "admin",   Description = "Administrator — full system access" };
        var student = new Role { Name = "student", Description = "Student — learning and assessment" };
        var teacher = new Role { Name = "teacher", Description = "Teacher — content creation" };
        db.Roles.AddRange(admin, student, teacher);
        await db.SaveChangesAsync();  // flush to get Role IDs

        // ── STEP 2: Permissions ───────────────────────────────────────────────
        var permissions = new[]
        {
            new Permission { Name = "questions.manage",   Module = "Questions",     Description = "Create, edit, delete questions" },
            new Permission { Name = "tests.manage",       Module = "Tests",         Description = "Create, publish, delete tests" },
            new Permission { Name = "users.manage",       Module = "Users",         Description = "View and toggle user accounts" },
            new Permission { Name = "content.manage",     Module = "Content",       Description = "Upload and manage content items" },
            new Permission { Name = "coins.manage",       Module = "Coins",         Description = "Process withdrawal requests" },
            new Permission { Name = "notifications.send", Module = "Notifications", Description = "Broadcast notifications to users" },
            new Permission { Name = "complaints.manage",  Module = "Complaints",    Description = "Reply to and resolve complaints" },
            new Permission { Name = "catalog.manage",     Module = "Catalog",       Description = "Manage grades, subjects, syllabus" },
            new Permission { Name = "settings.manage",    Module = "Settings",      Description = "Update system configuration" },
            new Permission { Name = "reports.view",       Module = "Reports",       Description = "View analytics and dashboard KPIs" },
        };
        db.Permissions.AddRange(permissions);
        await db.SaveChangesAsync();  // flush to get Permission IDs

        // ── STEP 3: Role → Permission assignments ─────────────────────────────
        // Admin role gets ALL permissions
        db.RolePermissions.AddRange(permissions.Select(p => new RolePermission
        {
            RoleId       = admin.Id,
            PermissionId = p.Id,
        }));

        // Teacher role gets content + questions + tests (read/create only by convention)
        var teacherPermNames = new[] { "questions.manage", "tests.manage", "content.manage" };
        db.RolePermissions.AddRange(permissions
            .Where(p => teacherPermNames.Contains(p.Name))
            .Select(p => new RolePermission { RoleId = teacher.Id, PermissionId = p.Id }));

        // ── STEP 4: Grades ────────────────────────────────────────────────────
        var grades = new[]
        {
            new Grade { Code = "1",     Label = "Grade 1",    Band = "Primary",   SortOrder = 1  },
            new Grade { Code = "2",     Label = "Grade 2",    Band = "Primary",   SortOrder = 2  },
            new Grade { Code = "3",     Label = "Grade 3",    Band = "Primary",   SortOrder = 3  },
            new Grade { Code = "4",     Label = "Grade 4",    Band = "Primary",   SortOrder = 4  },
            new Grade { Code = "5",     Label = "Grade 5",    Band = "Primary",   SortOrder = 5  },
            new Grade { Code = "6",     Label = "Grade 6",    Band = "Middle",    SortOrder = 6  },
            new Grade { Code = "7",     Label = "Grade 7",    Band = "Middle",    SortOrder = 7  },
            new Grade { Code = "8",     Label = "Grade 8",    Band = "Middle",    SortOrder = 8  },
            new Grade { Code = "9",     Label = "Grade 9",    Band = "Secondary", SortOrder = 9  },
            new Grade { Code = "10",    Label = "Grade 10",   Band = "Secondary", SortOrder = 10 },
            new Grade { Code = "11",    Label = "Grade 11",   Band = "Higher",    SortOrder = 11 },
            new Grade { Code = "12",    Label = "Grade 12",   Band = "Higher",    SortOrder = 12 },
            new Grade { Code = "entry", Label = "Entry Test", Band = "Entrance",  SortOrder = 13 },
        };
        db.Grades.AddRange(grades);

        // ── STEP 5: Subjects ──────────────────────────────────────────────────
        var math     = new Subject { Name = "Mathematics",      NameUr = "ریاضی",              Icon = "calculator",  Color = "#3B82F6" };
        var physics  = new Subject { Name = "Physics",          NameUr = "طبیعیات",            Icon = "atom",        Color = "#8B5CF6" };
        var chem     = new Subject { Name = "Chemistry",        NameUr = "کیمیا",              Icon = "flask",       Color = "#10B981" };
        var bio      = new Subject { Name = "Biology",          NameUr = "حیاتیات",            Icon = "leaf",        Color = "#22C55E" };
        var english  = new Subject { Name = "English",          NameUr = "انگریزی",            Icon = "book-open",   Color = "#F59E0B" };
        var urdu     = new Subject { Name = "Urdu",             NameUr = "اردو",               Icon = "pen",         Color = "#EF4444" };
        var islamiat = new Subject { Name = "Islamiat",         NameUr = "اسلامیات",           Icon = "moon",        Color = "#14B8A6" };
        var pakst    = new Subject { Name = "Pakistan Studies", NameUr = "مطالعہ پاکستان",     Icon = "map",         Color = "#F97316" };
        var cs       = new Subject { Name = "Computer Science", NameUr = "کمپیوٹر سائنس",     Icon = "monitor",     Color = "#6366F1" };
        var genSci   = new Subject { Name = "General Science",  NameUr = "عمومی سائنس",        Icon = "microscope",  Color = "#0EA5E9" };
        db.Subjects.AddRange(math, physics, chem, bio, english, urdu, islamiat, pakst, cs, genSci);

        // ── STEP 6: Users (Admin + Demo Student) ──────────────────────────────
        db.Users.Add(new User
        {
            Username     = "admin",
            PasswordHash = passwords.Hash("Admin@123"),
            RoleId       = admin.Id,
            Name         = "System Administrator",
            IsActive     = true,
        });
        // Demo student — useful for testing without full registration flow
        // Grade assigned after grades are saved; uses "9" (saved below)
        var demoStudent = new User
        {
            Username     = "demo_student",
            PasswordHash = passwords.Hash("Student@123"),
            RoleId       = student.Id,
            Name         = "Demo Student",
            GradeCode    = "9",
            Medium       = "English",
            IsActive     = true,
        };
        db.Users.Add(demoStudent);

        // ── STEP 7: System Settings ───────────────────────────────────────────
        // Values match the hardcoded defaults in CoinsController
        var settings = new[]
        {
            new SystemSetting { Key = "site_name",         Value = "Balochistan Academy Portal", Description = "Application display name" },
            new SystemSetting { Key = "coin_rate_pkr",     Value = "0.50",  Description = "PKR value per coin (1 coin = 0.50 PKR)" },
            new SystemSetting { Key = "min_withdrawal",    Value = "300",   Description = "Minimum coins required to request withdrawal" },
            new SystemSetting { Key = "ai_model",          Value = "gemini-2.5-flash-lite", Description = "Default AI model for tutoring" },
            new SystemSetting { Key = "registration_open", Value = "true",  Description = "Allow new student registrations" },
            new SystemSetting { Key = "coins_per_90pct",   Value = "50",    Description = "Coins awarded for 90%+ test score" },
            new SystemSetting { Key = "coins_per_70pct",   Value = "30",    Description = "Coins awarded for 70–89% test score" },
            new SystemSetting { Key = "coins_per_50pct",   Value = "15",    Description = "Coins awarded for 50–69% test score" },
            new SystemSetting { Key = "coins_per_pass",    Value = "5",     Description = "Coins awarded for below 50% but attempted" },
        };
        db.SystemSettings.AddRange(settings);

        await db.SaveChangesAsync();  // flush to get Subject IDs

        // ── STEP 8: Grade–Subject assignments ────────────────────────────────
        //
        // Primary (1–5): Math, English, Urdu, Islamiat, Pakistan Studies, General Science
        // Middle (6–8):  Primary + Computer Science
        // Secondary (9–10): Math, Physics, Chemistry, Biology, English, Urdu, Islamiat,
        //                   Pakistan Studies, Computer Science
        // Higher (11–12): same as Secondary
        // Entry Test:    Math, Physics, Chemistry, Biology, English

        var primary   = new[] { math, english, urdu, islamiat, pakst, genSci };
        var middle    = new[] { math, english, urdu, islamiat, pakst, genSci, cs };
        var secondary = new[] { math, physics, chem, bio, english, urdu, islamiat, pakst, cs };
        var entry     = new[] { math, physics, chem, bio, english };

        var gradeSubjects = new List<GradeSubject>();
        void AssignSubjects(string gradeCode, Subject[] subs)
            => gradeSubjects.AddRange(subs.Select(s => new GradeSubject { GradeCode = gradeCode, SubjectId = s.Id }));

        AssignSubjects("1",     primary);
        AssignSubjects("2",     primary);
        AssignSubjects("3",     primary);
        AssignSubjects("4",     primary);
        AssignSubjects("5",     primary);
        AssignSubjects("6",     middle);
        AssignSubjects("7",     middle);
        AssignSubjects("8",     middle);
        AssignSubjects("9",     secondary);
        AssignSubjects("10",    secondary);
        AssignSubjects("11",    secondary);
        AssignSubjects("12",    secondary);
        AssignSubjects("entry", entry);

        db.GradeSubjects.AddRange(gradeSubjects);

        // ── STEP 9: AI Tutors ─────────────────────────────────────────────────
        db.AiTutors.AddRange(
            new AiTutor
            {
                SubjectId   = math.Id,
                Persona     = "Rafi",
                Slug        = "rafi-math",
                Icon        = "calculator",
                Color       = "#3B82F6",
                Description = "Your Mathematics tutor — algebra, geometry, calculus made simple.",
                SystemPrompt =
                    "You are Rafi, a friendly and patient Mathematics tutor for Pakistani students. " +
                    "Explain concepts step-by-step using clear examples. Use simple language. " +
                    "When solving problems, show each step. Encourage the student.",
                IsActive = true,
            },
            new AiTutor
            {
                SubjectId   = physics.Id,
                Persona     = "Zainab",
                Slug        = "zainab-physics",
                Icon        = "atom",
                Color       = "#8B5CF6",
                Description = "Physics tutor — from mechanics to electromagnetism, made intuitive.",
                SystemPrompt =
                    "You are Zainab, an enthusiastic Physics tutor. Break down complex physics concepts " +
                    "with real-world examples from everyday life in Pakistan. Use diagrams described in text " +
                    "when helpful. Focus on understanding, not just formulas.",
                IsActive = true,
            },
            new AiTutor
            {
                SubjectId   = chem.Id,
                Persona     = "Dr. Karim",
                Slug        = "dr-karim-chemistry",
                Icon        = "flask",
                Color       = "#10B981",
                Description = "Chemistry tutor — reactions, bonding, and periodic table mastery.",
                SystemPrompt =
                    "You are Dr. Karim, a Chemistry tutor with 20 years of teaching experience. " +
                    "Explain chemical reactions with clear analogies. Help students memorize the periodic table " +
                    "with memory tricks. Always link theory to real-world applications.",
                IsActive = true,
            },
            new AiTutor
            {
                SubjectId   = bio.Id,
                Persona     = "Nadia",
                Slug        = "nadia-biology",
                Icon        = "leaf",
                Color       = "#22C55E",
                Description = "Biology tutor — cells, genetics, ecosystems and human body systems.",
                SystemPrompt =
                    "You are Nadia, a Biology tutor who makes life sciences fascinating. " +
                    "Explain biological processes with vivid descriptions. Connect concepts to human health " +
                    "and the natural environment of Balochistan. Use analogies for complex mechanisms.",
                IsActive = true,
            },
            new AiTutor
            {
                SubjectId   = english.Id,
                Persona     = "Sara",
                Slug        = "sara-english",
                Icon        = "book-open",
                Color       = "#F59E0B",
                Description = "English Language & Literature tutor — grammar, writing, and comprehension.",
                SystemPrompt =
                    "You are Sara, an English Language tutor. Help students improve grammar, vocabulary, " +
                    "essay writing, and reading comprehension. Give examples relevant to Pakistani students. " +
                    "Be encouraging and correct mistakes gently with explanations.",
                IsActive = true,
            },
            new AiTutor
            {
                SubjectId   = null,
                GradeCode   = null,
                Persona     = "Ustad",
                Slug        = "ustad-general",
                Icon        = "graduation-cap",
                Color       = "#6366F1",
                Description = "General knowledge tutor — Islamiat, Pakistan Studies, and exam strategy.",
                SystemPrompt =
                    "You are Ustad, a general-purpose academic tutor for Pakistani students. " +
                    "You cover Islamiat, Pakistan Studies, current affairs, and general exam strategy. " +
                    "Be culturally sensitive, respectful, and supportive. Motivate students and help " +
                    "with study planning and time management.",
                IsActive = true,
            }
        );

        await db.SaveChangesAsync();
    }
}
