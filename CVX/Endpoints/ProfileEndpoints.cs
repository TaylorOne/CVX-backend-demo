using CVX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace CVX.Endpoints
{
    public static class ProfileEndpoints
    {
        public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/profile", async (ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var username = user.Identity.Name;
                    
                    var applicationUser = await userManager.Users
                        .Include(u => u.Skills)
                        .Include(u => u.Sectors)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserName == username);

                    var userId = await userManager.GetUserIdAsync(applicationUser);

                    var collaboratives = await dbContext.CollaborativeMembers
                        .Where(cm => cm.UserId == userId)
                        .Include(cm => cm.Collaborative)
                        .Where(cm => cm.Collaborative.ApprovalStatus == ApprovalStatus.Active)
                        .Select(cm => cm.Collaborative.Name)
                        .ToListAsync();

                    if (applicationUser != null)
                    {
                        return Results.Ok(new
                        {
                            UserId = applicationUser.Id,
                            applicationUser.UserName,
                            applicationUser.FirstName,
                            applicationUser.LastName,
                            applicationUser.Bio,
                            applicationUser.City,
                            applicationUser.State,
                            applicationUser.PhoneNumber,
                            applicationUser.LinkedIn,
                            applicationUser.AvatarUrl,
                            CreatedAt = applicationUser.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                            collaboratives,
                            Skills = applicationUser.Skills?.Select(s => new
                                { 
                                    id = s.Id,
                                    value = s.Skill
                                })
                                .ToList(),
                            Experience = applicationUser.Sectors?.Select(s => new
                                {
                                    id = s.Id,
                                    value = s.Sector
                                })
                                .ToList(),
                            MemberStatus = GetEnumDisplayName(applicationUser.NetworkMemberStatus)
                        });
                    }
                }
                return Results.Unauthorized();
            })
            .WithOpenApi();


            app.MapPatch("/profile", async (ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager, [FromBody] UpdateProfileRequest updateRequest) =>
            {
                Log.Information($"UpdateProfileRequest: {updateRequest.FirstName}, {updateRequest.LastName}, {updateRequest.UserName}, {updateRequest.Bio}, {updateRequest.PhoneNumber}, {updateRequest.LinkedIn}, {updateRequest.AvatarUrl}");

                var user = httpContext.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var username = user.Identity.Name;
                    var applicationUser = await userManager.Users
                        .Include(u => u.Skills)
                        .Include(u => u.Sectors)
                        .FirstOrDefaultAsync(u => u.UserName == username);

                    var skills = applicationUser.Skills
                        .Select(s => s.Id )
                        .ToList();

                    var experience = applicationUser.Sectors
                        .Select(s => s.Id)
                        .ToList();

                    if (applicationUser != null)
                    {
                        // Update the user's profile
                        if (updateRequest.FirstName != null)
                            applicationUser.FirstName = updateRequest.FirstName;
                        if (updateRequest.LastName != null)
                            applicationUser.LastName = updateRequest.LastName;
                        if (updateRequest.Bio != null)
                            applicationUser.Bio = updateRequest.Bio;
                        if (updateRequest.City != null)
                            applicationUser.City = updateRequest.City;
                        if (updateRequest.State != null)
                            applicationUser.State = updateRequest.State;
                        if (updateRequest.PhoneNumber != null)
                            applicationUser.PhoneNumber = updateRequest.PhoneNumber;
                        if (updateRequest.LinkedIn != null)
                            applicationUser.LinkedIn = updateRequest.LinkedIn;
                        if (updateRequest.AvatarUrl != null)
                            applicationUser.AvatarUrl = updateRequest.AvatarUrl;

                        if (updateRequest.Skills != null && updateRequest.Skills.Length > 0)
                        {
                            // Add new skills
                            foreach (var skillId in updateRequest.Skills)
                            {
                                if (skills.Contains(skillId))
                                {
                                    // Skip if the skill already exists
                                    continue;
                                }

                                var dbSkill = await dbContext.Skills.FindAsync(skillId);
                                if (dbSkill != null)
                                {
                                    applicationUser.Skills.Add(dbSkill);
                                }
                            }

                            // Remove skills
                            foreach (var skillId in skills)
                            {
                                if (updateRequest.Skills != null && updateRequest.Skills.Contains(skillId))
                                {
                                    // Skip if the skill is still in the list
                                    continue;
                                }
                                var dbSkill = await dbContext.Skills.FindAsync(skillId);
                                if (dbSkill != null)
                                {
                                    applicationUser.Skills.Remove(dbSkill);
                                }
                            }
                        }

                        if (updateRequest.Experience != null && updateRequest.Experience.Length > 0)
                        {

                            // Add new experience
                            foreach (var sectorId in updateRequest.Experience)
                            {
                                if (experience.Contains(sectorId))
                                {
                                    // Skip if the sector already exists
                                    continue;
                                }

                                var dbSector = await dbContext.Sectors.FindAsync(sectorId);
                                if (dbSector != null)
                                {
                                    applicationUser.Sectors.Add(dbSector);
                                }
                            }

                            // Remove sectors
                            foreach (var sectorId in experience)
                            {
                                if (updateRequest.Experience != null && updateRequest.Experience.Contains(sectorId))
                                {
                                    // Skip if the sector is still in the list
                                    continue;
                                }
                                var dbSector = await dbContext.Sectors.FindAsync(sectorId);
                                if (dbSector != null)
                                {
                                    applicationUser.Sectors.Remove(dbSector);
                                }
                            }
                        }

                        var result = await userManager.UpdateAsync(applicationUser);

                        return Results.Ok(new { message = "Profile updated successfully" });

                        if (result.Succeeded)
                        {
                            return Results.Ok(new
                            {
                                message = "Profile updated successfully",
                                Username = applicationUser.UserName,
                                FirstName = applicationUser.FirstName,
                                LastName = applicationUser.LastName,
                                Bio = applicationUser.Bio,
                                LinkedIn = applicationUser.LinkedIn,
                                AvatarUrl = applicationUser.AvatarUrl,
                                CreatedAt = applicationUser.CreatedAt,
                            });
                        }
                        else
                        {
                            return Results.BadRequest(new { message = "Failed to update profile", errors = result.Errors });
                        }
                    }
                }
                return Results.Unauthorized();
            })
            .WithOpenApi();
        }
        public static string GetEnumDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                ?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }

    public record UpdateProfileRequest(
        string? FirstName,
        string? LastName,
        string? UserName,
        string? Bio,
        string? City,
        string? State,
        string? PhoneNumber,
        string? LinkedIn,
        string? AvatarUrl,
        string? CreatedAt,
        int[]? Skills,
        int[]? Experience);
}
