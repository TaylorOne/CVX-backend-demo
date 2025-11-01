using CVX.Models;
using CVX.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

namespace CVX.Endpoints
{
    public static class MilestoneEndpoints
    {
        public static void MapMilestoneEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("milestones", async (ApplicationDbContext dbContext, [FromBody] MilestoneDTO milestoneDTO, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var project = await dbContext.Projects
                    .Include(p => p.ProjectMembers)
                    .FirstOrDefaultAsync(p => p.Id == milestoneDTO.ProjectId);

                if (project == null)
                {
                    return Results.NotFound("Project not found.");
                }

                // Check if a milestone with the same name already exists for the project
                var existingMilestone = await dbContext.Milestones
                    .FirstOrDefaultAsync(m => m.ProjectId == milestoneDTO.ProjectId && m.Name == milestoneDTO.Name);

                if (existingMilestone != null)
                {
                    return Results.BadRequest("A milestone with the same name already exists for this project.");
                }

                Log.Information("milestone DTO: {@MilestoneDTO}", milestoneDTO);

                DateTime? dueDate = null;
                if (!string.IsNullOrWhiteSpace(milestoneDTO.DueDate))
                {
                    if (DateTime.TryParse(
                            milestoneDTO.DueDate,
                            null,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                            out var parsedDate))
                    {
                        dueDate = parsedDate;
                    }
                    else
                    {
                        // Handle parse failure (optional)
                    }
                }

                DateTime? startDate = null;
                if (!string.IsNullOrWhiteSpace(milestoneDTO.StartDate))
                {
                    if (DateTime.TryParse(
                            milestoneDTO.StartDate,
                            null,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                            out var parsedStartDate))
                    {
                        startDate = parsedStartDate;
                    }
                    else
                    {
                        // Handle parse failure (optional)
                    }
                }

                var user = httpContext.User;
                var userIsAssignee = false;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is the same as the assignee, in which case make the assignee status "Accepted"
                    if (userId == milestoneDTO.AssigneeId)
                    {
                        userIsAssignee = true;
                    }

                }

                var assignee = await dbContext.ProjectMembers
                    .Include(pm => pm.User)
                    .FirstOrDefaultAsync(pm => pm.UserId == milestoneDTO.AssigneeId && pm.ProjectId == milestoneDTO.ProjectId);

                var milestone = new Milestone
                {
                    Name = milestoneDTO.Name,
                    Description = milestoneDTO.Description,
                    DefinitionOfDone = milestoneDTO.DefinitionOfDone ?? string.Empty,
                    Deliverables = milestoneDTO.Deliverables ?? string.Empty,
                    AllocatedLaunchTokens = milestoneDTO.LaunchTokenAmount,
                    AssigneeStatus = userIsAssignee ? AssigneeStatus.Accepted : AssigneeStatus.Assigned,
                    CreatedAt = DateTime.UtcNow,
                    StartDate = startDate,
                    DueDate = dueDate,
                    ProjectId = milestoneDTO.ProjectId,
                    AssigneeId = assignee.Id
                };

                dbContext.Milestones.Add(milestone);

                // Deduct the milestones from the project's launch token balance
                project.LaunchTokenBalance -= milestone.AllocatedLaunchTokens;
                dbContext.Projects.Update(project);
                await dbContext.SaveChangesAsync();

                return Results.Ok(new
                {
                    milestone.Id,
                    milestone.Name,
                    milestone.Description,
                    milestone.AllocatedLaunchTokens,
                    milestone.CreatedAt,
                    AssigneeStatus = Enum.GetName(typeof(AssigneeStatus), milestone.AssigneeStatus),
                    ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), ApprovalStatus.Draft),
                    AssigneeName = assignee.User.FirstName + " " + assignee.User.LastName,
                });

            })
            .WithOpenApi();

            app.MapGet("milestones/{id}", async (ApplicationDbContext dbContext, int id) =>
            {
                var milestone = await dbContext.Milestones
                    .Include(p => p.Project)
                        .ThenInclude(p => p.ProjectMembers)
                            .ThenInclude(u => u.User)
                    .Include(p => p.Project)
                        .ThenInclude(c => c.Collaborative)
                    .Include(p => p.Assignee)
                        .ThenInclude(u => u.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (milestone == null)
                {
                    return Results.NotFound("Milestone not found.");
                }

                var milestoneData = new
                {
                    milestone.Id,
                    milestone.Name,
                    milestone.Description,
                    milestone.DefinitionOfDone,
                    milestone.Deliverables,
                    milestone.AllocatedLaunchTokens,
                    milestone.IsComplete,
                    milestone.CompletionSummary,
                    milestone.Feedback,
                    milestone.ReasonForDecline,
                    milestone.ArtifactUrl,
                    ProjectAdmins = milestone.Project.ProjectMembers
                        .Where(pm => pm.Role == ProjectRole.ProjectAdmin)
                        .Select(pm => new
                        {
                            AdminId = pm.UserId,
                            AdminName = pm.User != null
                                ? pm.User.FirstName + " " + pm.User.LastName
                                : string.Empty
                        })
                        .ToList(),
                    ProjectName = milestone.Project.Name,
                    StartDate = milestone.StartDate?.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    DueDate = milestone.DueDate?.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    CreatedAt = milestone.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    AssigneeStatus = Enum.GetName(typeof(AssigneeStatus), milestone.AssigneeStatus),
                    ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), milestone.ApprovalStatus),
                    AssigneeName = milestone.Assignee?.User.FirstName + " " + milestone.Assignee?.User.LastName,
                    AssigneeId = milestone.Assignee?.User.Id,
                    CashEquivalent = milestone.AllocatedLaunchTokens * milestone.Project.Collaborative.LaunchTokenValue,
                    CollabIsActive = milestone.Project.Collaborative.ApprovalStatus == ApprovalStatus.Active
                };

                return Results.Ok(milestoneData);

            })
            .WithOpenApi();

            app.MapPatch("milestones/{id}", async (ApplicationDbContext dbContext, int id, [FromBody] UpdateMilestoneDTO updateMilestoneDTO, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var milestone = await dbContext.Milestones
                    .Include(p => p.Project)
                        .ThenInclude(p => p.ProjectMembers)
                            .ThenInclude(u => u.User)
                    .Include(p => p.Project)
                        .ThenInclude(c => c.Collaborative)
                    .Include(p => p.Assignee)
                        .ThenInclude(u => u.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (milestone == null)
                {
                    return Results.NotFound("Milestone not found.");
                }

                // Update milestone details
                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.Name))
                {
                    milestone.Name = updateMilestoneDTO.Name;
                }

                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.Description))
                {
                    milestone.Description = updateMilestoneDTO.Description;
                }

                if (updateMilestoneDTO.DefinitionOfDone != null)
                {
                    milestone.DefinitionOfDone = updateMilestoneDTO.DefinitionOfDone;
                }

                if (updateMilestoneDTO.Deliverables != null)
                {
                    milestone.Deliverables = updateMilestoneDTO.Deliverables;
                }

                if (updateMilestoneDTO.AllocatedLaunchTokens.HasValue)
                {
                    var project = await dbContext.Projects
                        .FirstOrDefaultAsync(p => p.Id == milestone.ProjectId);

                    if (project == null)
                    {
                        return Results.NotFound("Project not found.");
                    }

                    // Calculate the difference between the new and old allocated launch tokens
                    var tokenDifference = updateMilestoneDTO.AllocatedLaunchTokens.Value - milestone.AllocatedLaunchTokens;

                    // Check if the project has enough balance for the increase
                    if (tokenDifference > 0 && project.LaunchTokenBalance < tokenDifference)
                    {
                        return Results.BadRequest("Insufficient launch token balance in the project.");
                    }

                    // Update the project's launch token balance
                    project.LaunchTokenBalance -= tokenDifference;
                    dbContext.Projects.Update(project);

                    // Update the milestone's allocated launch tokens
                    milestone.AllocatedLaunchTokens = updateMilestoneDTO.AllocatedLaunchTokens.Value;
                }

                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.StartDate))
                {
                    if (DateTime.TryParse(
                            updateMilestoneDTO.StartDate,
                            null,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                            out var parsedStartDate))
                    {
                        milestone.StartDate = parsedStartDate;
                    }
                    else
                    {
                        return Results.BadRequest("Invalid start date format.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.DueDate))
                {
                    if (DateTime.TryParse(
                            updateMilestoneDTO.DueDate,
                            null,
                            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                            out var parsedDueDate))
                    {
                        milestone.DueDate = parsedDueDate;
                    }
                    else
                    {
                        return Results.BadRequest("Invalid due date format.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.AssigneeId))
                {

                }

                // Update milestone assignment acceptance status
                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.AcceptanceStatus))
                {
                    if (updateMilestoneDTO.AcceptanceStatus == "Accepted")
                    {
                        milestone.AssigneeStatus = AssigneeStatus.Accepted;
                    }
                    else if (updateMilestoneDTO.AcceptanceStatus == "Declined")
                    {
                        milestone.AssigneeStatus = AssigneeStatus.Declined;
                        milestone.ReasonForDecline = updateMilestoneDTO.ReasonForDecline ?? string.Empty;

                    } else
                    {
                        return Results.BadRequest("Invalid status value.");
                    }
                }

                // Update milestone assignee status if previously declined and now being resubmitted to user
                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.AssigneeStatus))
                {
                    if (milestone.AssigneeStatus == AssigneeStatus.Declined && updateMilestoneDTO.AssigneeStatus == "Assigned")
                    {
                        milestone.AssigneeStatus = AssigneeStatus.Assigned;
                        milestone.ReasonForDecline = string.Empty;
                    }
                }

                Log.Information("updateMilestoneDT0: {@UpdateMilestoneDTO}", updateMilestoneDTO);

                var user = httpContext.User;
                var userIsProjectAdmin = false;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);
                    var projectAdminIds = milestone.Project.ProjectMembers
                        .Where(pm => pm.Role == ProjectRole.ProjectAdmin)
                        .Select(pm => pm.UserId)
                        .ToList();

                    // check if user is a project admin, in which case approve the milestone automatically upon completion below
                    if (projectAdminIds.Contains(userId))
                    {
                        userIsProjectAdmin = true;
                    }

                }

                // Update milestone completion details
                if (updateMilestoneDTO.IsComplete != milestone.IsComplete || updateMilestoneDTO.CompletionSummary != milestone.CompletionSummary
                    || updateMilestoneDTO.ArtifactUrl != milestone.ArtifactUrl)
                {
                    milestone.IsComplete = updateMilestoneDTO.IsComplete;

                    if (milestone.IsComplete == true)
                    {
                        milestone.ApprovalStatus = ApprovalStatus.Submitted;
                    }

                    if (milestone.IsComplete == false)
                    {
                        milestone.ApprovalStatus = ApprovalStatus.Draft;
                    }

                    if (updateMilestoneDTO.ArtifactUrl != milestone.ArtifactUrl)
                    {
                        milestone.ArtifactUrl = updateMilestoneDTO.ArtifactUrl ?? string.Empty;
                    }

                    milestone.CompletionSummary = updateMilestoneDTO.CompletionSummary ?? string.Empty;
                }

                // Update milestone approval status
                if (!string.IsNullOrWhiteSpace(updateMilestoneDTO.ApprovalStatus))
                {
                    if (updateMilestoneDTO.ApprovalStatus == "approve")
                    {
                        milestone.ApprovalStatus = ApprovalStatus.Archived;
                        milestone.CompletedAt = DateTime.UtcNow;

                        var project = await dbContext.Projects
                            .Include(project => project.Collaborative)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == milestone.ProjectId);

                        // pay user for their work on the milestone
                        var milestonePayment = new LaunchTokenTransaction
                        {
                            Amount = milestone.AllocatedLaunchTokens,
                            PaymentType = PaymentType.Milestone,
                            UserId = milestone.Assignee.UserId,
                            MilestoneId = milestone.Id,
                            ProjectId = milestone.ProjectId,
                            CollaborativeId = project.CollaborativeId,
                            CreatedAt = DateTime.UtcNow
                        };

                        // pay network transaction fee
                        var networkPayment = new LaunchTokenTransaction
                        {
                            Amount = milestone.AllocatedLaunchTokens * NetworkConstants.TransactionFee,
                            PaymentType = PaymentType.Network,
                            MilestoneId = milestone.Id,
                            ProjectId = milestone.ProjectId,
                            CollaborativeId = project.CollaborativeId,
                            CreatedAt = DateTime.UtcNow
                        };

                        dbContext.Add(milestonePayment);
                        dbContext.Add(networkPayment);
                    }

                    if (updateMilestoneDTO.ApprovalStatus == "decline")
                    {
                        milestone.IsComplete = false;
                        milestone.ApprovalStatus = ApprovalStatus.Declined;
                        milestone.Feedback = updateMilestoneDTO.Feedback ?? string.Empty;
                    }
                }

                dbContext.Milestones.Update(milestone);
                await dbContext.SaveChangesAsync();

                var milestoneData = new
                {
                    milestone.Id,
                    milestone.Name,
                    milestone.Description,
                    milestone.DefinitionOfDone,
                    milestone.Deliverables,
                    milestone.AllocatedLaunchTokens,
                    milestone.IsComplete,
                    milestone.CompletionSummary,
                    milestone.Feedback,
                    milestone.ArtifactUrl,
                    ProjectAdmins = milestone.Project.ProjectMembers
                        .Where(pm => pm.Role == ProjectRole.ProjectAdmin)
                        .Select(pm => new
                        {
                            AdminId = pm.UserId,
                            AdminName = pm.User != null
                                ? pm.User.FirstName + " " + pm.User.LastName
                                : string.Empty
                        })
                        .ToList(),
                    ProjectName = milestone.Project.Name,
                    DueDate = milestone.DueDate?.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    CreatedAt = milestone.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    AssigneeStatus = Enum.GetName(typeof(AssigneeStatus), milestone.AssigneeStatus),
                    ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), milestone.ApprovalStatus),
                    AssigneeName = milestone.Assignee?.User.FirstName + " " + milestone.Assignee?.User.LastName,
                    AssigneeId = milestone.Assignee?.User.Id,
                    CashEquivalent = milestone.AllocatedLaunchTokens * milestone.Project.Collaborative.LaunchTokenValue,
                };

                return Results.Ok(milestoneData);

            })
            .WithOpenApi();
        }
    }

    public record UpdateMilestoneDTO
    {
        public string? AcceptanceStatus { get; set; }
        public bool IsComplete { get; set; } = false;
        public string? CompletionSummary { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? AssigneeStatus { get; set; }
        public string? ArtifactUrl { get; set; }
        public string? Feedback { get; set; }
        public string? AssigneeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DefinitionOfDone { get; set; }
        public string? Deliverables { get; set; }
        public decimal? AllocatedLaunchTokens { get; set; }
        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public string? ReasonForDecline { get; set; }
    }
}