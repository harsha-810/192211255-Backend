using System.Text.Json.Serialization;

namespace PetClinicAPI.Models;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;

    public int UserId { get; set; }
    public User? User { get; set; }

    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    [JsonIgnore]
    public List<Appointment> Appointments { get; set; } = new();
}
