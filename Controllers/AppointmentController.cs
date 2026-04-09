using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicAPI.Models;
using PetClinicAPI.Services;

namespace PetClinicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAiService _aiService;

    public AppointmentController(AppDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [Authorize(Roles = "Patient")]
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (patient == null) return NotFound(new { message = "Patient not found." });

        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == request.PetId && p.PatientId == patient.Id);
        if (pet == null) return Unauthorized(new { message = "Pet does not belong to you." });

        // Use AI results from request, or fallback to internal analysis
        var aiCondition = request.AiCondition;
        var aiSeverity = request.AiSeverity;
        var priorityLevel = request.PriorityLevel;

        if (string.IsNullOrEmpty(aiCondition))
        {
            var symptomsList = request.Symptoms?.Split(",").Select(s => s.Trim()).ToList() ?? new List<string>();
            var analysis = await _aiService.AnalyzeSymptomsAsync(
                symptomsList, 
                request.Duration ?? "", 
                pet.Age, 
                pet.Species, 
                pet.Breed
            );
            aiCondition = analysis.condition;
            aiSeverity = analysis.severity;
            priorityLevel = analysis.priorityLevel;
        }

        var appointment = new Appointment
        {
            PetId = request.PetId,
            DoctorId = request.DoctorId,
            Date = request.Date,
            Symptoms = request.Symptoms,
            Duration = request.Duration,
            AiCondition = aiCondition,
            AiSeverity = aiSeverity,
            PriorityLevel = priorityLevel > 0 ? priorityLevel : 3,
            Status = "Pending"
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Appointment booked successfully.", appointment });
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("queue")]
    public async Task<IActionResult> GetDoctorQueue()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        
        if (doctor == null) return NotFound();

        var appointments = await _context.Appointments
            .Include(a => a.Pet!)
                .ThenInclude(p => p.Patient!)
            .Include(a => a.Prescription)
            .Where(a => a.DoctorId == doctor.Id && (a.Status == "Pending" || a.Status == "Accepted" || a.Status == "Rejected" || a.Status == "Completed"))
            .OrderBy(a => a.PriorityLevel)
            .ThenByDescending(a => a.Date)
            .ToListAsync();

        return Ok(appointments);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{appointmentId}/status")]
    public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] UpdateStatusRequest request)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null) return NotFound(new { message = "Appointment not found." });

        if (request.Status == "Rejected" && string.IsNullOrEmpty(request.RejectionReason))
        {
            return BadRequest(new { message = "Rejection reason is mandatory." });
        }

        appointment.Status = request.Status;
        appointment.RejectionReason = request.RejectionReason;
        
        await _context.SaveChangesAsync();
        
        // Setup Notification logic
        if (appointment.Pet != null)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == appointment.Pet.PatientId);
            if (patient != null)
            {
                var notification = new Notification
                {
                    UserId = patient.UserId,
                    Message = $"Your appointment for {appointment.Pet.Name} has been {request.Status.ToLower()}."
                };
                if (request.Status == "Rejected")
                {
                    notification.Message += $" Reason: {request.RejectionReason}";
                }
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
        }
        
        return Ok(new { message = $"Appointment status updated to {request.Status}." });
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost("{appointmentId}/prescribe")]
    public async Task<IActionResult> AddPrescription(int appointmentId, [FromBody] Prescription request)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return Unauthorized(new { message = "Doctor record not found." });

        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Prescription)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
            
        if (appointment == null) return NotFound(new { message = "Appointment not found." });

        // Security: Ensure the doctor is the one assigned to this appointment
        if (appointment.DoctorId != doctor.Id)
            return Unauthorized(new { message = "This appointment is not assigned to you." });

        if (appointment.Status != "Accepted" && appointment.Status != "Completed")
            return BadRequest(new { message = "Cannot prescribe to a pending or rejected appointment." });

        // Prevent duplicates
        if (appointment.Prescription != null)
            return BadRequest(new { message = "A prescription already exists for this appointment." });

        request.AppointmentId = appointmentId; // Ensure correct ID mapping
        
        // Use navigation property for cleaner EF Core linking
        appointment.Prescription = request;
        appointment.Status = "Completed";

        if (appointment.Pet != null)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == appointment.Pet.PatientId);
            if (patient != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = patient.UserId,
                    Message = $"A digital prescription has been added for {appointment.Pet.Name}."
                });
            }
        }
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "Prescription successfully added." });
    }

    [Authorize(Roles = "Patient")]
    [HttpDelete("{appointmentId}")]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return NotFound();

        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null) return NotFound();
        if (appointment.Pet == null || appointment.Pet.PatientId != patient.Id)
            return Unauthorized(new { message = "Unauthorized to cancel this appointment." });

        if (appointment.Status == "Completed")
            return BadRequest(new { message = "Successfully completed appointments cannot be cancelled." });

        appointment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Appointment cancelled." });
    }
}

public class BookAppointmentRequest
{
    public int PetId { get; set; }
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public string? Symptoms { get; set; }
    public string? Duration { get; set; }
    public string? AiCondition { get; set; }
    public string? AiSeverity { get; set; }
    public int PriorityLevel { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty; // Accepted, Rejected
    public string? RejectionReason { get; set; }
}
