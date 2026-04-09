using System.Threading.Tasks;

namespace PetClinicAPI.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
}
