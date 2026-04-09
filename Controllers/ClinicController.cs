using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/clinics")]
[ApiController]
[Authorize]
public class ClinicController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public ClinicController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllClinics()
    {
        var clinics = await _context.Hospitals
            .Select(h => h.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
        return Ok(clinics);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAvailableDoctors([FromQuery] string? clinicName = null)
    {
        var query = _context.Doctors
            .Include(d => d.Hospital)
            .Where(d => d.IsActive);

        if (!string.IsNullOrEmpty(clinicName))
        {
            query = query.Where(d => d.Hospital != null && d.Hospital.Name == clinicName);
        }

        var doctors = await query
            .Select(d => new 
            { 
                Id = d.Id, 
                Name = d.Name, 
                Specialization = d.Specialization, 
                HospitalName = d.Hospital != null ? d.Hospital.Name : "General Clinic" 
            })
            .ToListAsync();
        return Ok(doctors);
    }
}
