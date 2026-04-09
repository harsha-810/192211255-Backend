using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/Patient")]
[ApiController]
[Authorize(Roles = "Patient")]
public class PatientController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("my-pets")]
    public async Task<IActionResult> GetMyPets()
    {
        Console.WriteLine("API: GetMyPets called");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (patient == null) return NotFound(new { message = "Patient record not found." });

        var pets = await _context.Pets.Where(p => p.PatientId == patient.Id)
            .Include(p => p.Vaccinations)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Prescription)
            .ToListAsync();
            
        return Ok(pets);
    }

    [HttpPost("add-pet")]
    public async Task<IActionResult> AddPet([FromBody] Pet petRequest)
    {
        Console.WriteLine("API: AddPet called");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (patient == null) return NotFound("Patient record not found.");

        petRequest.PatientId = patient.Id;
        _context.Pets.Add(petRequest);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetMyPets), null, petRequest);
    }


    [HttpGet("pet/{petId}/history")]
    public async Task<IActionResult> GetPetHistory(int petId)
    {
        Console.WriteLine($"API: GetPetHistory called for {petId}");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(new { message = "Patient record not found." });

        var pet = await _context.Pets
            .Include(p => p.Vaccinations)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Prescription)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == petId && p.PatientId == patient.Id);

        if (pet == null) return NotFound("Pet record not found or unauthorized.");

        return Ok(pet);
    }

    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments()
    {
        Console.WriteLine("API: GetMyAppointments called");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound(new { message = "Patient record not found." });

        var appointments = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Doctor)
            .Include(a => a.Prescription)
            .Where(a => a.Pet!.PatientId == patient.Id)
            .OrderByDescending(a => a.Date)
            .Select(a => new {
                a.Id,
                a.PetId,
                a.DoctorId,
                a.Date,
                a.Status,
                a.RejectionReason,
                Pet = new { a.Pet!.Id, a.Pet.Name },
                Doctor = a.Doctor != null ? new { a.Doctor.Id, a.Doctor.Name } : null,
                Prescription = a.Prescription != null ? new { 
                    a.Prescription.Id, 
                    a.Prescription.Diagnosis, 
                    a.Prescription.Medicines, 
                    a.Prescription.Advice 
                } : null
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (patient == null) return NotFound("Patient not found.");

        return Ok(new {
            patient.Name,
            patient.Phone,
            Email = patient.User?.Email ?? "N/A",
            PetCount = await _context.Pets.CountAsync(p => p.PatientId == patient.Id)
        });
    }

    [HttpPut("update-pet/{id}")]
    public async Task<IActionResult> UpdatePet(int id, [FromBody] Pet request)
    {
        Console.WriteLine($"API: UpdatePet called for {id}");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound("Patient record not found.");

        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.PatientId == patient.Id);
        if (pet == null) return NotFound("Pet record not found or unauthorized.");

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.Age = request.Age;

        await _context.SaveChangesAsync();
        return Ok(pet);
    }

    [HttpDelete("delete-pet/{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        Console.WriteLine($"API: DeletePet called for {id}");
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound("Patient record not found.");

        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.PatientId == patient.Id);
        if (pet == null) return NotFound("Pet record not found or unauthorized.");

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Pet removed from your family." });
    }
}
