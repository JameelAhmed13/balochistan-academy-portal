-- ============================================================
-- BalochiAcademyDB — MS SQL Server 2019+
-- Balochistan Academy Portal
-- Generated: 2026-06-15
-- Architecture: Clean Architecture (.NET 10 Web API)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BalochiAcademyDB')
    CREATE DATABASE BalochiAcademyDB
    COLLATE Arabic_CI_AS;   -- supports Urdu/Arabic text natively
GO

USE BalochiAcademyDB;
GO

-- ============================================================
-- SECTION 1: ROLES & PERMISSIONS (RBAC)
-- ============================================================

CREATE TABLE Roles (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(255),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);

CREATE TABLE Permissions (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,   -- e.g. 'questions.create', 'tests.publish'
    Module      NVARCHAR(50)  NOT NULL,   -- e.g. 'Questions', 'Tests', 'Users'
    Description NVARCHAR(255),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Permissions_Name UNIQUE (Name)
);

CREATE TABLE RolePermissions (
    RoleId       INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PermissionId INT NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, PermissionId)
);

-- ============================================================
-- SECTION 2: USERS & AUTHENTICATION
-- ============================================================

CREATE TABLE Grades (
    Code      NVARCHAR(10)  PRIMARY KEY,   -- 'ECD','1'..'12'
    Label     NVARCHAR(50)  NOT NULL,       -- 'ECD', 'Grade 1' ..
    Band      NVARCHAR(20)  NOT NULL,       -- 'ECD-5','6-8','9-12'
    SortOrder INT           NOT NULL DEFAULT 0,
    IsEnabled BIT           NOT NULL DEFAULT 1
);

CREATE TABLE Users (
    Id            INT           PRIMARY KEY IDENTITY(1,1),
    Username      NVARCHAR(100) NOT NULL,
    PasswordHash  NVARCHAR(255) NOT NULL,
    RoleId        INT           NOT NULL REFERENCES Roles(Id),
    Name          NVARCHAR(150),
    Phone         NVARCHAR(20),
    Email         NVARCHAR(150),
    GradeCode     NVARCHAR(10)  REFERENCES Grades(Code),
    Medium        NVARCHAR(20)  NOT NULL DEFAULT 'English',
    Institute     NVARCHAR(200),
    Board         NVARCHAR(100) NOT NULL DEFAULT 'Balochistan',
    Coins         INT           NOT NULL DEFAULT 0,
    IsActive      BIT           NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt   DATETIME2,
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);
CREATE INDEX IX_Users_GradeCode ON Users(GradeCode);
CREATE INDEX IX_Users_RoleId    ON Users(RoleId);

CREATE TABLE RefreshTokens (
    Id           BIGINT        PRIMARY KEY IDENTITY(1,1),
    UserId       INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token        NVARCHAR(500) NOT NULL,
    ExpiresAt    DATETIME2     NOT NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt    DATETIME2,
    ReplacedByToken NVARCHAR(500),
    CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);

CREATE TABLE LoginHistory (
    Id         BIGINT        PRIMARY KEY IDENTITY(1,1),
    UserId     INT           REFERENCES Users(Id) ON DELETE CASCADE,
    Timestamp  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    Status     NVARCHAR(20),                -- 'ok' | 'logout' | 'failed'
    IpAddress  NVARCHAR(50),
    UserAgent  NVARCHAR(500),
    DeviceInfo NVARCHAR(200)
);
CREATE INDEX IX_LoginHistory_UserId    ON LoginHistory(UserId);
CREATE INDEX IX_LoginHistory_Timestamp ON LoginHistory(Timestamp);

-- ============================================================
-- SECTION 3: CURRICULUM CATALOG
-- ============================================================

CREATE TABLE Subjects (
    Id     INT           PRIMARY KEY IDENTITY(1,1),
    Name   NVARCHAR(100) NOT NULL,    -- 'Mathematics', 'Biology' ...
    NameUr NVARCHAR(100),             -- اردو name
    Icon   NVARCHAR(10),
    Color  NVARCHAR(50),
    Medium NVARCHAR(20)  NOT NULL DEFAULT 'english',
    CONSTRAINT UQ_Subjects_Name UNIQUE (Name)
);

