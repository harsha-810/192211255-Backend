namespace PetClinicAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Admin", "Doctor", "Patient"
    public string? ResetOtp { get; set; }
    public DateTime? ResetOtpExpiry { get; set; }
}
