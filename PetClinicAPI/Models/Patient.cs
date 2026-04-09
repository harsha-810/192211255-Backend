using System.Text.Json.Serialization;

namespace PetClinicAPI.Models;

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    [JsonIgnore]
    public List<Pet> Pets { get; set; } = new();
}