CREATE TABLE GradeSubjects (
    GradeCode NVARCHAR(10) NOT NULL REFERENCES Grades(Code) ON DELETE CASCADE,
    SubjectId INT          NOT NULL REFERENCES Subjects(Id) ON DELETE CASCADE,
    PRIMARY KEY (GradeCode, SubjectId)
);

-- ============================================================
-- SECTION 4: SYLLABUS (Units → Topics → Learning Objectives)
-- ============================================================

CREATE TABLE Units (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    GradeCode   NVARCHAR(10)  NOT NULL REFERENCES Grades(Code) ON DELETE CASCADE,
    SubjectId   INT           NOT NULL REFERENCES Subjects(Id) ON DELETE CASCADE,
    Name        NVARCHAR(200) NOT NULL,
    Number      INT,
    SortOrder   INT           NOT NULL DEFAULT 0,
    Description NVARCHAR(MAX),
    Source      NVARCHAR(200)
);
CREATE INDEX IX_Units_GradeSubject ON Units(GradeCode, SubjectId);

CREATE TABLE Topics (
    Id        INT           PRIMARY KEY IDENTITY(1,1),
    UnitId    INT           NOT NULL REFERENCES Units(Id) ON DELETE CASCADE,
    Name      NVARCHAR(200) NOT NULL,
    SortOrder INT           NOT NULL DEFAULT 0
);
CREATE INDEX IX_Topics_UnitId ON Topics(UnitId);

CREATE TABLE LearningObjectives (
    Id             INT           PRIMARY KEY IDENTITY(1,1),
    UnitId         INT           REFERENCES Units(Id),
    TopicId        INT           REFERENCES Topics(Id),
    Code           NVARCHAR(50),
    ObjectiveText  NVARCHAR(MAX) NOT NULL,
    CognitiveLevel NVARCHAR(50),              -- 'Knowledge','Understanding','Applying'
    Source         NVARCHAR(200),
    IsAiStructured BIT           NOT NULL DEFAULT 0
);
CREATE INDEX IX_LO_UnitId  ON LearningObjectives(UnitId);
CREATE INDEX IX_LO_TopicId ON LearningObjectives(TopicId);

-- ============================================================
-- SECTION 5: AI TUTORS
-- ============================================================

CREATE TABLE AiTutors (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    SubjectId    INT           REFERENCES Subjects(Id),   -- NULL = cross-subject
    GradeCode    NVARCHAR(10)  REFERENCES Grades(Code),   -- NULL = all grades
    Persona      NVARCHAR(100) NOT NULL,                   -- 'Albert Einstein'
    Slug         NVARCHAR(50)  NOT NULL,                   -- 'einstein'
    Icon         NVARCHAR(10),
    Color        NVARCHAR(50),
    Description  NVARCHAR(255),
    SystemPrompt NVARCHAR(MAX),
    IsActive     BIT           NOT NULL DEFAULT 1,
    CONSTRAINT UQ_AiTutors_Slug UNIQUE (Slug)
);

-- ============================================================
-- SECTION 6: QUESTION BANK
-- ============================================================

CREATE TABLE Questions (
    Id             INT           PRIMARY KEY IDENTITY(1,1),
    GradeCode      NVARCHAR(10)  REFERENCES Grades(Code),
    SubjectId      INT           REFERENCES Subjects(Id),
    UnitId         INT           REFERENCES Units(Id),
    TopicId        INT           REFERENCES Topics(Id),
    Kind           NVARCHAR(20)  NOT NULL CHECK (Kind IN ('objective','subjective')),
    Stem           NVARCHAR(MAX) NOT NULL,
    OptionsJson    NVARCHAR(MAX),             -- JSON: ["opt A","opt B","opt C","opt D"]
    CorrectIndex   INT,                       -- 0-based for objective
    QuestionType   NVARCHAR(20),              -- 'Short' | 'Long' for subjective
    Marks          INT,
    ModelAnswer    NVARCHAR(MAX),
    TopicText      NVARCHAR(500),
    Skill          NVARCHAR(200),
    Difficulty     NVARCHAR(20)  CHECK (Difficulty IN ('Easy','Medium','Hard')),
    CognitiveLevel NVARCHAR(50),
    Feedback       NVARCHAR(MAX),
    Article        NVARCHAR(MAX),
    SourcePaper    NVARCHAR(200),
    SourceFormat   NVARCHAR(20),              -- 'xlsx','csv','docx','admin'
    IsAiGenerated  BIT           NOT NULL DEFAULT 0,
    IsEntranceExam BIT           NOT NULL DEFAULT 0,   -- MDCAT/ETEA flag
    SloCode        NVARCHAR(50),
    ContentHash    NVARCHAR(64),              -- dedup key
    CreatedById    INT           REFERENCES Users(Id),
    CreatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Questions_ContentHash UNIQUE (ContentHash)
);
CREATE INDEX IX_Questions_GradeSubjectUnit ON Questions(GradeCode, SubjectId, UnitId);
CREATE INDEX IX_Questions_Kind             ON Questions(Kind);
CREATE INDEX IX_Questions_Difficulty       ON Questions(Difficulty);
CREATE INDEX IX_Questions_SubjectId        ON Questions(SubjectId);

