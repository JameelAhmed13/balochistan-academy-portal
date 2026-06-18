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
                src.Institute,
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
                src.TestQuestions.Count,
                src.CreatedAt))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<TestAttempt, AttemptResultDto>()
            .ConstructUsing(src => new AttemptResultDto(
                src.Id,
                src.UserId,
                src.TestId,
                src.Score,
                src.Total,
                src.Percent,
                src.CoinsEarned,
                src.AttemptType ?? string.Empty,
                src.SubmittedAt))
            .ForAllMembers(opt => opt.Ignore());

        // ── Coins ─────────────────────────────────────────────────────────────
        CreateMap<CoinLedger, CoinLedgerEntryDto>();
        CreateMap<PayoutAccount, PayoutAccountDto>();
        CreateMap<WithdrawalRequest, WithdrawalRequestDto>();

        // ── Complaints ────────────────────────────────────────────────────────
        CreateMap<Complaint, ComplaintDto>()
            .ConstructUsing(src => new ComplaintDto(
                src.Id,
                src.User != null ? src.User.Name : null,
                src.Category,
                src.Subject,
                src.Description,
                src.Status,
                src.AdminReply,
                src.CreatedAt,
                src.ResolvedAt))
            .ForAllMembers(opt => opt.Ignore());
    }
}
