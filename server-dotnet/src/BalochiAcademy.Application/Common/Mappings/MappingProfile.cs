using AutoMapper;
using BalochiAcademy.Application.Auth;
using BalochiAcademy.Application.Coins;
using BalochiAcademy.Application.Complaints;
using BalochiAcademy.Application.Questions;
using BalochiAcademy.Application.Tests;
using BalochiAcademy.Domain.Entities;

namespace BalochiAcademy.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // AutoMapper 12 overwrites positional-record init properties after ConstructUsing runs.
        // ForAllMembers(Ignore) prevents that post-construction overwrite.

        // ── Auth ──────────────────────────────────────────────────────────────
        CreateMap<User, UserDto>()
            .ConstructUsing(src => new UserDto(
                src.Id,
                src.Username,
                src.Role != null ? src.Role.Name : "student",
                src.Name,
                src.Phone,
                src.Email,
                src.GradeCode,
                src.Medium ?? "English",
                src.Institute != null ? src.Institute.Name : null,
                src.Board ?? "Balochistan",
                src.Coins))
            .ForAllMembers(opt => opt.Ignore());

        // ── Questions ─────────────────────────────────────────────────────────
        CreateMap<Question, QuestionDto>();

        // ── Tests ─────────────────────────────────────────────────────────────
        CreateMap<Test, TestDto>()
            .ConstructUsing(src => new TestDto(
                src.Id,
                src.Title,
                src.GradeCode,
                src.SubjectId,
                src.UnitId,
                src.Kind,
                src.DurationMin,
                src.TotalMarks,
                src.IsPublished,
                src.IsScheduled,
                src.ScheduledAt,
                src.EndsAt,
                src.TestQuestions.Count,
                src.CreatedAt))
            .ForAllMembers(opt => opt.Ignore());

        // SubjectName isn't on TestAttempt itself — the controller attaches it afterward via a batch
        // lookup (AttachSubjectNames) since it needs a cross-row Subject-table join, not a per-row map.
        CreateMap<TestAttempt, AttemptResultDto>()
            .ConstructUsing(src => new AttemptResultDto(
                src.Id,
                src.UserId,
                src.TestId,
                src.SubjectId,
                null,
                src.Score,
                src.Total,
                src.Percent,
                src.CoinsEarned,
                src.AttemptType ?? string.Empty,
                src.SubmittedAt))
            .ForAllMembers(opt => opt.Ignore());

        // ── Coins ─────────────────────────────────────────────────────────────
        CreateMap<CoinLedger, CoinLedgerEntryDto>();

        // ── Complaints ────────────────────────────────────────────────────────
        CreateMap<Complaint, ComplaintDto>()
            .ConstructUsing(src => new ComplaintDto(
                src.Id,
                src.UserId,
                src.User != null ? src.User.Name : null,
                src.Category,
                src.Subject,
                src.Description,
                src.Status,
                src.Messages.Count,
                src.CreatedAt,
                src.ResolvedAt))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<ComplaintMessage, ComplaintMessageDto>()
            .ConstructUsing(src => new ComplaintMessageDto(
                src.Id,
                src.SenderId,
                src.Sender != null ? src.Sender.Name : null,
                src.IsAdmin,
                src.Message,
                src.CreatedAt))
            .ForAllMembers(opt => opt.Ignore());
    }
}
