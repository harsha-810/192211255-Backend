using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
public class DoctorController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        
        if (doctor == null) return NotFound("Doctor not found.");

        // Calculate today's stats
        var today = DateTime.UtcNow.Date;
        
        var totalPending = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Pending");
            
        var completedToday = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Completed" && a.Date.Date == today);
            
        var emergencies = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Pending" && a.PriorityLevel == 1);

        return Ok(new
        {
            TotalPending = totalPending,
            CompletedToday = completedToday,
            EmergenciesCritical = emergencies
        });
    }

    [HttpGet("pet/{petId}/history")]
    public async Task<IActionResult> GetPetHistory(int petId)
    {
        var pet = await _context.Pets
            .Include(p => p.Vaccinations)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Prescription)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == petId);
            
        if (pet == null) return NotFound("Pet not found.");

        return Ok(pet);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Hospital)
            .FirstOrDefaultAsync(d => d.UserId == userId);
        
        if (doctor == null) return NotFound("Doctor not found.");

        return Ok(new {
            doctor.Name,
            doctor.Specialization,
            Email = doctor.User?.Email ?? "N/A",
            HospitalName = doctor.Hospital?.Name ?? "N/A",
            doctor.IsActive
        });
    }

    [HttpPost("vaccination")]
    public async Task<IActionResult> AddVaccination([FromBody] Vaccination request)
    {
        Console.WriteLine("API: Doctor AddVaccination called");
        
        var pet = await _context.Pets
            .Include(p => p.Patient)
            .FirstOrDefaultAsync(p => p.Id == request.PetId);
            
        if (pet == null) return NotFound("Pet not found.");

        _context.Vaccinations.Add(request);
        
        if (pet.Patient != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = pet.Patient.UserId,
                Message = $"A doctor has recorded a new vaccination for {pet.Name}: {request.VaccineName}. Check medical records for details."
            });
        }

        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Vaccination record successfully added." });
    }
}
