namespace PetClinicAPI.Models;

public class Prescription
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Appointment? Appointment { get; set; }

    public string Diagnosis { get; set; } = string.Empty;
    public string Medicines { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}
