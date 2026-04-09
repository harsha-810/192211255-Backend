using System.Text.Json.Serialization;

namespace PetClinicAPI.Models;

public class Hospital
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
    
    public int AdminId { get; set; }
    public User? Admin { get; set; }

    public List<Doctor> Doctors { get; set; } = new();
}
