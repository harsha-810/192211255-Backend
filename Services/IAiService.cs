using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetClinicAPI.Services;

public interface IAiService
{
    Task<(string condition, string severity, string recommendation, int priorityLevel)> AnalyzeSymptomsAsync(List<string> symptoms, string duration, int petAge, string species, string breed);
}
