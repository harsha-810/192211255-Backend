using System.Text.Json.Serialization;

namespace PetClinicAPI.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
    public List<Vaccination> Vaccinations { get; set; } = new();
}
