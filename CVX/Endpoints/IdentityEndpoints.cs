using CVX.Models;
using CVX.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CVX.Endpoints
{
    public static class IdentityEndpoints
    {
        public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            // Register
            app.MapPost("/register", async (
                UserManager<ApplicationUser> userManager,
                [FromBody] RegisterDto dto) =>
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Bio = dto.Bio,
                    City = dto.City,
                    State = dto.State,
                    PhoneNumber = dto.PhoneNumber,
                    LinkedIn = dto.LinkedIn,
                };

                var result = await userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return Results.BadRequest(new { message = result.Errors.FirstOrDefault()?.Description });
                    
                return Results.Ok(new { message = "User registered successfully." });
            })
            .WithOpenApi();

            // Login
            app.MapPost("/login", async (
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                [FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies) =>
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(login.Email);
                if (user == null)
                    return Results.BadRequest(new { message = "Invalid login attempt." });

                // Check if user is DeniedApplicant
                if (user.NetworkMemberStatus == NetworkMemberStatus.DeniedApplicant)
                    return Results.BadRequest(new { message = "Your account has been denied. Please contact support if you believe this is an error." });

                signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

                var isPersistent = (useCookies == true) && (useSessionCookies != true);
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, false);

                if (!result.Succeeded)
                    return Results.BadRequest(new { message = "Invalid login attempt." });

                return Results.Ok(new { message = "Login successful." });
            })
            .WithOpenApi();

            app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager,
                [FromBody] object empty) =>
            {
                if (empty != null)
                {
                    await signInManager.SignOutAsync();
                    return Results.Ok();
                }
                return Results.Unauthorized();
            })
            .WithOpenApi()
            .RequireAuthorization();

            // Forgot Password
            app.MapPost("/forgotPassword", async (
                UserManager<ApplicationUser> userManager,
                [FromBody] ForgotPasswordDto dto,
                [FromServices] IEmailService emailService) =>
            {
                var user = await userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Results.Ok(new { message = "If the email is registered, a reset link will be sent." });

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = $"https://c-vx-mantine-ui-knwx.vercel.app/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

                await emailService.SendEmailAsync(
                    user.Email,
                    "Reset your CVx password",
                    $@"
                        <img src=""https://c-vx-mantine-ui-knwx.vercel.app/assets/CVxLogoWhite.png"" alt=""CVx Logo"" style=""height:60px;""><br><br>
                        <h2>Password Reset Request</h2>
                        <p>We received a request to reset the password for your CVx account.</p>
                        <p>To reset your password, click the link below:</p>
                        <p>
                            <a href=""{resetLink}"">Reset your password</a>
                        </p>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p>{resetLink}</p>
                        <hr>
                        <p style=""font-size:small;color:gray;"">
                            If you did not request a password reset, you can safely ignore this email.<br>
                            This password reset link will expire in 24 hours.
                        </p>
                    "
                );

                return Results.Ok(new { message = "If the email is registered, a reset link will be sent." });
            })
            .WithOpenApi();

            // Reset Password
            app.MapPost("/resetPassword", async (
                UserManager<ApplicationUser> userManager,
                [FromBody] ResetPasswordDto dto) =>
            {
                var user = await userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Results.BadRequest(new { message = "Invalid request." });

                var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors);

                return Results.Ok(new { message = "Password has been reset successfully." });
            })
            .WithOpenApi();
        }

        // DTOs for login and password reset
        public record LoginRequest(string Email, string Password);
        public record ForgotPasswordDto(string Email);
        public record ResetPasswordDto(string Email, string Token, string NewPassword);

        public record RegisterDto(
            string Email,
            string Password,
            string FirstName,
            string LastName,
            string Bio,
            string City,
            string State,
            string PhoneNumber,
            string LinkedIn
        );
    }
}
