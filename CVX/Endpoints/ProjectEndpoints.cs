using CVX.Models;
using CVX.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Serilog;
using System.Globalization;

namespace CVX.Endpoints
{
    public static class ProjectEndpoints
    {
        public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("projects", async (ApplicationDbContext dbContext, [FromBody] ProjectDTO projectDTO) =>
            {
                if (string.IsNullOrWhiteSpace(projectDTO.AdminId))
                {
                    return Results.BadRequest(new { message = "ProjectAdminId is required." });
                }

                // Check if the collaborative exists
                var collaborative = await dbContext.Collaboratives.FindAsync(projectDTO.CollabId);
                if (collaborative == null)
                {
                    return Results.BadRequest(new { message = "Invalid CollabId: collaborative not found." });
                }

                // Check if a project with the same name already exists in the collaborative
                var existingProject = await dbContext.Projects
                    .Where(p => p.CollaborativeId == projectDTO.CollabId && p.Name == projectDTO.Name)
                    .FirstOrDefaultAsync();

                if (existingProject != null)
                {
                    return Results.BadRequest(new { message = "A project with the same name already exists in this collaborative." });
                }

                var project = new Project
                {
                    Name = projectDTO.Name ?? "Untitled Project",
                    Description = projectDTO.Description ?? string.Empty,
                    CollaborativeId = projectDTO.CollabId,
                    Collaborative = collaborative,
                    LaunchTokenBudget = projectDTO.Budget,
                    LaunchTokenBalance = projectDTO.Budget - projectDTO.AdminPay,
                    ProjectAdminCompensationLaunchTokens = projectDTO.AdminPay,
                };

                // Add the project to the database context
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();

                var projectMember = await dbContext.Users.FindAsync(projectDTO.AdminId);

                // Add the project member with ProjectAdmin role
                var projectAdmin = new ProjectMember
                {
                    UserId = projectDTO.AdminId,
                    Role = ProjectRole.ProjectAdmin,
                    InviteStatus = InviteStatus.Invited,
                    ProjectId = project.Id,
                };

                dbContext.ProjectMembers.Add(projectAdmin);

                // Update token balance in collaborative
                collaborative.LaunchTokensBalance -= project.LaunchTokenBudget;

                await dbContext.SaveChangesAsync();

                return Results.Ok(new { message = "Project created successfully!" });
            })
            .WithOpenApi();

            app.MapPatch("projects/{id}", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager < ApplicationUser > userManager, [FromBody] UpdateProjectDTO updateProjectDTO) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .Include(c => c.Collaborative)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                var message = "Project updated successfully!";

                if (!string.IsNullOrEmpty(updateProjectDTO.Name))
                {
                    // check if a project with the same name already exists in the collaborative
                    var existingProject = await dbContext.Projects
                        .Where(p => p.CollaborativeId == project.CollaborativeId && p.Name == updateProjectDTO.Name && p.Id != project.Id)
                        .FirstOrDefaultAsync();

                    if (existingProject != null)
                    {
                        return Results.BadRequest("A project with the same name already exists in this collaborative.");
                    }
                    
                    project.Name = updateProjectDTO.Name;
                }

                if (!string.IsNullOrEmpty(updateProjectDTO.Description))
                {
                    project.Description = updateProjectDTO.Description;
                }

                if (updateProjectDTO.Budget.HasValue && updateProjectDTO.Budget != project.LaunchTokenBudget)
                {
                    // Update the LaunchTokenBudget and adjust the LaunchTokenBalance accordingly
                    var budgetDifference = updateProjectDTO.Budget.Value - project.LaunchTokenBudget;

                    if (budgetDifference > 0)
                    {
                        // Ensure the collaborative has enough LaunchTokensBalance to cover the increase
                        var collab = await dbContext.Collaboratives.FindAsync(project.CollaborativeId);
                        if (collab == null || collab.LaunchTokensBalance < budgetDifference)
                        {
                            return Results.BadRequest("Insufficient LaunchTokensBalance in the collaborative to increase the project budget.");
                        }

                    }
                    else if (budgetDifference < 0 && Math.Abs(budgetDifference) > project.LaunchTokenBalance)
                    {
                        // Prevent reducing the budget below the already allocated amount
                        return Results.BadRequest("Cannot reduce the project budget below the already allocated amount.");
                    }

                    project.LaunchTokenBudget = updateProjectDTO.Budget.Value;
                    project.LaunchTokenBalance += budgetDifference;

                    // Also update the collaborative's LaunchTokensBalance
                    var collaborative = await dbContext.Collaboratives.FindAsync(project.CollaborativeId);
                    if (collaborative != null)
                    {
                        collaborative.LaunchTokensBalance -= budgetDifference;
                        dbContext.Collaboratives.Update(collaborative);
                    }
                }

