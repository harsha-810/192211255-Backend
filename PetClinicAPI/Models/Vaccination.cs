using System.Text.Json.Serialization;

namespace PetClinicAPI.Models;

public class Vaccination
{
    public int Id { get; set; }
    public int PetId { get; set; }
    
    [JsonIgnore]
    public Pet? Pet { get; set; }

    public string VaccineName { get; set; } = string.Empty;
    public DateTime LastDate { get; set; }
    public DateTime NextDueDate { get; set; }
}
