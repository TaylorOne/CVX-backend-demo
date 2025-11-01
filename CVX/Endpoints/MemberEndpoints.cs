using CVX.Models;
using CVX.Services;
using CVX.Utilities;
using CVX.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Serilog;


namespace CVX.Endpoints
{
    public static class MemberEndpoints
    {
        public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/members", async (ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var members = await dbContext.Users
                    .Where(u => u.NetworkMemberStatus != CVX.Models.NetworkMemberStatus.DeniedApplicant && u.NetworkMemberStatus != CVX.Models.NetworkMemberStatus.Applicant)
                    .Include(u => u.Skills)
                    .Include(u => u.Sectors)
                    .AsNoTracking()
                    .ToListAsync();

                var result = new List<object>();

                foreach (var m in members)
                {
                    result.Add(new
                    {
                        m.Id,
                        m.UserName,
                        m.FirstName,
                        m.LastName,
                        m.Bio,
                        m.City,
                        m.State,
                        m.PhoneNumber,
                        m.LinkedIn,
                        m.AvatarUrl,
                        CreatedAt = m.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                        Skills = m.Skills?.Select(s => s.Skill).ToList(),
                        Experience = m.Sectors?.Select(e => e.Sector).ToList(),
                        MemberStatus = EnumUtility.GetEnumDisplayName(m.NetworkMemberStatus)
                    });
                }
                return Results.Ok(result);
            })
            .WithOpenApi();

            app.MapGet("members/{id}", async (string id, ApplicationDbContext dbContext) =>
            {
                // Find the user by ID
                var user = await dbContext.Users
                    .Include(u => u.Skills)
                    .Include(u => u.Sectors)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return Results.NotFound(new { message = "User not found" });
                }

                var collaboratives = await dbContext.CollaborativeMembers
                        .Where(cm => cm.UserId == user.Id)
                        .Include(cm => cm.Collaborative)
                        .Where(cm => cm.Collaborative.ApprovalStatus == ApprovalStatus.Active)
                        .Select(cm => cm.Collaborative.Name)
                        .ToListAsync();

                var result = new
                {
                    user.Id,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Bio,
                    user.City,
                    user.State,
                    user.PhoneNumber,
                    user.LinkedIn,
                    user.AvatarUrl,
                    CreatedAt = user.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    collaboratives,
                    Skills = user.Skills?.Select(s => new
                        {
                            id = s.Id,
                            value = s.Skill
                        })
                        .ToList(),
                    Experience = user.Sectors?.Select(s => new
                        {
                            id = s.Id,
                            value = s.Sector
                        })
                        .ToList(),
                    MemberStatus = EnumUtility.GetEnumDisplayName(user.NetworkMemberStatus)
                };
                return Results.Ok(result);
            })
            .WithOpenApi();

            app.MapPatch("/members/{id}", async (string id, [FromBody] UpdateUserStatus updateUserStatus, ApplicationDbContext dbContext, IEmailService emailService) =>
            {
                // Find the user by ID
                var user = await dbContext.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound(new { message = "User not found" });
                }

                if (updateUserStatus.Role == "Denied Applicant")
                {
                    await emailService.SendDeniedApplicantEmailAsync(user, updateUserStatus.ReasonForDenial, (updateUserStatus.NetworkAdmin.FirstName + " " + updateUserStatus.NetworkAdmin.LastName), updateUserStatus.NetworkAdmin.UserName );
                    Log.Information("Updating user {UserId} to Denied Applicant with reason: {Reason} by Network Admin {Name}", id, updateUserStatus.ReasonForDenial, (updateUserStatus.NetworkAdmin.FirstName + " " + updateUserStatus.NetworkAdmin.LastName));
                }

                // Convert the display name to the corresponding enum value
                var newRole = EnumUtility.GetEnumValueFromDisplayName<Models.NetworkMemberStatus>(updateUserStatus.Role);
                if (newRole.HasValue)
                {
                    // update the user's role
                    user.NetworkMemberStatus = newRole.Value;
                    await dbContext.SaveChangesAsync();
                    return Results.Ok(new { message = "User role updated successfully" });
                }
                else
                {
                    return Results.BadRequest(new { message = "Invalid role value" });
                }
            })
            .WithOpenApi();
        }

    }

    public record UpdateUserStatus(
        string? Role,
        string? ReasonForDenial,
        NetworkAdmin? NetworkAdmin
    );

    public record NetworkAdmin(
        string UserId,
        string UserName,
        string FirstName,
        string LastName,
        string AvatarUrl,
        string MemberStatus
    );

}
