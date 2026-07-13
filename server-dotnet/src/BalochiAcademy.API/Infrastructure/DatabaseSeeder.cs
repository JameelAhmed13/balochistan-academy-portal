using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BalochiAcademy.API.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IApplicationDbContext db, IPasswordService passwords, IConfiguration? config = null)
    {
        try
        {
            // Always sync AI settings from appsettings → DB so the correct Ollama URL
            // and Gemini key are picked up without a manual SQL update.
            if (config != null)
                await SyncAiSettingsAsync(db, config);
            // Seed grade bands independently so adding new bands works even after initial seed
            if (!await db.GradeBands.AnyAsync())
            {
                db.GradeBands.AddRange(
                    new GradeBand { Name = "Primary",   SortOrder = 1 },
                    new GradeBand { Name = "Middle",    SortOrder = 2 },
                    new GradeBand { Name = "Secondary", SortOrder = 3 },
                    new GradeBand { Name = "Higher",    SortOrder = 4 },
                    new GradeBand { Name = "Entrance",  SortOrder = 5 }
                );
                await db.SaveChangesAsync();
            }

            // Seed mediums independently (same pattern as grade bands)
            if (!await db.Mediums.AnyAsync())
            {
                db.Mediums.AddRange(
                    new Medium { Name = "english", Label = "English Medium", SortOrder = 1 },
                    new Medium { Name = "urdu",    Label = "اردو میڈیم",       SortOrder = 2 }
                );
                await db.SaveChangesAsync();
            }

            // Seed professional notification templates (idempotent)
            if (!await db.NotificationTemplates.AnyAsync())
            {
                db.NotificationTemplates.AddRange(
                    // ── Welcome & Onboarding ───────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Welcome to Balochistan Academy Portal",
                        Body  = "Assalamualaikum! Welcome to Balochistan Academy Portal — your complete preparation platform for board exams. Explore AI Tutor, Daily Tests, Video Lectures, and the MCQ Bank. Your journey to academic excellence starts now. Best of luck! 🎓",
                        Type  = "announcement", Icon = "🎉", IsDefault = true,
                    },
                    // ── Daily & Weekly Tests ──────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Daily Practice Test Is Ready",
                        Body  = "Today's daily practice test is now live! Students who attempt daily tests consistently score 40% higher in board exams. Log in, give it your best, and track your progress. Har roz practice, har roz progress! 📝",
                        Type  = "info", Icon = "📝", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Weekly Subject Quiz — Attempt Now",
                        Body  = "This week's competitive subject quiz is live! Challenge students across Balochistan, earn coins, and climb the leaderboard. Top 3 students get bonus rewards. Don't miss out — the quiz closes in 48 hours. 🧠",
                        Type  = "info", Icon = "🧠", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "You Missed Today's Daily Test",
                        Body  = "We noticed you haven't attempted today's daily test yet. Consistency is the key to board exam success! Log in now — the test is still available and takes only 15 minutes. Aap kar saktay hain! 💪",
                        Type  = "warning", Icon = "⚡", IsDefault = true,
                    },
                    // ── Monthly Competition ───────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Monthly Competitive Exam Is Open",
                        Body  = "The monthly mega exam is now open for all students! Compete with thousands of students from across Balochistan. Complete all sections to earn your rank, coins, and certificate. Exam closes on the last day of the month. 🎓",
                        Type  = "announcement", Icon = "🎓", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Monthly Results Are Available",
                        Body  = "Your monthly performance report is now published! Check your subject-wise scores, overall rank, and progress chart on the dashboard. Compare with last month and set new goals. Keep pushing forward! 📊",
                        Type  = "announcement", Icon = "📊", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Leaderboard Has Been Updated",
                        Body  = "The monthly leaderboard has been refreshed! Check where you stand among the top students in your grade. Top performers receive extra coins and certificates. View your rank on the Competition page. 🏅",
                        Type  = "info", Icon = "🏅", IsDefault = true,
                    },
                    // ── Achievements & Coins ──────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "New Achievement Unlocked!",
                        Body  = "Mubarak ho! You have unlocked a new achievement badge. Your hard work and consistency are paying off. Check your profile to see all your badges and keep earning more. Aap ka future roshan hai! 🏆",
                        Type  = "success", Icon = "🏆", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Study Streak — Keep Going!",
                        Body  = "Incredible! You are on a study streak. Students with consistent streaks perform 3x better in board exams. Keep logging in daily, attempting tests, and using AI Tutor. Don't break the chain! 🔥",
                        Type  = "success", Icon = "🔥", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Coins Earned — Check Your Wallet",
                        Body  = "You have earned coins from your recent activity on Balochistan Academy Portal! Open your Coins & Rewards section to check your balance and see available redemption options. Keep learning to earn more! 🪙",
                        Type  = "success", Icon = "🪙", IsDefault = true,
                    },
                    // ── Content & Features ────────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "New Study Material Added",
                        Body  = "Fresh study material has been added to your subject library — including new notes, MCQs, and past paper solutions. Visit the Preparation section to explore the latest content for your grade and medium. 📚",
                        Type  = "announcement", Icon = "📚", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "New Video Lectures Available",
                        Body  = "New video lectures have been uploaded for your subjects! Expert teachers explain complex concepts in simple Urdu and English. Watch, pause, ask AI Tutor follow-up questions, and master every topic. 🎬",
                        Type  = "announcement", Icon = "🎬", IsDefault = true,
                    },
                    // ── Subscription ──────────────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Subscription Expires in 3 Days",
                        Body  = "Your Balochistan Academy Portal subscription expires in 3 days. Renew now to avoid interruption to your AI Tutor, daily tests, video lectures, and competitive exams. Contact us on WhatsApp: 0370-3153540. 💳",
                        Type  = "error", Icon = "💳", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Subscription Has Expired",
                        Body  = "Your subscription has expired. Some premium features are now restricted. Renew your annual plan for just Rs. 999 and get full access to all features. Contact admin or visit the checkout page to renew today. ⚠️",
                        Type  = "error", Icon = "⚠️", IsDefault = true,
                    },
                    // ── Board Exam Prep ───────────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Board Exam Season Is Approaching",
                        Body  = "Board exams are just around the corner! Make the most of Past Papers, MCQ Bank, AI Tutor, and Daily Tests to cover your entire syllabus. Start your revision now — every day counts. Aap tayar hain! 📅",
                        Type  = "warning", Icon = "📅", IsDefault = true,
                    },
                    // ── Platform & Admin ──────────────────────────────────────────────────
                    new NotificationTemplate
                    {
                        Title = "Scheduled Platform Maintenance",
                        Body  = "Balochistan Academy Portal will undergo scheduled maintenance for system improvements. Some features may be temporarily unavailable. We will be back shortly. Thank you for your patience and continued trust. 🔧",
                        Type  = "warning", Icon = "🔧", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Holiday Notice — Platform Available",
                        Body  = "On the occasion of this public holiday, Balochistan Academy Portal remains fully accessible. Use this free time to attempt practice tests, watch video lectures, and review past papers. Keep learning! 🌙",
                        Type  = "announcement", Icon = "🌙", IsDefault = true,
                    },
                    new NotificationTemplate
                    {
                        Title = "Your Complaint Has Been Resolved",
                        Body  = "Thank you for reaching out! Your complaint or suggestion has been reviewed and addressed by our support team. We appreciate your feedback — it helps us make Balochistan Academy Portal better for every student. ✅",
                        Type  = "success", Icon = "✅", IsDefault = true,
                    }
                );
                await db.SaveChangesAsync();
            }

            // Seed subscription plans (idempotent)
            if (!await db.SubscriptionPlans.AnyAsync())
            {
                db.SubscriptionPlans.AddRange(
                    new SubscriptionPlan
                    {
                        Name         = "Free Trial",
                        Description  = "7-day free access to explore the platform — no payment required.",
                        Price        = 0m,
                        DurationDays = 7,
                        AiTokenQuota = 20,
                        IsActive     = true,
                        SortOrder    = 1,
                    },
                    new SubscriptionPlan
                    {
                        Name         = "Monthly Plan",
                        Description  = "Full access for 30 days — all subjects, daily tests, AI Tutor, and video lectures.",
                        Price        = 299m,
                        DurationDays = 30,
                        AiTokenQuota = 150,
                        IsActive     = true,
                        SortOrder    = 2,
                    },
                    new SubscriptionPlan
                    {
                        Name         = "Annual Plan",
                        Description  = "Best value — 365 days of unlimited access with maximum AI Tutor tokens.",
                        Price        = 999m,
                        DurationDays = 365,
                        AiTokenQuota = 1000,
                        IsActive     = true,
                        SortOrder    = 3,
                    }
                );
                await db.SaveChangesAsync();
            }

            // Always ensure all expected settings keys exist (safe to re-run on existing DB)
            await EnsureSystemSettingsAsync(db);

            // Seed past papers once subjects exist in DB
            if (!await db.PastPapers.AnyAsync() && await db.Subjects.AnyAsync())
                await SeedPastPapersAsync(db);

            // Idempotent: skip if any roles already exist
            if (await db.Roles.AnyAsync())
            {
                // Catch the case where roles existed but past papers weren't seeded yet
                if (!await db.PastPapers.AnyAsync() && await db.Subjects.AnyAsync())
                    await SeedPastPapersAsync(db);
                return;
            }

            // ── STEP 1: Roles ─────────────────────────────────────────────────────
            var admin = new Role { Name = "admin", Description = "Administrator — full system access" };
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
            new Permission { Name = "coins.manage",       Module = "Coins",         Description = "Manage coin economy settings and approve pending cash subscriptions" },
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
                RoleId = admin.Id,
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
            var math = new Subject { Name = "Mathematics", NameUr = "ریاضی", Icon = "🧮", Color = "grad-blue" };
            var physics = new Subject { Name = "Physics", NameUr = "طبیعیات", Icon = "⚛️", Color = "grad-violet" };
            var chem = new Subject { Name = "Chemistry", NameUr = "کیمیا", Icon = "⚗️", Color = "grad-emerald" };
            var bio = new Subject { Name = "Biology", NameUr = "حیاتیات", Icon = "🌿", Color = "grad-green" };
            var english = new Subject { Name = "English", NameUr = "انگریزی", Icon = "📖", Color = "grad-amber" };
            var urdu = new Subject { Name = "Urdu", NameUr = "اردو", Icon = "✒️", Color = "grad-rose" };
            var islamiat = new Subject { Name = "Islamiat", NameUr = "اسلامیات", Icon = "🌙", Color = "grad-teal" };
            var pakst = new Subject { Name = "Pakistan Studies", NameUr = "مطالعہ پاکستان", Icon = "🗺️", Color = "grad-orange" };
            var cs = new Subject { Name = "Computer Science", NameUr = "کمپیوٹر سائنس", Icon = "🖥️", Color = "grad-violet" };
            var genSci = new Subject { Name = "General Science", NameUr = "عمومی سائنس", Icon = "🔬", Color = "grad-cyan" };
            db.Subjects.AddRange(math, physics, chem, bio, english, urdu, islamiat, pakst, cs, genSci);

            // ── STEP 6: Users (Admin + Demo Student) ──────────────────────────────
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = passwords.Hash("Admin@123"),
                RoleId = admin.Id,
                Name = "System Administrator",
                IsActive = true,
            });
            // Demo student — useful for testing without full registration flow
            // Grade assigned after grades are saved; uses "9" (saved below)
            var demoStudent = new User
            {
                Username = "demo_student",
                PasswordHash = passwords.Hash("Student@123"),
                RoleId = student.Id,
                Name = "Demo Student",
                GradeCode = "9",
                Medium = "English",
                IsActive = true,
            };
            db.Users.Add(demoStudent);

            // ── STEP 7: System Settings ───────────────────────────────────────────
            // Values match the hardcoded defaults in CoinsController
            var settings = new[]
            {
            new SystemSetting { Key = "site_name",         Value = "Balochistan Academy Portal", Description = "Application display name" },
            new SystemSetting { Key = "coin_rate_pkr",     Value = "0.005", Description = "PKR value per coin (200 coins = 1 PKR) — prices subscriptions/tokens, not cash" },
            new SystemSetting { Key = "coins_per_ai_token",Value = "20",    Description = "Coin cost of one extra AI token bought outside a plan's quota" },
            new SystemSetting { Key = "ai_model",          Value = "gemini-2.5-flash-lite", Description = "Default AI model for tutoring" },
            new SystemSetting { Key = "registration_open", Value = "true",  Description = "Allow new student registrations" },
            new SystemSetting { Key = "coins_per_90pct",   Value = "50",    Description = "Coins awarded for 90%+ test score" },
            new SystemSetting { Key = "coins_per_70pct",   Value = "30",    Description = "Coins awarded for 70–89% test score" },
            new SystemSetting { Key = "coins_per_50pct",   Value = "15",    Description = "Coins awarded for 50–69% test score" },
            new SystemSetting { Key = "coins_per_pass",    Value = "5",     Description = "Coins awarded for below 50% but attempted" },
            // Panel 1 — General Platform
            new SystemSetting { Key = "institute_name",    Value = "UltraSoft Systems",  Description = "Official institute/organization name" },
            new SystemSetting { Key = "support_whatsapp",  Value = "+923703153540",      Description = "Support WhatsApp number (with country code)" },
            new SystemSetting { Key = "allowed_grades",    Value = "",                   Description = "Comma-separated grade codes visible to students (empty = all enabled)" },
            // Panel 2 — Subscription
            new SystemSetting { Key = "trial_days",                      Value = "7",     Description = "Free trial duration in days" },
            new SystemSetting { Key = "subscription_auto_expire",        Value = "true",  Description = "Automatically expire subscriptions past EndDate" },
            new SystemSetting { Key = "subscription_activate_on_payment",Value = "false", Description = "Activate subscription immediately on payment confirmation" },
            // Panel 3 — Coin Economy
            new SystemSetting { Key = "min_questions_for_eligibility", Value = "50",  Description = "Minimum total questions in a test for coin eligibility" },
            new SystemSetting { Key = "daily_login_bonus",             Value = "2",   Description = "Coins awarded for each daily login" },
            // Panel 4 — AI & Features
            new SystemSetting { Key = "gemini_api_key",       Value = "",                    Description = "Google Gemini API key (overrides .env VITE_GEMINI_API_KEY)" },
            new SystemSetting { Key = "gemini_model",         Value = "gemini-2.5-flash-lite",Description = "Gemini model name used for AI tutoring and grading" },
            new SystemSetting { Key = "tts_api_key",          Value = "",                    Description = "Google Cloud TTS API key" },
            new SystemSetting { Key = "ollama_url",           Value = "http://localhost:11434", Description = "Ollama server base URL" },
            new SystemSetting { Key = "ollama_model",         Value = "llama3",              Description = "Ollama model name" },
            new SystemSetting { Key = "ollama_timeout_sec",   Value = "30",                  Description = "Ollama request timeout in seconds" },
            new SystemSetting { Key = "ai_default_language",  Value = "both",                Description = "Default AI response language: english | urdu | both" },
            new SystemSetting { Key = "ai_tutor_enabled",     Value = "true",                Description = "Enable AI Tutor feature for students" },
            new SystemSetting { Key = "video_lessons_enabled",Value = "true",                Description = "Enable Video Lessons feature for students" },
            new SystemSetting { Key = "ai_grading_enabled",   Value = "true",                Description = "Enable AI-powered grading" },
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

            var primary = new[] { math, english, urdu, islamiat, pakst, genSci };
            var middle = new[] { math, english, urdu, islamiat, pakst, genSci, cs };
            var secondary = new[] { math, physics, chem, bio, english, urdu, islamiat, pakst, cs };
            var entry = new[] { math, physics, chem, bio, english };

            var gradeSubjects = new List<GradeSubject>();
            void AssignSubjects(string gradeCode, Subject[] subs)
                => gradeSubjects.AddRange(subs.Select(s => new GradeSubject { GradeCode = gradeCode, SubjectId = s.Id }));

            AssignSubjects("1", primary);
            AssignSubjects("2", primary);
            AssignSubjects("3", primary);
            AssignSubjects("4", primary);
            AssignSubjects("5", primary);
            AssignSubjects("6", middle);
            AssignSubjects("7", middle);
            AssignSubjects("8", middle);
            AssignSubjects("9", secondary);
            AssignSubjects("10", secondary);
            AssignSubjects("11", secondary);
            AssignSubjects("12", secondary);
            AssignSubjects("entry", entry);

            db.GradeSubjects.AddRange(gradeSubjects);

            // ── STEP 9: AI Tutors ─────────────────────────────────────────────────
            db.AiTutors.AddRange(
                new AiTutor
                {
                    SubjectId = math.Id,
                    Persona = "Rafi",
                    Slug = "rafi-math",
                    Icon = "🧮",
                    Color = "grad-blue",
                    Description = "Your Mathematics tutor — algebra, geometry, calculus made simple.",
                    SystemPrompt =
                        "You are Rafi, a friendly and patient Mathematics tutor for Pakistani students. " +
                        "Explain concepts step-by-step using clear examples. Use simple language. " +
                        "When solving problems, show each step. Encourage the student.",
                    IsActive = true,
                },
                new AiTutor
                {
                    SubjectId = physics.Id,
                    Persona = "Zainab",
                    Slug = "zainab-physics",
                    Icon = "⚛️",
                    Color = "grad-violet",
                    Description = "Physics tutor — from mechanics to electromagnetism, made intuitive.",
                    SystemPrompt =
                        "You are Zainab, an enthusiastic Physics tutor. Break down complex physics concepts " +
                        "with real-world examples from everyday life in Pakistan. Use diagrams described in text " +
                        "when helpful. Focus on understanding, not just formulas.",
                    IsActive = true,
                },
                new AiTutor
                {
                    SubjectId = chem.Id,
                    Persona = "Dr. Karim",
                    Slug = "dr-karim-chemistry",
                    Icon = "⚗️",
                    Color = "grad-emerald",
                    Description = "Chemistry tutor — reactions, bonding, and periodic table mastery.",
                    SystemPrompt =
                        "You are Dr. Karim, a Chemistry tutor with 20 years of teaching experience. " +
                        "Explain chemical reactions with clear analogies. Help students memorize the periodic table " +
                        "with memory tricks. Always link theory to real-world applications.",
                    IsActive = true,
                },
                new AiTutor
                {
                    SubjectId = bio.Id,
                    Persona = "Nadia",
                    Slug = "nadia-biology",
                    Icon = "🌿",
                    Color = "grad-green",
                    Description = "Biology tutor — cells, genetics, ecosystems and human body systems.",
                    SystemPrompt =
                        "You are Nadia, a Biology tutor who makes life sciences fascinating. " +
                        "Explain biological processes with vivid descriptions. Connect concepts to human health " +
                        "and the natural environment of Balochistan. Use analogies for complex mechanisms.",
                    IsActive = true,
                },
                new AiTutor
                {
                    SubjectId = english.Id,
                    Persona = "Sara",
                    Slug = "sara-english",
                    Icon = "📖",
                    Color = "grad-amber",
                    Description = "English Language & Literature tutor — grammar, writing, and comprehension.",
                    SystemPrompt =
                        "You are Sara, an English Language tutor. Help students improve grammar, vocabulary, " +
                        "essay writing, and reading comprehension. Give examples relevant to Pakistani students. " +
                        "Be encouraging and correct mistakes gently with explanations.",
                    IsActive = true,
                },
                new AiTutor
                {
                    SubjectId = null,
                    GradeCode = null,
                    Persona = "Ustad",
                    Slug = "ustad-general",
                    Icon = "🎓",
                    Color = "grad-violet",
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
        catch
        {

        }
    }

    private static async Task EnsureSystemSettingsAsync(IApplicationDbContext db)
    {
        var defaults = new Dictionary<string, (string Value, string Description)>
        {
            ["site_name"]                        = ("Balochistan Academy Portal",  "Application display name"),
            ["coin_rate_pkr"]                    = ("0.005",                       "PKR value per coin (200 coins = 1 PKR)"),
            ["coins_per_ai_token"]               = ("20",                          "Coin cost of one extra AI token bought outside a plan's quota"),
            ["ai_model"]                         = ("gemini-2.5-flash-lite",       "Default AI model for tutoring"),
            ["registration_open"]                = ("true",                        "Allow new student registrations"),
            ["coins_per_90pct"]                  = ("50",                          "Coins awarded for 90%+ test score"),
            ["coins_per_70pct"]                  = ("30",                          "Coins awarded for 70–89% test score"),
            ["coins_per_50pct"]                  = ("15",                          "Coins awarded for 50–69% test score"),
            ["coins_per_pass"]                   = ("5",                           "Coins awarded for below 50% but attempted"),
            ["institute_name"]                   = ("UltraSoft Systems",           "Official institute/organization name"),
            ["support_whatsapp"]                 = ("+923703153540",               "Support WhatsApp number (with country code)"),
            ["allowed_grades"]                   = ("",                            "Comma-separated grade codes visible to students (empty = all enabled)"),
            ["trial_days"]                       = ("7",                           "Free trial duration in days"),
            ["subscription_auto_expire"]         = ("true",                        "Automatically expire subscriptions past EndDate"),
            ["subscription_activate_on_payment"] = ("false",                       "Activate subscription immediately on payment confirmation"),
            ["min_questions_for_eligibility"]    = ("50",                          "Minimum total questions in a test for coin eligibility"),
            ["daily_login_bonus"]                = ("2",                           "Coins awarded for each daily login"),
            ["gemini_api_key"]                   = ("",                            "Google Gemini API key"),
            ["gemini_model"]                     = ("gemini-2.5-flash-lite",       "Gemini model name used for AI tutoring and grading"),
            ["tts_api_key"]                      = ("",                            "Google Cloud TTS API key"),
            ["ollama_url"]                       = ("http://localhost:11434",      "Ollama server base URL"),
            ["ollama_model"]                     = ("llama3",                      "Ollama model name"),
            ["ollama_timeout_sec"]               = ("30",                          "Ollama request timeout in seconds"),
            ["ai_default_language"]              = ("both",                        "Default AI response language: english | urdu | both"),
            ["ai_tutor_enabled"]                 = ("true",                        "Enable AI Tutor feature for students"),
            ["video_lessons_enabled"]            = ("true",                        "Enable Video Lessons feature for students"),
            ["ai_grading_enabled"]               = ("true",                        "Enable AI-powered grading"),
        };

        var existing = await db.SystemSettings.Select(s => s.Key).ToHashSetAsync();
        var missing  = defaults.Where(kv => !existing.Contains(kv.Key)).ToList();
        if (missing.Count == 0) return;

        foreach (var (key, (value, desc)) in missing)
            db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Description = desc });

        await db.SaveChangesAsync();
    }

    private static async Task SeedPastPapersAsync(IApplicationDbContext db)
    {
        // Look up subject IDs by name (subjects must already be in the DB).
        var subjectMap = await db.Subjects
            .Select(s => new { s.Name, s.Id })
            .ToDictionaryAsync(s => s.Name, s => s.Id);

        if (subjectMap.Count == 0) return;

        // Subjects that have real Balochistan Board exam papers.
        var subjectNames = new[]
        {
            "Mathematics", "Physics", "Chemistry", "Biology",
            "English", "Urdu", "Islamiat", "Pakistan Studies", "Computer Science",
        };

        // Grades with board exams.
        var gradeYears = new Dictionary<string, int[]>
        {
            ["9"]  = [2018, 2019, 2020, 2021, 2022, 2023, 2024],
            ["10"] = [2018, 2019, 2020, 2021, 2022, 2023, 2024],
        };

        var papers = new List<PastPaper>();
        int order = 0;

        foreach (var (grade, years) in gradeYears)
        foreach (var subjectName in subjectNames)
        {
            if (!subjectMap.TryGetValue(subjectName, out var sid)) continue;
            foreach (var year in years)
            {
                // Annual paper — every year.
                papers.Add(new PastPaper
                {
                    SubjectId = sid, GradeCode = grade, Year = year,
                    Board = "Balochistan Board (BISE)", PaperType = "Annual",
                    TotalMarks = 75, TimeLimitMinutes = 180, SortOrder = order++,
                });

                // Supplementary paper — available for 2018–2023.
                if (year <= 2023)
                    papers.Add(new PastPaper
                    {
                        SubjectId = sid, GradeCode = grade, Year = year,
                        Board = "Balochistan Board (BISE)", PaperType = "Supplementary",
                        TotalMarks = 75, TimeLimitMinutes = 180, SortOrder = order++,
                    });
            }
        }

        db.PastPapers.AddRange(papers);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Upserts AI infrastructure settings from appsettings.json into SystemSettings so the
    /// correct Ollama URL and Gemini key are active without a manual SQL UPDATE.
    /// Only updates keys whose appsettings value is non-empty.
    /// </summary>
    private static async Task SyncAiSettingsAsync(IApplicationDbContext db, IConfiguration config)
    {
        var sync = new Dictionary<string, string>
        {
            ["ollama_url"]         = config["Ai:OllamaUrl"]         ?? "",
            ["ollama_model"]       = config["Ai:OllamaModel"]       ?? "",
            ["ollama_timeout_sec"] = config["Ai:OllamaTimeoutSec"]  ?? "",
            ["gemini_api_key"]     = config["Ai:GeminiApiKey"]      ?? "",
            ["gemini_model"]       = config["Ai:GeminiModel"]       ?? "",
            ["tts_api_key"]        = config["Ai:TtsApiKey"]         ?? "",
        };

        // Only push keys that have a value in config.
        var toSync = sync.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)).ToList();
        if (toSync.Count == 0) return;

        var keys     = toSync.Select(kv => kv.Key).ToList();
        var existing = await db.SystemSettings
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key);

        foreach (var (key, value) in toSync)
        {
            if (existing.TryGetValue(key, out var row))
                row.Value = value;
            else
                db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Description = $"Synced from appsettings — {key}" });
        }

        await db.SaveChangesAsync();
    }
}
