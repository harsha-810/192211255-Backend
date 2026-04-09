using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PetClinicAPI.Models;
using PetClinicAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PetClinicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var password = request.Password.Trim();

        Console.WriteLine($"Login Attempt: Email=[{email}]");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        
        if (user == null)
        {
            Console.WriteLine("Login Failed: User not found.");
            return Unauthorized(new { message = "Invalid email or password" });
        }

        if (user.PasswordHash != password)
        {
            Console.WriteLine($"Login Failed: Password mismatch for {email}.");
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var name = "User";
        if (user.Role == "Patient")
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            name = patient?.Name ?? "Patient";
        }
        else if (user.Role == "Doctor")
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            name = doctor?.Name ?? "Doctor";
        }
        else if (user.Role == "Admin")
        {
            name = "Administrator";
        }

        Console.WriteLine($"Login Success: {user.Email} (Role: {user.Role}, Name: {name})");
        var token = GenerateJwtToken(user);
        return Ok(new { token, role = user.Role, userId = user.Id, name = name });
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in token" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (user.PasswordHash != request.OldPassword)
        {
            return BadRequest(new { message = "Incorrect old password" });
        }

        user.PasswordHash = request.NewPassword;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully." });
    }

    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var password = request.Password.Trim();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "Email already exists" });

        var user = new User { Email = email, PasswordHash = password, Role = "Admin" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin registered successfully." });
    }

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var password = request.Password.Trim();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "Email already exists" });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User { Email = email, PasswordHash = password, Role = "Patient" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var patient = new Patient { Name = request.Name, Phone = request.Phone, UserId = user.Id };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { message = "Patient registered successfully." });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error during patient registration.");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        
        if (user == null)
        {
            // Don't reveal if user exists or not for security, but for PetClinic let's be helpful
            return NotFound(new { message = "User with this email not found." });
        }

        var otp = new Random().Next(100000, 999999).ToString();
        user.ResetOtp = otp;
        user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
        var body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #008080;'>PetClinic Password Reset</h2>
                <p>Hello,</p>
                <p>You requested to reset your password. Use the following OTP to proceed. This code is valid for 15 minutes.</p>
                <div style='font-size: 24px; font-weight: bold; color: #008080; padding: 10px; background: #f0fafa; display: inline-block; border-radius: 5px;'>
                    {otp}
                </div>
                <p>If you didn't request this, please ignore this email.</p>
                <p>Best regards,<br/>The PetClinic Team</p>
            </div>";

        var sent = await emailService.SendEmailAsync(user.Email, "PetClinic Password Reset OTP", body);

        if (!sent)
        {
            return StatusCode(500, new { message = "Failed to send OTP email. Please try again later." });
        }

        return Ok(new { message = "OTP sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var receivedOtp = request.Otp?.Trim();
        Console.WriteLine($"[OTP-DEBUG] Attempt for {email}: Stored='{user.ResetOtp}', Received='{receivedOtp}'");
        Console.WriteLine($"[OTP-DEBUG] Expiry: {user.ResetOtpExpiry}, CurrentTime (UTC): {DateTime.UtcNow}");

        if (user.ResetOtp != receivedOtp || user.ResetOtpExpiry < DateTime.UtcNow)
        {
            Console.WriteLine($"[OTP-DEBUG] Validation Failed. Match: {user.ResetOtp == receivedOtp}, Expired: {user.ResetOtpExpiry < DateTime.UtcNow}");
            return BadRequest(new { message = "Invalid or expired OTP." });
        }

        user.PasswordHash = request.NewPassword;
        user.ResetOtp = null; // Clear OTP after use
        user.ResetOtpExpiry = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully." });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "SecretKey";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("role", user.Role), // Add as "role" for easier parsing/debugging
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7), // Extend expiry for better dev experience
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RegisterPatientRequest : LoginRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
