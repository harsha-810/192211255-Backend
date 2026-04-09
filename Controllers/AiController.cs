using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using PetClinicAPI.Services;
using PetClinicAPI.Models;

namespace PetClinicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly AppDbContext _context;

    public AiController(IAiService aiService, AppDbContext context)
    {
        _aiService = aiService;
        _context = context;
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckCondition([FromBody] AiCheckRequest request)
    {
        if (request.Symptoms == null || !request.Symptoms.Any())
        {
            return BadRequest(new { message = "At least one symptom is required." });
        }

        // Fetch pet details for context
        var pet = await _context.Pets.FindAsync(request.PetId);
        string species = pet?.Species ?? "Unknown";
        string breed = pet?.Breed ?? "Unknown";
        int age = pet?.Age ?? 0;

        var (condition, severity, recommendation, priorityLevel) = await _aiService.AnalyzeSymptomsAsync(
            request.Symptoms, 
            request.Duration, 
            age, 
            species, 
            breed
        );

        return Ok(new AiCheckResponse
        {
            Condition = condition,
            Severity = severity,
            Recommendation = recommendation,
            PriorityLevel = priorityLevel
        });
    }
}

public class AiCheckRequest
{
    public int PetId { get; set; }
    
    [Required]
    public List<string> Symptoms { get; set; } = new();
    
    public string Duration { get; set; } = string.Empty;
}

public class AiCheckResponse
{
    public string Condition { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public int PriorityLevel { get; set; } = 3; // 1 = High, 2 = Medium, 3 = Low
}
