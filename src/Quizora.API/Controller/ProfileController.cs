using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizora.Application.Common;
using Quizora.Application.DTOs.Auth;
using Quizora.Application.Interfaces;
using Quizora.Domain.Enums;
using Quizora.Infrastructure.Persistence;
using System.Security.Claims;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(
        IUserRepository userRepository,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _userRepository = userRepository;
        _db = db;
        _env = env;
    }

    // ───────── Profile ─────────

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Ok(Result<ProfileDto>.Failure("User not found"));

            var dto = new ProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                Phone = user.Candidate?.Phone,
                CompanyName = user.Company?.CompanyName,
                CompanyDescription = user.Company?.Description
            };

            if (user.Role == UserRole.Candidate && user.Candidate != null)
            {
                var inv = await _db.TestInvitations
                    .Where(i => i.CandidateId == user.Candidate.Id)
                    .ToListAsync();
                dto.TestsCompleted = inv.Count(i => i.Status == InvitationStatus.Completed);
                dto.TestsPending = inv.Count(i => i.Status == InvitationStatus.Pending);
            }
            else if (user.Role == UserRole.Company && user.Company != null)
            {
                var tests = await _db.Tests
                    .Include(t => t.Invitations)
                    .Where(t => t.CompanyId == user.Company.Id)
                    .ToListAsync();
                dto.TestsCreated = tests.Count;
                dto.TotalInvited = tests.Sum(t => t.Invitations?.Count ?? 0);
            }

            return Ok(Result<ProfileDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return Ok(Result<ProfileDto>.Failure(ex.Message));
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.FullName))
                return Ok(Result.Failure("Full name is required"));

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Ok(Result.Failure("User not found"));

            user.FullName = dto.FullName.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            if (user.Role == UserRole.Candidate && user.Candidate != null)
            {
                user.Candidate.Phone = string.IsNullOrWhiteSpace(dto.Phone)
                    ? null
                    : dto.Phone.Trim();
            }

            if (user.Role == UserRole.Company && user.Company != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.CompanyName))
                    user.Company.CompanyName = dto.CompanyName.Trim();

                user.Company.Description = string.IsNullOrWhiteSpace(dto.CompanyDescription)
                    ? null
                    : dto.CompanyDescription.Trim();
            }

            await _userRepository.SaveChangesAsync();
            return Ok(Result.Success("Profile updated"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.Message));
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.CurrentPassword)
                || string.IsNullOrWhiteSpace(dto.NewPassword))
                return Ok(Result.Failure("Current and new password are required"));

            if (dto.NewPassword.Length < 6)
                return Ok(Result.Failure("New password must be at least 6 characters"));

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Ok(Result.Failure("User not found"));

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return Ok(Result.Failure("Current password is incorrect"));

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            return Ok(Result.Success("Password changed successfully"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.Message));
        }
    }

    // ───────── Candidate CV ─────────

    [HttpPost("cv")]
    [Authorize(Roles = "Candidate")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Ok(Result.Failure("No file uploaded"));

            if (file.Length > 10 * 1024 * 1024)
                return Ok(Result.Failure("File too large. Maximum 10 MB."));

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".pdf")
                return Ok(Result.Failure("Only PDF files are allowed"));

            var ct = (file.ContentType ?? "").ToLowerInvariant();
            if (!ct.Contains("pdf") && ct != "application/octet-stream")
                return Ok(Result.Failure("Invalid file type. PDF only."));

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Candidate == null)
                return Ok(Result.Failure("Candidate not found"));

            var folder = Path.Combine(_env.ContentRootPath, "Uploads", "cvs");
            Directory.CreateDirectory(folder);

            if (!string.IsNullOrEmpty(user.Candidate.CvStoredName))
            {
                var oldPath = Path.Combine(folder, user.Candidate.CvStoredName);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var stored = $"{user.Candidate.Id:N}_{Guid.NewGuid():N}.pdf";
            var path = Path.Combine(folder, stored);

            await using (var stream = System.IO.File.Create(path))
                await file.CopyToAsync(stream);

            user.Candidate.CvOriginalName = Path.GetFileName(file.FileName);
            user.Candidate.CvStoredName = stored;
            user.Candidate.CvFileSize = file.Length;
            user.Candidate.CvUploadedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return Ok(Result.Success("CV uploaded successfully"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.Message));
        }
    }

    [HttpGet("cv/mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> MyCvInfo()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Candidate == null)
                return Ok(Result<object>.Failure("Candidate not found"));

            if (string.IsNullOrEmpty(user.Candidate.CvStoredName))
                return Ok(Result<object>.Success(new { HasCv = false }));

            return Ok(Result<object>.Success(new
            {
                HasCv = true,
                FileName = user.Candidate.CvOriginalName,
                FileSize = user.Candidate.CvFileSize,
                UploadedAt = user.Candidate.CvUploadedAt
            }));
        }
        catch (Exception ex)
        {
            return Ok(Result<object>.Failure(ex.Message));
        }
    }

    [HttpGet("cv/download/mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> DownloadMyCv()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Candidate?.CvStoredName == null)
            return NotFound("No CV uploaded");

        var path = Path.Combine(_env.ContentRootPath, "Uploads", "cvs", user.Candidate.CvStoredName);
        if (!System.IO.File.Exists(path))
            return NotFound("File missing");

        return PhysicalFile(path, "application/pdf", user.Candidate.CvOriginalName ?? "cv.pdf");
    }

    [HttpDelete("cv")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> DeleteMyCv()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.Candidate == null)
                return Ok(Result.Failure("Candidate not found"));

            if (!string.IsNullOrEmpty(user.Candidate.CvStoredName))
            {
                var path = Path.Combine(_env.ContentRootPath, "Uploads", "cvs", user.Candidate.CvStoredName);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            user.Candidate.CvOriginalName = null;
            user.Candidate.CvStoredName = null;
            user.Candidate.CvFileSize = null;
            user.Candidate.CvUploadedAt = null;
            await _userRepository.SaveChangesAsync();

            return Ok(Result.Success("CV deleted"));
        }
        catch (Exception ex)
        {
            return Ok(Result.Failure(ex.Message));
        }
    }

    // ───────── Company: all CVs ─────────

    [HttpGet("cv/all")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> ListAllCvs()
    {
        try
        {
            var list = await _db.Candidates
                .Include(c => c.User)
                .Where(c => c.CvStoredName != null && c.CvStoredName != "")
                .OrderByDescending(c => c.CvUploadedAt)
                .Select(c => new CandidateCvDto
                {
                    CandidateId = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    Email = c.User.Email,
                    Phone = c.Phone,
                    FileName = c.CvOriginalName ?? "cv.pdf",
                    FileSize = c.CvFileSize ?? 0,
                    UploadedAt = c.CvUploadedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(Result<List<CandidateCvDto>>.Success(list));
        }
        catch (Exception ex)
        {
            return Ok(Result<List<CandidateCvDto>>.Failure(ex.Message));
        }
    }

    [HttpGet("cv/download/{candidateId:guid}")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> DownloadCandidateCv(Guid candidateId)
    {
        var c = await _db.Candidates
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == candidateId);

        if (c?.CvStoredName == null)
            return NotFound("CV not found");

        var path = Path.Combine(_env.ContentRootPath, "Uploads", "cvs", c.CvStoredName);
        if (!System.IO.File.Exists(path))
            return NotFound("File missing");

        return PhysicalFile(path, "application/pdf", c.CvOriginalName ?? "cv.pdf");
    }
}