                if (updateProjectDTO.AdminPay.HasValue && updateProjectDTO.AdminPay != project.ProjectAdminCompensationLaunchTokens)
                {
                    var adminPayDifference = updateProjectDTO.AdminPay.Value - project.ProjectAdminCompensationLaunchTokens;
                    project.ProjectAdminCompensationLaunchTokens = updateProjectDTO.AdminPay.Value;

                    // Adjust the LaunchTokenBalance accordingly
                    project.LaunchTokenBalance -= adminPayDifference;
                }


                // everything below is just for returning the updated project object to the front-end


                var projectAdmin = project?.ProjectMembers
                    .FirstOrDefault(pm => pm.Role == ProjectRole.ProjectAdmin);

                bool userIsProjectAdmin = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin
                    // so front-end knows whether to display the "Submit for Approval" button or not
                    userIsProjectAdmin = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.ProjectId == project.Id && m.Role == ProjectRole.ProjectAdmin);
                }

                // collect reasons any project members may have declined the project
                var reasonsForDecline = project.ProjectMembers
                    .Where(m => !string.IsNullOrEmpty(m.ReasonForProjectDecline))
                    .Select((m, idx) => new
                    {
                        Id = idx + 1,
                        MemberName = m.User.FirstName + " " + m.User.LastName,
                        Reason = m.ReasonForProjectDecline
                    })
                    .ToList();

                if (reasonsForDecline.Any())
                {
                    Log.Information("Project has been declined by the following members:");
                    foreach (var reason in reasonsForDecline)
                    {
                        Log.Information($"- {reason.MemberName}: {reason.Reason}");
                    }
                }

                // return a new project object so data is refreshed immediately on front-end
                var result = new
                {
                    CollabId = project.CollaborativeId,
                    CollabName = project.Collaborative.Name,
                    CollabLogoUrl = project.Collaborative.LogoUrl,
                    project.Id,
                    project.Name,
                    project.Description,
                    CreatedAt = project.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    Budget = project.LaunchTokenBudget,
                    Balance = project.LaunchTokenBalance,
                    ApprovalStatus = EnumUtility.GetEnumDisplayName(project.ApprovalStatus),
                    AdminName = projectAdmin?.User.FirstName + " " + projectAdmin?.User.LastName,
                    AdminEmail = projectAdmin?.User.Email,
                    AdminPay = project.ProjectAdminCompensationLaunchTokens,
                    UserIsProjectAdmin = userIsProjectAdmin,
                    AllUsersAcceptedTheirInvites = project.ProjectMembers.All(m => m.InviteStatus == InviteStatus.Accepted),
                    reasonsForDecline,
                };

                // Save changes to the database
                await dbContext.SaveChangesAsync();
                return Results.Ok(result);

            })
            .WithOpenApi();

            // Project home page
            app.MapGet("projects/{id}", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(p => p.User)
                    .Include(p => p.Milestones)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                var projectAdmin = project?.ProjectMembers
                    .FirstOrDefault(pm => pm.Role == ProjectRole.ProjectAdmin);

                var collab = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == project.CollaborativeId);

                if (collab == null || collab.Id != project.CollaborativeId)
                {
                    return Results.BadRequest("Invalid CollabId: collaborative does not match the project.");
                }

                bool userIsProjectAdmin = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin
                    // so front-end knows whether to display the "Submit for Approval" button or not
                    userIsProjectAdmin = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.ProjectId == project.Id && m.Role == ProjectRole.ProjectAdmin);
                }

                // collect reasons any project members may have declined the project
                var reasonsForDecline = project.ProjectMembers
                    .Where(m => !string.IsNullOrEmpty(m.ReasonForProjectDecline))
                    .Select((m,idx) => new
                    {
                        Id = idx + 1,
                        MemberId = m.UserId,
                        MemberName = m.User.FirstName + " " + m.User.LastName,
                        Reason = m.ReasonForProjectDecline
                    })
                    .ToList();

                // collect reasons any project members may have declined their invitations to the project
                var reasonsForInviteDecline = project.ProjectMembers
                    .Where(m => !string.IsNullOrEmpty(m.ReasonForProjectInviteDecline))
                    .Select((m, idx) => new
                    {
                        Id = idx + 1,
                        MemberId = m.UserId,
                        MemberName = m.User.FirstName + " " + m.User.LastName,
                        MemberRole = EnumUtility.GetEnumDisplayName(m.Role),
                        Reason = m.ReasonForProjectInviteDecline
                    })
                    .ToList();

                var userIsCollabAdmin = collab.CollaborativeMembers
                    .Where(c => c.Role == CollaborativeRole.CollaborativeAdmin)
                    .Any(c => c.UserId == userManager.GetUserId(user));

                var result = new
                {
                    CollabId = project.CollaborativeId,
                    CollabName = collab.Name,
                    CollabLogoUrl = collab.LogoUrl,
                    CollabLaunchTokenBalance = CollaborativeEndpoints.CalculateCollabTokenBalance(dbContext,collab),
                    project.Id,
                    project.Name,
                    project.Description,
                    CreatedAt = project.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    Budget = project.LaunchTokenBudget,
                    Balance = project.LaunchTokenBalance,
                    SumMilestonesAllocatedLaunchTokens = project.Milestones.Sum(m => m.AllocatedLaunchTokens),
                    networkTransactionFee = NetworkConstants.TransactionFee,
                    ApprovalStatus = EnumUtility.GetEnumDisplayName(project.ApprovalStatus),
                    AdminName = projectAdmin?.User.FirstName + " " + projectAdmin?.User.LastName,
                    AdminEmail = projectAdmin?.User.Email,
                    AdminPay = project.ProjectAdminCompensationLaunchTokens,
                    UserIsProjectAdmin = userIsProjectAdmin,
                    AllUsersAcceptedTheirInvites = project.ProjectMembers.All(m => m.InviteStatus == InviteStatus.Accepted),
                    reasonsForDecline,
                    reasonsForInviteDecline,
                    userIsCollabAdmin,
                };

                return Results.Ok(result);
            });

            // Project members page
            app.MapGet("projects/{id}/members", async (ApplicationDbContext dbContext, int id, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var project = await dbContext.Projects
                    .Include(c => c.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                bool userIsProjectAdminAndStatusAccepted = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin AND they've accepted their invite
                    // this info is necessary so front-end knows whether to display the edit button or not
                    userIsProjectAdminAndStatusAccepted = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.Role == ProjectRole.ProjectAdmin && m.InviteStatus == InviteStatus.Accepted);
                }

                var collaborative = await dbContext.Collaboratives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == project.CollaborativeId);

                var members = project.ProjectMembers
                    .Select(pm => new
                    {
                        pm.User.Id,
                        pm.User.UserName,
                        pm.User.FirstName,
                        pm.User.LastName,
                        pm.User.AvatarUrl,
                        Role = EnumUtility.GetEnumDisplayName(pm.Role),
                        InviteStatus = Enum.GetName(typeof(InviteStatus), pm.InviteStatus),
                        pm.IsActive,
                        CreatedAt = pm.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)
                    });

                var result = new
                {
                    CollabName = collaborative?.Name,
                    CollabLogoUrl = collaborative?.LogoUrl,
                    project.Id,
                    project.Name,
                    userIsProjectAdminAndStatusAccepted,
                    Members = members
                };

                return Results.Ok(result);
            })
            .WithOpenApi();

            app.MapPost("projects/{id}/members", async (ApplicationDbContext dbContext, int id, HttpContext httpContext, UserManager < ApplicationUser > userManager, [FromBody] UpdateProjectDTO updateProjectDTO) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                if (string.IsNullOrEmpty(updateProjectDTO.UserId))
                {
                    return Results.BadRequest("UserId is required.");
                }

                var member = project.ProjectMembers
                    .FirstOrDefault(m => m.UserId == updateProjectDTO.UserId);

                if (member == null)
                {
                    // Add new member
                    member = new ProjectMember
                    {
                        UserId = updateProjectDTO.UserId,
                        Role = EnumUtility.GetEnumValueFromDisplayName<ProjectRole>(updateProjectDTO.UserRole) ?? ProjectRole.ProjectMember,
                        ProjectId = id,
                    };
                    dbContext.ProjectMembers.Add(member);
                }

                // check and see if the project status is Active: if so it needs to be reset to submitted
                if (project.ApprovalStatus == ApprovalStatus.Active)
                {
                    project.ApprovalStatus = ApprovalStatus.Submitted;
                    dbContext.Projects.Update(project);
                }

                bool userIsProjectAdminAndStatusAccepted = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin AND they've accepted their invite
                    // this info is necessary so front-end knows whether to display the edit button or not
                    userIsProjectAdminAndStatusAccepted = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.Role == ProjectRole.ProjectAdmin && m.InviteStatus == InviteStatus.Accepted);
                }

                await dbContext.SaveChangesAsync();

                var collaborative = await dbContext.Collaboratives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == project.CollaborativeId);

                // Reload the project and its members
                var updatedProject = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                var members = updatedProject.ProjectMembers
                    .Select(pm => new
                    {
                        pm.User.Id,
                        pm.User.UserName,
                        pm.User.FirstName,
                        pm.User.LastName,
                        pm.User.AvatarUrl,
                        Role = EnumUtility.GetEnumDisplayName(pm.Role),
                        InviteStatus = Enum.GetName(typeof(InviteStatus), pm.InviteStatus),
                        pm.IsActive,
                        CreatedAt = pm.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)
                    });

                // Send back updated data for page reload
                var result = new
                {
                    CollabName = collaborative?.Name,
                    CollabLogoUrl = collaborative?.LogoUrl,
                    project.Id,
                    project.Name,
                    userIsProjectAdminAndStatusAccepted,
                    Members = members
                };

                return Results.Ok(result);
            })
            .WithOpenApi();

            // edit a project member's invite status
            app.MapPatch("projects/{id}/members/{userId}", async (ApplicationDbContext dbContext, int id, string userId, [FromBody] UpdateProjectDTO updateProjectDTO) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest("UserId is required.");
                }

                var member = project.ProjectMembers
                    .FirstOrDefault(m => m.UserId == userId);

                if (member == null)
                {
                    return Results.NotFound("Project member not found.");
                }

                if (!string.IsNullOrEmpty(updateProjectDTO.InviteStatus))
                {
                    if (Enum.TryParse<InviteStatus>(updateProjectDTO.InviteStatus, out var parsedInviteStatus))
                    {
                        // if member is being reinvited to the project change inviteStatus and clear reasonForDecline
                        if (parsedInviteStatus == InviteStatus.Invited && member.InviteStatus == InviteStatus.Declined)
                        {
                            // remove any previous reason for decline
                            member.ReasonForProjectInviteDecline = null;
                        }

                        member.InviteStatus = parsedInviteStatus;

                        if (member.InviteStatus == InviteStatus.Declined && !string.IsNullOrEmpty(updateProjectDTO.ReasonForDecline))
                        {
                            member.ReasonForProjectInviteDecline = updateProjectDTO.ReasonForDecline;
                        }
                    }
                    else
                    {
                        return Results.BadRequest("Invalid InviteStatus value.");
                    }
                }

                dbContext.ProjectMembers.Update(member);
                await dbContext.SaveChangesAsync();
                return Results.Ok(new { message = "Project member updated successfully!" });
            })
            .WithOpenApi();

            // Project milestones page
            app.MapGet("projects/{id}/milestones", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.Milestones)
                        .ThenInclude(m => m.Assignee)
                            .ThenInclude(a => a.User)
                    .Include(p => p.ProjectMembers)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                bool userIsProjectAdminAndStatusAccepted = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin AND they've accepted their invite
                    // this info is necessary so front-end knows whether to display the edit button or not
                    userIsProjectAdminAndStatusAccepted = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.Role == ProjectRole.ProjectAdmin && m.InviteStatus == InviteStatus.Accepted);
                }

                var collaborative = await dbContext.Collaboratives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == project.CollaborativeId);

                var milestones = project.Milestones
                    .Select(pm => new
                    {
                        pm.Id,
                        pm.Name,
                        pm.Description,
                        pm.AllocatedLaunchTokens,
                        AssigneeName = pm.Assignee.User.FirstName + " " + pm.Assignee.User.LastName,
                        AssigneeStatus = EnumUtility.GetEnumDisplayName(pm.AssigneeStatus),
                        ApprovalStatus = EnumUtility.GetEnumDisplayName(pm.ApprovalStatus),
                        CreatedAt = pm.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)
                    });

                var launchTokenBalance = project.LaunchTokenBudget - milestones.Sum(m => m.AllocatedLaunchTokens) * (1 + NetworkConstants.TransactionFee);

                var result = new
                {
                    CollabName = collaborative?.Name,
                    CollabLogoUrl = collaborative?.LogoUrl,
                    project.Id,
                    project.Name,
                    project.LaunchTokenBudget,
                    project.ProjectAdminCompensationLaunchTokens,
                    launchTokenBalance,
                    userIsProjectAdminAndStatusAccepted,
                    NetworkTransactionFeeRate = NetworkConstants.TransactionFee,
                    Milestones = milestones
                };

                return Results.Ok(result);
            })
            .WithOpenApi();

            // Submit project for approval
            app.MapPost("projects/{id}/submit", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (project == null)
                {
                    return Results.NotFound(new { message = "Project not found." });
                }

                if (project.ApprovalStatus != ApprovalStatus.Draft && project.ApprovalStatus != ApprovalStatus.Declined)
                {
                    return Results.BadRequest(new { message = "Only projects in Draft or Declined status can be submitted for approval." });
                }

                var membersWhoDeclinedProject = project.ProjectMembers
                    .Where(m => !string.IsNullOrEmpty(m.ReasonForProjectDecline))
                    .ToList();

                foreach (var member in membersWhoDeclinedProject)
                {
                    // remove their reason for declining since project is being resubmitted
                    member.ReasonForProjectDecline = null;
                    dbContext.ProjectMembers.Update(member);
                }

                bool userIsProjectAdminAndStatusNotActive = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a project admin AND they haven't yet approved the project
                    // if so they're submitting the project for approval: make them active immediately (since they're obviously ready to approve it)
                    userIsProjectAdminAndStatusNotActive = project.ProjectMembers
                        .Any(m => m.UserId == userId && m.Role == ProjectRole.ProjectAdmin && m.InviteStatus == InviteStatus.Accepted && m.IsActive == false);
                }

                project.ApprovalStatus = ApprovalStatus.Submitted;

                string message = "submitted";

                if (userIsProjectAdminAndStatusNotActive)
                {
                    var projectAdmin = project.ProjectMembers
                        .FirstOrDefault(pm => pm.Role == ProjectRole.ProjectAdmin && pm.UserId == userManager.GetUserId(user));

                    if (projectAdmin != null)
                    {
                        projectAdmin.IsActive = true;

                        // if there are no other members, make the project active immediately
                        if (project.ProjectMembers.All(m => m.Role == ProjectRole.ProjectAdmin))
                        {
                            project.ApprovalStatus = ApprovalStatus.Active;
                            message = "active";
                        }

                        dbContext.ProjectMembers.Update(projectAdmin);
                    }
                }

                await dbContext.SaveChangesAsync();

                return Results.Ok(new { message });
            })
            .WithOpenApi();


            // member approval of project
            app.MapPatch("projects/{id}/status", async (int id, ApplicationDbContext dbContext, [FromBody] ProjectStatusDTO projectStatusDTO) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                        .ThenInclude(pm => pm.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (project == null)
                {
                    return Results.NotFound(new { message = "Project not found." });
                }

                if (string.IsNullOrEmpty(projectStatusDTO.Status))
                {
                    return Results.BadRequest(new { message = "Status is required." });
                }

                if (string.IsNullOrEmpty(projectStatusDTO.UserId))
                {
                    return Results.BadRequest(new { message = "UserId is required." });
                }

                if (project.ProjectMembers.All(m => m.UserId != projectStatusDTO.UserId))
                {
                    return Results.BadRequest(new { message = "User is not a member of this project." });
                }

                var projectMember = project.ProjectMembers
                        .FirstOrDefault(pm => pm.UserId == projectStatusDTO.UserId);

                if (projectStatusDTO.Status == "approve")
                {
                    
                    projectMember.IsActive = true;

                    // if member has previously declined remove their reason for declining
                    projectMember.ReasonForProjectDecline = null;

                    string message = "You have approved the project";

                    // check to see if all members have approved the project
                    if (project.ProjectMembers.All(m => m.IsActive))
                    {
                        project.ApprovalStatus = ApprovalStatus.Active;
                        message += " All members have approved the project. Project status is now Active.";
                    }

                    await dbContext.SaveChangesAsync();
                    return Results.Ok(new { message });
                }
                else if (projectStatusDTO.Status == "decline")
                {
                    project.ApprovalStatus = ApprovalStatus.Declined;
                    projectMember.ReasonForProjectDecline = projectStatusDTO.ReasonForDecline;

                    await dbContext.SaveChangesAsync();
                    return Results.Ok(new { message = "You have declined the project. Project status is now Declined." });
                }
                else
                {
                    return Results.BadRequest(new { message = "Invalid status value" });
                }

            })
            .WithOpenApi();
        }
    }

    public record UpdateProjectDTO
    {
        public string? UserId { get; set; }
        public string? UserRole { get; set; }
        public string? InviteStatus { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Decimal? Budget { get; set; }
        public Decimal? AdminPay { get; set; }
        public string? ReasonForDecline { get; set; }
    }

    public record ProjectStatusDTO(
        string Status,
        string? ReasonForDecline,
        string UserId
    );
}
