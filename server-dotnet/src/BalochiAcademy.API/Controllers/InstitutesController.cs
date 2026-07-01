using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/institutes")]
[Authorize(Policy = "AdminOnly")]
public class InstitutesController(IUnitOfWork uow) : ControllerBase
{
    /// <summary>GET /api/admin/institutes — list all institutes</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await uow.Repository<Institute>().Query()
            .OrderBy(i => i.Name)
            .Select(i => new
            {
                i.Id, i.Name, i.Code, i.Address, i.IsActive, i.CreatedAt,
                UserCount = i.Users.Count,
            })
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>POST /api/admin/institutes — create institute</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InstituteRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Institute name is required." });

        var exists = await uow.Repository<Institute>().Query()
            .AnyAsync(i => i.Name == req.Name.Trim(), ct);
        if (exists)
            return Conflict(new { message = "An institute with this name already exists." });

        var institute = new Institute
        {
            Name    = req.Name.Trim(),
            Code    = req.Code?.Trim().ToUpper(),
            Address = req.Address?.Trim(),
            IsActive = true,
        };
        uow.Repository<Institute>().Add(institute);
        await uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(List), new { id = institute.Id }, new
        {
            institute.Id, institute.Name, institute.Code,
            institute.Address, institute.IsActive, institute.CreatedAt,
            UserCount = 0,
        });
    }

    /// <summary>PUT /api/admin/institutes/{id} — update institute</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InstituteRequest req, CancellationToken ct)
    {
        var institute = await uow.Repository<Institute>().FindAsync([id], ct);
        if (institute == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Name)) institute.Name    = req.Name.Trim();
        if (req.Code    != null)                   institute.Code    = req.Code.Trim().ToUpper();
        if (req.Address != null)                   institute.Address = req.Address.Trim();
        if (req.IsActive.HasValue)                 institute.IsActive = req.IsActive.Value;

        await uow.SaveChangesAsync(ct);
        return Ok(new { institute.Id, institute.Name, institute.Code, institute.Address, institute.IsActive });
    }

    /// <summary>DELETE /api/admin/institutes/{id} — delete institute (409 if users assigned)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var hasUsers = await uow.Repository<User>().Query()
            .AnyAsync(u => u.InstituteId == id, ct);
        if (hasUsers)
            return Conflict(new { message = "Cannot delete an institute that has users assigned to it." });

        var institute = await uow.Repository<Institute>().FindAsync([id], ct);
        if (institute == null) return NotFound();

        uow.Repository<Institute>().Remove(institute);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record InstituteRequest(string Name, string? Code = null, string? Address = null, bool? IsActive = null);