-- ============================================================
-- SECTION 7: TESTS & ATTEMPTS
-- ============================================================

CREATE TABLE Tests (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Title       NVARCHAR(200) NOT NULL,
    GradeCode   NVARCHAR(10)  REFERENCES Grades(Code),
    SubjectId   INT           REFERENCES Subjects(Id),
    UnitId      INT           REFERENCES Units(Id),
    Kind        NVARCHAR(30)  NOT NULL DEFAULT 'objective',
    --  'objective' | 'subjective' | 'daily' | 'monthly' | 'challenge' | 'weekly' | 'parent'
    DurationMin INT           DEFAULT 30,
    TotalMarks  INT,
    IsPublished BIT           NOT NULL DEFAULT 0,
    IsScheduled BIT           NOT NULL DEFAULT 0,
    ScheduledAt DATETIME2,
    CreatedById INT           REFERENCES Users(Id),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_Tests_GradeCode  ON Tests(GradeCode);
CREATE INDEX IX_Tests_SubjectId  ON Tests(SubjectId);
CREATE INDEX IX_Tests_Kind       ON Tests(Kind);

CREATE TABLE TestQuestions (
    TestId     INT NOT NULL REFERENCES Tests(Id) ON DELETE CASCADE,
    QuestionId INT NOT NULL REFERENCES Questions(Id) ON DELETE CASCADE,
    SortOrder  INT NOT NULL DEFAULT 0,
    PRIMARY KEY (TestId, QuestionId)
);

CREATE TABLE TestAttempts (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    UserId      INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    TestId      INT           REFERENCES Tests(Id),
    GradeCode   NVARCHAR(10),
    SubjectId   INT,
    Score       INT,
    Total       INT,
    Percent     DECIMAL(5,2),
    DurationSec INT,
    AnswersJson NVARCHAR(MAX),   -- JSON: [{questionId, chosen, correct, marks, timeSpent}]
    AttemptType NVARCHAR(30),    -- 'self-test'|'parent-test'|'daily'|'monthly'|'objective'|'subjective'
    CoinsEarned INT             NOT NULL DEFAULT 0,
    StartedAt   DATETIME2,
    SubmittedAt DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_Attempts_UserId  ON TestAttempts(UserId);
CREATE INDEX IX_Attempts_TestId  ON TestAttempts(TestId);
CREATE INDEX IX_Attempts_Grade   ON TestAttempts(GradeCode);

-- ============================================================
-- SECTION 8: COINS & REWARDS
-- ============================================================

CREATE TABLE CoinLedger (
    Id        BIGINT        PRIMARY KEY IDENTITY(1,1),
    UserId    INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Amount    INT           NOT NULL,          -- positive = earned, negative = spent
    Reason    NVARCHAR(200),                   -- 'test_complete','withdrawal_request','admin_credit'
    AttemptId INT           REFERENCES TestAttempts(Id),
    Timestamp DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_CoinLedger_UserId    ON CoinLedger(UserId);
CREATE INDEX IX_CoinLedger_Timestamp ON CoinLedger(Timestamp);

CREATE TABLE PayoutAccounts (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    UserId      INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    AccountName NVARCHAR(150) NOT NULL,
    AccountNo   NVARCHAR(50)  NOT NULL,
    Service     NVARCHAR(50)  NOT NULL,    -- 'JazzCash','EasyPaisa','BankTransfer'
    IsDefault   BIT           NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_PayoutAccounts_UserId UNIQUE (UserId)   -- one payout account per user
);

CREATE TABLE WithdrawalRequests (
    Id            INT           PRIMARY KEY IDENTITY(1,1),
    UserId        INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    AmountCoins   INT           NOT NULL,
    AmountPkr     DECIMAL(10,2) NOT NULL,
    AccountId     INT           REFERENCES PayoutAccounts(Id),
    Status        NVARCHAR(20)  NOT NULL DEFAULT 'pending',   -- 'pending'|'approved'|'rejected'|'paid'
    AdminNote     NVARCHAR(500),
    ProcessedById INT           REFERENCES Users(Id),
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt   DATETIME2
);
CREATE INDEX IX_Withdrawals_UserId ON WithdrawalRequests(UserId);
CREATE INDEX IX_Withdrawals_Status ON WithdrawalRequests(Status);

-- ============================================================
-- SECTION 9: NOTIFICATIONS
-- ============================================================

CREATE TABLE Notifications (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Title       NVARCHAR(200) NOT NULL,
    Body        NVARCHAR(MAX),
    Type        NVARCHAR(30)  NOT NULL DEFAULT 'info',   -- 'info'|'warning'|'success'|'alert'
    Icon        NVARCHAR(50),
    TargetRole  NVARCHAR(20),                             -- NULL = all; 'student','admin'
    TargetGrade NVARCHAR(10)  REFERENCES Grades(Code),
    CreatedById INT           REFERENCES Users(Id),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt   DATETIME2
);

CREATE TABLE UserNotifications (
    Id             BIGINT    PRIMARY KEY IDENTITY(1,1),
    UserId         INT       NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    NotificationId INT       NOT NULL REFERENCES Notifications(Id) ON DELETE CASCADE,
    IsRead         BIT       NOT NULL DEFAULT 0,
    ReadAt         DATETIME2,
    CreatedAt      DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_UserNotif_UserId         ON UserNotifications(UserId);
CREATE INDEX IX_UserNotif_NotificationId ON UserNotifications(NotificationId);

-- ============================================================
-- SECTION 10: COMPLAINTS & SUPPORT
-- ============================================================

CREATE TABLE Complaints (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    UserId      INT           REFERENCES Users(Id),
    Category    NVARCHAR(50)  NOT NULL DEFAULT 'general',   -- 'technical'|'content'|'billing'|'general'
    Subject     NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Status      NVARCHAR(20)  NOT NULL DEFAULT 'open',      -- 'open'|'in_progress'|'resolved'|'closed'
    AdminReply  NVARCHAR(MAX),
    HandledById INT           REFERENCES Users(Id),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt  DATETIME2
);
CREATE INDEX IX_Complaints_UserId ON Complaints(UserId);
CREATE INDEX IX_Complaints_Status ON Complaints(Status);

-- ============================================================
-- SECTION 11: CONTENT LIBRARY
-- ============================================================

CREATE TABLE ContentItems (
    Id           INT            PRIMARY KEY IDENTITY(1,1),
    GradeCode    NVARCHAR(10)   REFERENCES Grades(Code),
    SubjectId    INT            REFERENCES Subjects(Id),
    UnitId       INT            REFERENCES Units(Id),
    TopicId      INT            REFERENCES Topics(Id),
    Kind         NVARCHAR(30)   NOT NULL,
    -- 'video' | 'keynote' | 'resource' | 'ebook' | 'pastpaper' | 'simulation' | 'vlab'
    Title        NVARCHAR(300)  NOT NULL,
    Description  NVARCHAR(MAX),
    Url          NVARCHAR(1000),
    ThumbnailUrl NVARCHAR(1000),
    FilePath     NVARCHAR(500),
    DurationSec  INT,
    SortOrder    INT            NOT NULL DEFAULT 0,
    IsPublished  BIT            NOT NULL DEFAULT 0,
    Tags         NVARCHAR(500),
    SourceYear   NVARCHAR(10),            -- for past papers: exam year
    CreatedById  INT            REFERENCES Users(Id),
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_ContentItems_GradeSubject ON ContentItems(GradeCode, SubjectId);
CREATE INDEX IX_ContentItems_Kind         ON ContentItems(Kind);

-- ── Video Courses (structured playlists) ──

CREATE TABLE VideoCourses (
    Id           INT            PRIMARY KEY IDENTITY(1,1),
    GradeCode    NVARCHAR(10)   REFERENCES Grades(Code),
    SubjectId    INT            REFERENCES Subjects(Id),
    Title        NVARCHAR(300)  NOT NULL,
    Description  NVARCHAR(MAX),
    ThumbnailUrl NVARCHAR(1000),
    TutorName    NVARCHAR(150),
    IsPublished  BIT            NOT NULL DEFAULT 0,
    SortOrder    INT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE VideoLessons (
    Id          INT            PRIMARY KEY IDENTITY(1,1),
    CourseId    INT            NOT NULL REFERENCES VideoCourses(Id) ON DELETE CASCADE,
    UnitId      INT            REFERENCES Units(Id),
    Title       NVARCHAR(300)  NOT NULL,
    VideoUrl    NVARCHAR(1000) NOT NULL,
    DurationSec INT,
    SortOrder   INT            NOT NULL DEFAULT 0,
    Description NVARCHAR(MAX),
    Transcript  NVARCHAR(MAX),
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_VideoLessons_CourseId ON VideoLessons(CourseId);

-- ============================================================
-- SECTION 12: STUDENT TOOLS (Notes, Progress)
-- ============================================================

CREATE TABLE StudentNotes (
    Id        INT           PRIMARY KEY IDENTITY(1,1),
    UserId    INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    GradeCode NVARCHAR(10),
    SubjectId INT           REFERENCES Subjects(Id),
    UnitId    INT           REFERENCES Units(Id),
    Title     NVARCHAR(300),
    Content   NVARCHAR(MAX),
    Tags      NVARCHAR(300),
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_StudentNotes_UserId ON StudentNotes(UserId);

CREATE TABLE StudentProgress (
    Id             INT           PRIMARY KEY IDENTITY(1,1),
    UserId         INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    GradeCode      NVARCHAR(10),
    SubjectId      INT           REFERENCES Subjects(Id),
    TotalAttempts  INT           NOT NULL DEFAULT 0,
    TotalCorrect   INT           NOT NULL DEFAULT 0,
    TotalQuestions INT           NOT NULL DEFAULT 0,
    AvgPercent     DECIMAL(5,2)  NOT NULL DEFAULT 0,
    LastActivityAt DATETIME2,
    UpdatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_StudentProgress UNIQUE (UserId, GradeCode, SubjectId)
);
CREATE INDEX IX_StudentProgress_UserId    ON StudentProgress(UserId);
CREATE INDEX IX_StudentProgress_SubjectId ON StudentProgress(SubjectId);

-- ============================================================
-- SECTION 13: AUDIT LOG & SYSTEM SETTINGS
-- ============================================================

CREATE TABLE AuditLogs (
    Id         BIGINT        PRIMARY KEY IDENTITY(1,1),
    UserId     INT           REFERENCES Users(Id),
    Action     NVARCHAR(100) NOT NULL,        -- 'CREATE','UPDATE','DELETE','LOGIN','LOGOUT'
    EntityType NVARCHAR(100),                  -- 'Question','Test','User' ...
    EntityId   INT,
    OldValues  NVARCHAR(MAX),                  -- JSON snapshot before
    NewValues  NVARCHAR(MAX),                  -- JSON snapshot after
    IpAddress  NVARCHAR(50),
    Timestamp  DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_AuditLogs_UserId     ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Timestamp  ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType, EntityId);

CREATE TABLE SystemSettings (
    [Key]       NVARCHAR(100) PRIMARY KEY,
    Value       NVARCHAR(MAX),
    Description NVARCHAR(500),
    UpdatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedById INT           REFERENCES Users(Id)
);

-- ============================================================
-- SECTION 14: VIEWS
-- ============================================================

GO

CREATE OR ALTER VIEW vw_Leaderboard AS
    SELECT
        u.Id        AS UserId,
        u.Name,
        u.GradeCode,
        u.Coins,
        COUNT(a.Id)                                             AS TotalAttempts,
        COALESCE(CAST(ROUND(AVG(a.Percent), 1) AS DECIMAL(5,1)), 0) AS AvgPercent,
        RANK() OVER (ORDER BY u.Coins DESC)                     AS CoinRank,
        RANK() OVER (ORDER BY COALESCE(AVG(a.Percent), 0) DESC) AS ScoreRank
    FROM Users u
    JOIN Roles r ON r.Id = u.RoleId AND r.Name = 'student'
    LEFT JOIN TestAttempts a ON a.UserId = u.Id
    WHERE u.IsActive = 1
    GROUP BY u.Id, u.Name, u.GradeCode, u.Coins;
GO

CREATE OR ALTER VIEW vw_SubjectQuestionCounts AS
    SELECT
        s.Id       AS SubjectId,
        s.Name     AS SubjectName,
        q.GradeCode,
        q.Kind,
        q.Difficulty,
        COUNT(q.Id) AS QuestionCount
    FROM Subjects s
    LEFT JOIN Questions q ON q.SubjectId = s.Id
    GROUP BY s.Id, s.Name, q.GradeCode, q.Kind, q.Difficulty;
GO

-- ============================================================
-- SECTION 15: SEED DATA
-- ============================================================

-- Roles
INSERT INTO Roles (Name, Description) VALUES
    ('admin',   'System administrator — full access'),
    ('student', 'Student — learning modules and tests');
GO

-- Permissions
INSERT INTO Permissions (Name, Module, Description) VALUES
    ('grades.read',           'Grades',     'View grades'),
    ('grades.write',          'Grades',     'Create/update/delete grades'),
    ('subjects.read',         'Subjects',   'View subjects'),
    ('subjects.write',        'Subjects',   'Create/update/delete subjects'),
    ('syllabus.read',         'Syllabus',   'View syllabus'),
    ('syllabus.write',        'Syllabus',   'Manage syllabus units/topics/objectives'),
    ('questions.read',        'Questions',  'View questions'),
    ('questions.write',       'Questions',  'Create/update/delete questions'),
    ('tests.read',            'Tests',      'View tests'),
    ('tests.write',           'Tests',      'Create/update/delete tests'),
    ('tests.publish',         'Tests',      'Publish/unpublish tests'),
    ('users.read',            'Users',      'View users'),
    ('users.write',           'Users',      'Create/update/deactivate users'),
    ('coins.read',            'Coins',      'View coin ledger and withdrawal requests'),
    ('coins.process',         'Coins',      'Approve/reject withdrawal requests'),
    ('content.read',          'Content',    'View content items'),
    ('content.write',         'Content',    'Upload/manage content'),
    ('notifications.send',    'Notifications','Send notifications'),
    ('complaints.read',       'Complaints', 'View complaints'),
    ('complaints.respond',    'Complaints', 'Respond to complaints'),
    ('analytics.view',        'Analytics',  'View analytics dashboards'),
    ('settings.write',        'Settings',   'Update system settings'),
    ('tutors.write',          'AiTutors',   'Manage AI tutor personas');
GO

-- Grant all permissions to admin role
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r CROSS JOIN Permissions p WHERE r.Name = 'admin';
GO

-- Grant student read permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r JOIN Permissions p
    ON p.Name IN ('grades.read','subjects.read','syllabus.read','questions.read','tests.read','content.read')
WHERE r.Name = 'student';
GO

-- Grades
INSERT INTO Grades (Code, Label, Band, SortOrder, IsEnabled) VALUES
    ('ECD', 'ECD',      'ECD-5',  0,  1),
    ('1',   'Grade 1',  'ECD-5',  1,  1),
    ('2',   'Grade 2',  'ECD-5',  2,  1),
    ('3',   'Grade 3',  'ECD-5',  3,  1),
    ('4',   'Grade 4',  'ECD-5',  4,  1),
    ('5',   'Grade 5',  'ECD-5',  5,  1),
    ('6',   'Grade 6',  '6-8',    6,  1),
    ('7',   'Grade 7',  '6-8',    7,  1),
    ('8',   'Grade 8',  '6-8',    8,  1),
    ('9',   'Grade 9',  '9-12',   9,  1),
    ('10',  'Grade 10', '9-12',   10, 1),
    ('11',  'Grade 11', '9-12',   11, 1),
    ('12',  'Grade 12', '9-12',   12, 1);
GO

-- Subjects
INSERT INTO Subjects (Name, NameUr, Icon, Color, Medium) VALUES
    ('Urdu',             N'اردو',             N'📖', 'grad-violet',  'urdu'),
    ('Mutalia Pakistan', N'مطالعہ پاکستان',   N'🌍', 'grad-green',   'urdu'),
    ('English',          N'انگریزی',          N'✍️', 'grad-blue',    'english'),
    ('Biology',          N'حیاتیات',          N'🧬', 'grad-emerald', 'english'),
    ('Chemistry',        N'کیمیاء',           N'⚗️', 'grad-rose',    'english'),
    ('Physics',          N'طبیعیات',          N'⚡', 'grad-amber',   'english'),
    ('Mathematics',      N'ریاضی',            N'📐', 'grad-teal',    'english'),
    ('Pakistan Studies', N'پاکستان سٹڈیز',   N'🏛️', 'grad-pink',    'english'),
    ('Islamiyat Lazmi',  N'اسلامیات لازمی',  N'☪️', 'grad-green',   'urdu'),
    ('Computer Science', N'کمپیوٹر سائنس',   N'💻', 'grad-cyan',    'english');
GO

-- AI Tutors
INSERT INTO AiTutors (SubjectId, Persona, Slug, Icon, Color, Description)
SELECT s.Id, t.Persona, t.Slug, t.Icon, t.Color, t.Description
FROM (VALUES
    ('Physics',          'Albert Einstein',     'einstein',    N'⚡', 'grad-amber',   'Theoretical Physics Master'),
    ('Mathematics',      'Al-Khwarizmi',        'khwarizmi',   N'📐', 'grad-teal',    'Father of Algebra'),
    ('Chemistry',        'Marie Curie',         'curie',       N'⚗️', 'grad-rose',    'Nobel Prize Chemist'),
    ('Biology',          'Ibn Sina',            'ibnsina',     N'🧬', 'grad-emerald', 'Father of Early Medicine'),
    ('Computer Science', 'Alan Turing',         'turing',      N'💻', 'grad-blue',    'Father of Computing'),
    ('Pakistan Studies', 'Allama Iqbal',        'iqbal',       N'🏛️', 'grad-violet',  'Poet-Philosopher of Pakistan'),
    ('English',          'William Shakespeare', 'shakespeare', N'✍️', 'grad-cyan',    'Greatest English Writer'),
    ('Urdu',             'Mirza Ghalib',        'ghalib',      N'📖', 'grad-pink',    'Master of Urdu Poetry'),
    ('Islamiyat Lazmi',  'Imam Al-Ghazali',     'ghazali',     N'🕌', 'grad-green',   'Islamic Scholar & Philosopher')
) t(SubjectName, Persona, Slug, Icon, Color, Description)
JOIN Subjects s ON s.Name = t.SubjectName;
GO

-- Admin user (password: Admin@123 — change immediately in production)
-- BCrypt hash for 'Admin@123' with work factor 12
DECLARE @adminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'admin');
INSERT INTO Users (Username, PasswordHash, RoleId, Name, Board, IsActive)
VALUES ('admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPJ9UGNxfzfBK', @adminRoleId, 'System Administrator', 'Balochistan', 1);
GO

-- System Settings
INSERT INTO SystemSettings ([Key], Value, Description) VALUES
    ('app.name',             'Balochistan Academy Portal', 'Application display name'),
    ('app.board',            'Balochistan',                'Default board name'),
    ('coins.rate_pkr',       '0.50',                       'PKR value per 1 coin'),
    ('coins.min_withdrawal', '300',                        'Minimum coins required for withdrawal'),
    ('coins.min_questions',  '35',                         'Minimum questions in a test to earn coins'),
    ('app.ai_provider',      'gemini',                     'AI provider: gemini or ollama'),
    ('app.maintenance',      '0',                          '1 = maintenance mode active'),
    ('jwt.access_expiry',    '60',                         'JWT access token expiry in minutes'),
    ('jwt.refresh_expiry',   '43200',                      'Refresh token expiry in minutes (30 days)');
GO

-- ============================================================
-- END OF SCHEMA
-- ============================================================
PRINT 'BalochiAcademyDB schema created and seeded successfully.';
GO
