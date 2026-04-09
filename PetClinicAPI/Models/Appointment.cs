namespace PetClinicAPI.Models;

public class Appointment
{
    public int Id { get; set; }
    
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTime Date { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Completed
    public string? RejectionReason { get; set; }

    // AI Checking
    public string? Symptoms { get; set; }
    public string? Duration { get; set; }
    public string? AiCondition { get; set; }
    public string? AiSeverity { get; set; } 
    public int PriorityLevel { get; set; } // 1 (High), 2 (Medium), 3 (Low)
    
    public Prescription? Prescription { get; set; }
}
