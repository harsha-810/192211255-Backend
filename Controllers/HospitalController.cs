using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/hospital")]
[ApiController]
[Authorize]
public class HospitalController : ControllerBase
{
    private readonly AppDbContext _context;

    public HospitalController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetHospitals()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        var adminIdStr = User.FindFirst("userId")?.Value;
        
        IQueryable<Hospital> query = _context.Hospitals.Include(h => h.Doctors);
        
        // Admins only see hospitals they manage
        if (role == "Admin" && !string.IsNullOrEmpty(adminIdStr))
        {
            var adminId = int.Parse(adminIdStr);
            query = query.Where(h => h.AdminId == adminId);
        }
        // Patients and Doctors see ALL active hospitals
        
        var hospitals = await query
            .Select(h => new HospitalResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                Timings = h.Timings,
                Doctors = h.Doctors.Where(d => d.IsActive).Select(d => new DoctorResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specialization = d.Specialization
                }).ToList()
            })
            .ToListAsync();
        return Ok(hospitals);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateHospital([FromBody] Hospital request)
    {
        var adminId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        request.AdminId = adminId;
        
        // Prevent duplicate hospitals
        bool exists = await _context.Hospitals
            .AnyAsync(h => h.Name.ToLower() == request.Name.ToLower() || 
                           h.Address.ToLower() == request.Address.ToLower());
        
        if (exists)
        {
            return BadRequest(new { message = "A hospital with this name or address already exists." });
        }

        _context.Hospitals.Add(request);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetHospitals), null, request);
    }

    [HttpPost("{hospitalId}/doctors")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDoctor(int hospitalId, [FromBody] AddDoctorRequest request)
    {
        var hospital = await _context.Hospitals.FindAsync(hospitalId);
        if (hospital == null) return NotFound(new { message = "Hospital not found." });

        var email = request.Email.Trim().ToLower();
        var password = request.Password.Trim();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "Doctor email already exists." });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User { Email = email, PasswordHash = password, Role = "Doctor" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                Name = request.Name,
                Specialization = request.Specialization,
                IsActive = true,
                UserId = user.Id,
                HospitalId = hospitalId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { message = "Doctor added successfully." });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error adding doctor.");
        }
    }

    [HttpPut("doctors/{doctorId}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleDoctorStatus(int doctorId, [FromBody] bool isActive)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null) return NotFound(new { message = "Doctor not found." });

        doctor.IsActive = isActive;
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Doctor status updated." });
    }

    [HttpDelete("{hospitalId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHospital(int hospitalId)
    {
        var hospital = await _context.Hospitals
            .Include(h => h.Doctors)
            .FirstOrDefaultAsync(h => h.Id == hospitalId);

        if (hospital == null) return NotFound(new { message = "Hospital not found." });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Collect all User IDs associated with the doctors
            var doctorUserIds = hospital.Doctors.Select(d => d.UserId).ToList();

            // The Hospital and Doctors will be deleted together via Entity Framework,
            // but we must manually delete the Users for those doctors
            var usersToDelete = await _context.Users.Where(u => doctorUserIds.Contains(u.Id)).ToListAsync();

            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();

            if (usersToDelete.Any())
            {
                _context.Users.RemoveRange(usersToDelete);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Hospital and associated doctors deleted successfully." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error deleting hospital.", details = ex.Message });
        }
    }

    [HttpDelete("doctors/{doctorId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDoctor(int doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null) return NotFound(new { message = "Doctor not found." });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users.FindAsync(doctor.UserId);

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            if (user != null) 
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Doctor deleted successfully." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error deleting doctor.", details = ex.Message });
        }
    }
}

public class AddDoctorRequest
{
    [Required(ErrorMessage = "Doctor Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;
}

public class HospitalResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
    public List<DoctorResponseDto> Doctors { get; set; } = new();
}

public class DoctorResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}
