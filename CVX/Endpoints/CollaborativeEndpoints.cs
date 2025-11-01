using CVX.Models;
using CVX.Utilities;
using CVX.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Serilog;

namespace CVX.Endpoints
{
    public static class CollaborativeEndpoints
    {
        public static void MapCollaborativeEndpoints(this IEndpointRouteBuilder app)
        {

            app.MapGet("/collaboratives", async (ApplicationDbContext dbContext) =>
            {
                var collaboratives = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                        .ThenInclude(c => c.User)
                    .AsNoTracking()
                    .ToListAsync();

                var result = collaboratives.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.LogoUrl,
                    Status = EnumUtility.GetEnumDisplayName(c.ApprovalStatus)
                });

                return Results.Ok(result);
            })
            .WithOpenApi();

            app.MapPost("/collaboratives", async (ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager, [FromBody] CollaborativeDTO collab) =>
            {

                if (string.IsNullOrWhiteSpace(collab.Name) || string.IsNullOrWhiteSpace(collab.Description))
                {
                    return Results.BadRequest(new { message = "Name and Description are required." });
                }

                var collaboratives = await dbContext.Collaboratives
                    .AsNoTracking()
                    .ToListAsync();

                // Check if a collaborative with the same name already exists
                if (collaboratives.Any(c => c.Name == collab.Name))
                {
                    return Results.BadRequest(new { message = "A collaborative with this name already exists." });
                }

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // Map CollaborativeDTO to Collaborative
                    var collaborative = new Collaborative
                    {
                        Name = collab.Name,
                        Description = collab.Description,
                        WebsiteUrl = collab.WebsiteUrl,
                        LogoUrl = collab.LogoUrl,
                        City = collab.City,
                        State = collab.State,
                        CollabAdminCompensationPercent = collab.CollabAdminCompensationPercent / 100,
                        LaunchTokensBalance = collab.LaunchTokensCreated * (collab.LaunchTokenReleaseRate / 100),
                        LaunchTokensCreated = collab.LaunchTokensCreated,
                        LaunchTokensPriorWorkPercent = collab.LaunchTokensPriorWorkPercent / 100,
                        LastTokenRelease = DateTime.UtcNow,
                        LaunchCyclePeriod = collab.LaunchCyclePeriod,
                        TokenReleaseRate = collab.LaunchTokenReleaseRate / 100,
                        LaunchTokenValue = collab.LaunchTokenValue,
                        WeeksTillSecondTokenRelease = collab.LaunchTokenSecondReleaseWeeks,
                        CreatorId = userId,
                    };

                    // Add skills to collaborative
                    foreach (var skillId in collab.Skills)
                    {
                        var dbSkill = dbContext.Skills.Find(skillId);
                        if (dbSkill != null)
                        {
                            collaborative.Skills.Add(dbSkill);
                        }
                    }

                    // Add experience sectors to collaborative
                    foreach (var sectorId in collab.Experience)
                    {
                        var dbSector = dbContext.Sectors.Find(sectorId);
                        if (dbSector != null)
                        {
                            collaborative.Sectors.Add(dbSector);
                        }
                    }

                    // Save the collaborative to the db
                    dbContext.Collaboratives.Add(collaborative);
                    await dbContext.SaveChangesAsync();

                    // now retrieve collaborative.Id from db
                    // and create first member role: make the creator the collaborative admin
                    var memberRole = new CollaborativeMember
                    {
                        UserId = collaborative.CreatorId,
                        Role = CollaborativeRole.CollaborativeAdmin,
                        CollaborativeId = collaborative.Id,
                        InviteStatus = InviteStatus.Accepted
                    };


                    // Add the CSA if provided
                    if (!string.IsNullOrWhiteSpace(collab.CsaDocUrl))
                    {
                        await AddCsaToCollaborativeAsync(dbContext, collaborative, collab.CsaDocUrl);

                        if (collaborative.CurrentCSAId.HasValue)
                        {
                            // make the collab admin accept the CSA and become active automatically
                            memberRole.CSAAcceptedStatus = CSAAcceptedStatus.Accepted;
                            memberRole.CSAAcceptedAt = DateTime.UtcNow;
                            memberRole.CSAAcceptedId = collaborative.CurrentCSAId.Value;
                            memberRole.IsActive = true;

                            // and set readyForSumittal to true since all members have currently accepted
                            collaborative.ReadyForSubmittal = true;
                        }

                    }

                    dbContext.CollaborativeMembers.Add(memberRole);
                    await dbContext.SaveChangesAsync();

                    return Results.Ok(new { message = "Collaborative created successfully!" });
                }

                return Results.Unauthorized();
            })
            .WithOpenApi();

            // Collaborative home page
            app.MapGet("collaboratives/{id}", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var collab = await dbContext.Collaboratives
                    .Include(c => c.Skills)
                    .Include(c => c.Sectors)
                    .Include(c => c.CollaborativeMembers)
                        .ThenInclude(c => c.User)
                    .Include(c => c.CurrentCSA)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collab == null)
                {
                    return Results.NotFound(new { message = "collab not found" });
                }

                bool userIsCollabAdmin = false;
                bool userIsCollabContributor = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a collaborative admin
                    // so front-end knows whether to display the "Edit Collaborative Info" button or not
                    userIsCollabAdmin = collab.CollaborativeMembers
                        .Any(m => m.UserId == userId && m.CollaborativeId == collab.Id && m.Role == CollaborativeRole.CollaborativeAdmin);

                    // check if user is a collaborative member
                    // so front-end knows whether to display the "View Collaborative Sharing Agreement" button or not
                    userIsCollabContributor = collab.CollaborativeMembers
                        .Any(m => m.UserId == userId && m.CollaborativeId == collab.Id &&
                            (m.Role == CollaborativeRole.CollaborativeAdmin || m.Role == CollaborativeRole.CollaborativeMember));
                }

                // collect reasons any collab members may have declined the collab
                var reasonsForInviteDecline = collab.CollaborativeMembers?
                    .Where(m => !string.IsNullOrEmpty(m.ReasonForCollabDecline))
                    .Select((m, idx) => new
                    {
                        Id = idx + 1,
                        MemberId = m.UserId,
                        MemberName = m.User.FirstName + " " + m.User.LastName,
                        Reason = m.ReasonForCollabDecline
                    })
                    .ToList();

                var result = new
                {
                    collab.Id,
                    collab.Name,
                    collab.Description,
                    collab.WebsiteUrl,
                    collab.LogoUrl,
                    collab.City,
                    collab.State,
                    collabAdminCompensationPercent = collab.CollabAdminCompensationPercent * 100,
                    collab.ReadyForSubmittal,
                    collab.ReasonForDecline,
                    approvalStatus = EnumUtility.GetEnumDisplayName(collab.ApprovalStatus),
                    csaDocUrl = collab.CurrentCSA?.Url,
                    CreatedAt = collab.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    userIsCollabAdmin,
                    userIsCollabContributor,
                    reasonsForInviteDecline,

                    AdminEmail = collab.CollaborativeMembers?
                        .FirstOrDefault(m => m.Role == CollaborativeRole.CollaborativeAdmin)?
                        .User.UserName,

                    AdminName = collab.CollaborativeMembers?
                        .FirstOrDefault(m => m.Role == CollaborativeRole.CollaborativeAdmin)?
                        .User.FirstName + " " +
                        collab.CollaborativeMembers?
                        .FirstOrDefault(m => m.Role == CollaborativeRole.CollaborativeAdmin)?
                        .User.LastName,

                    Skills = collab.Skills?
                        .Select(s => new
                        {
                            id = s.Id,
                            value = s.Skill
                        })
                        .ToList(),

                    Experience = collab.Sectors?
                        .Select(e => new
                        {
                            id = e.Id,
                            value = e.Sector
                        })
                        .ToList(),
                };
                return Results.Ok(result);
            })
            .WithOpenApi();

            // Collaborative members page
            app.MapGet("collaboratives/{id}/members", async (int id, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var collab = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                        .ThenInclude(c => c.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collab == null)
                {
                    return Results.NotFound(new { message = "collab not found" });
                }

                bool userIsCollabAdmin = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a Collaborative Admin
                    // this info is necessary so front-end knows whether to display the "Edit" button or not
                    userIsCollabAdmin = collab.CollaborativeMembers
                        .Any(m => m.UserId == userId && m.Role == CollaborativeRole.CollaborativeAdmin);
                }

                var result = new
                {
                    collab.Id,
                    collab.Name,
                    collab.LogoUrl,
                    userIsCollabAdmin,
                    Members = collab.CollaborativeMembers?
                        .Select(m => new
                        {
                            m.User.Id,
                            m.User.FirstName,
                            m.User.LastName,
                            m.User.UserName,
                            m.User.AvatarUrl,
                            m.IsActive,
                            Role = EnumUtility.GetEnumDisplayName(m.Role),
                            InviteStatus = Enum.GetName(typeof(InviteStatus), m.InviteStatus) // Convert enum value to string
                        })
                        .ToList(),
                };
                return Results.Ok(result);
            })
            .WithOpenApi();


            // Collaborative projects page
            app.MapGet("collaboratives/{id}/projects", async (int id, ApplicationDbContext dbContext) =>
            {
                var projects = await dbContext.Projects
                    .Where(p => p.CollaborativeId == id)
                    .Include(pm => pm.ProjectMembers)
                        .ThenInclude(m => m.User)
                    .AsNoTracking()
                    .ToListAsync();

                if (projects == null || !projects.Any())
                {
                    return Results.NotFound(new { message = "No projects found for this collaborative." });
                }

                var result = projects.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ProjectAdminCompensationLaunchTokens,
                    Budget = p.LaunchTokenBudget,
                    CollabId = p.CollaborativeId,
                    ApprovalStatus = EnumUtility.GetEnumDisplayName(p.ApprovalStatus),
                    AdminName  = p.ProjectMembers
                        .Where(pm => pm.Role == ProjectRole.ProjectAdmin)
                        .Select(pm => pm.User != null
                            ? (pm.User.FirstName ?? "") + " " + (pm.User.LastName ?? "")
                            : null)
                        .FirstOrDefault(),
                    CreatedAt = p.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                });

                return Results.Ok(result);
            })
            .WithOpenApi();

            // Collaborative treasury page
            app.MapGet("collaboratives/{id}/treasury", async (int id,
                ApplicationDbContext dbContext,
                HttpContext httpContext,
                UserManager<ApplicationUser> userManager,
                [FromServices] ITokenReleaseService tokenReleaseService) =>
            {
                var collab = await dbContext.Collaboratives
                    .Include(c => c.Projects)
                        .ThenInclude(m => m.Milestones)
                    .Include(c => c.CollaborativeMembers)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collab == null)
                {
                    return Results.NotFound(new { message = "collab not found" });
                }

                bool userIsCollabAdmin = false;

                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a Collaborative Admin
                    // this info is necessary so front-end knows whether to display the "Edit" button or not
                    userIsCollabAdmin = collab.CollaborativeMembers
                        .Any(m => m.UserId == userId && m.Role == CollaborativeRole.CollaborativeAdmin);
                }

                // Launch tokens
                DateTime? nextTokenReleaseDate = null;
                decimal currentTokenRelease = 0.0m;
                decimal nextTokenRelease = 0.0m;
                decimal tokensReceivable = 0.0m;
                decimal tokenBalance = 0.0m;
                decimal tokenReleaseRate = collab.TokenReleaseRate ?? 0.10m;
                decimal tokensCreated = collab.LaunchTokensCreated ?? 0m;
                decimal tokensPriorWorkPercent = collab.LaunchTokensPriorWorkPercent ?? 0m;

                double currentCycleNum = GetCurrentCycleNum(collab);

                var baseTokenReleaseAmount = tokensCreated * (1 - (tokensPriorWorkPercent)) * tokenReleaseRate;
                var geometricSeriesFormula = (decimal)Math.Pow((double)(1 - tokenReleaseRate), currentCycleNum);

                currentTokenRelease = baseTokenReleaseAmount * (decimal)Math.Pow((double)(1 - tokenReleaseRate), currentCycleNum - 1);

                nextTokenRelease = baseTokenReleaseAmount * geometricSeriesFormula;

                tokensReceivable = nextTokenRelease / tokenReleaseRate;

                nextTokenReleaseDate = GetNextTokenReleaseDate(collab, currentCycleNum);

                // Sum up launch tokens from all milestones that remain unpaid (i.e., not archived)
                decimal projectWorkPayment = collab.Projects
                    .SelectMany(p => p.Milestones)
                    .Where(m => m.ApprovalStatus != ApprovalStatus.Archived)
                    .Sum(m => m.AllocatedLaunchTokens);

                decimal projectAdminPay = collab.Projects
                    .Sum(m => m.ProjectAdminCompensationLaunchTokens);

                var networkTransactionMultiplier = 1 + NetworkConstants.TransactionFee;

                var cumulativeReleasedTokens = CalculateCumulativeReleasedTokens(collab, currentCycleNum);
                tokenBalance = CalculateCollabTokenBalance(dbContext,collab);

                var result = new
                {
                    collab.Id,
                    collab.Name,
                    collab.LogoUrl,
                    nextTokenReleaseDate = nextTokenReleaseDate?.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    currentTokenRelease = Math.Round(currentTokenRelease, 2),
                    nextTokenRelease = Math.Round(nextTokenRelease, 2),
                    tokensReceivable = Math.Round(tokensReceivable, 2),
                    tokenBalance = Math.Round(tokenBalance, 2),
                    tokenValue = Math.Round(collab.LaunchTokenValue, 2),

                    collabAdminCompensationPercent = collab.CollabAdminCompensationPercent * 100,
                    networkFeePercent = NetworkConstants.TransactionFee * 100,
                    tokensCollabAdmin = Math.Round((currentTokenRelease * collab.CollabAdminCompensationPercent) * networkTransactionMultiplier, 2),
                    tokensNetworkFee = Math.Round(cumulativeReleasedTokens * NetworkConstants.TransactionFee,2),
                    tokensPriorWork = Math.Round(tokensCreated * tokensPriorWorkPercent, 2),
                    projectWorkPayment = (projectWorkPayment + projectAdminPay) * networkTransactionMultiplier,
                    userIsCollabAdmin,

                    // for the edit form
                    tokensCreated,
                    tokenReleaseRate = collab.TokenReleaseRate * 100,
                    launchCyclePeriodWeeks = collab.LaunchCyclePeriod,
                    tokenSecondReleaseWeeks = collab.WeeksTillSecondTokenRelease,
                    tokensPriorWorkPercent = collab.LaunchTokensPriorWorkPercent * 100,

                };
                return Results.Ok(result);

            })
            .WithOpenApi();

            app.MapPatch("collaboratives/{id}/treasury", async (int id, [FromBody] UpdateTreasuryDTO updateDTO, ApplicationDbContext dbContext) =>
            {
                var collaborative = await dbContext.Collaboratives
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found" });
                }

                string message = "Collaborative treasury settings ";

                // Update fields if provided
                if (updateDTO.TokensCreated.HasValue)
                {
                    collaborative.LaunchTokensCreated = updateDTO.TokensCreated.Value;
                    message += "LaunchTokensCreated, ";
                }

                if (updateDTO.TokenReleaseRate.HasValue)
                {
                    collaborative.TokenReleaseRate = updateDTO.TokenReleaseRate.Value / 100;
                    message += "LaunchTokenReleaseRate, ";
                }

                if (updateDTO.LaunchCyclePeriodWeeks.HasValue)
                {
                    collaborative.LaunchCyclePeriod = updateDTO.LaunchCyclePeriodWeeks.Value;
                    message += "LaunchCyclePeriod, ";
                }

                if (updateDTO.TokenSecondReleaseWeeks.HasValue)
                {
                    collaborative.WeeksTillSecondTokenRelease = updateDTO.TokenSecondReleaseWeeks.Value;

                    // Also update SecondTokenReleaseDate based on new weeks value
                    collaborative.SecondTokenReleaseDate = DateTime.UtcNow.AddDays((double)collaborative.WeeksTillSecondTokenRelease * 7);
                    message += "LaunchTokenSecondReleaseWeeks, ";

                }

                if (updateDTO.TokensPriorWorkPercent.HasValue)
                {
                    collaborative.LaunchTokensPriorWorkPercent = updateDTO.TokensPriorWorkPercent.Value / 100;
                    message += "LaunchTokensPriorWorkPercent, ";
                }

                if (updateDTO.CollabAdminCompensationPercent.HasValue)
                {
                    collaborative.CollabAdminCompensationPercent = updateDTO.CollabAdminCompensationPercent.Value / 100;
                    message += "CollabAdminCompensationPercent, ";
                }

                if (updateDTO.TokenValue.HasValue)
                {
                    collaborative.LaunchTokenValue = updateDTO.TokenValue.Value;
                    message += "LaunchTokenValue, ";
                } 

                message += "updated successfully!";
                await dbContext.SaveChangesAsync();
                return Results.Ok(new { message });
            })
            .WithOpenApi();

            app.MapGet("collaboratives/{id}/wallet", async (int id, ApplicationDbContext dbContext,
                HttpContext httpContext,
                UserManager<ApplicationUser> userManager) =>
            {
                var collab = await dbContext.Collaboratives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collab == null)
                {
                    return Results.NotFound(new { message = "collab not found" });
                }

                string userId = null;
                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    userId = userManager.GetUserId(user);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.NotFound(new { message = "User not found or not authenticated" });
                }

                // Check if user is a member of the collaborative because if not they won't have a wallet
                var isMember = await dbContext.CollaborativeMembers
                    .AnyAsync(m => m.CollaborativeId == id && m.UserId == userId);

                if (!isMember)
                {
                    var CollabInfoNoWalletData = new
                    {
                        collab.Id,
                        collab.Name,
                        collab.LogoUrl,
                        userIsCollabMember = false,

                    };
                    return Results.Ok(CollabInfoNoWalletData);
                }

                // Add up all launch tokens due the user as project admin for all projects in collaborative
                var projectsUser = await dbContext.Projects
                    .Where(p => p.CollaborativeId == id && p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Role == ProjectRole.ProjectAdmin))
                    .ToListAsync();

                decimal userProjectAdminCompensation = 0.0m;

                foreach (var project in projectsUser)
                {
                    userProjectAdminCompensation += project.ProjectAdminCompensationLaunchTokens;
                }

                // Add up all launch tokens due for project admins across all collaborative projects
                var projectsAll = await dbContext.Projects
                    .Where(p => p.CollaborativeId == id)
                    .ToListAsync();

                decimal allProjectAdminCompensation = 0.0m;

                foreach (var project in projectsAll)
                {
                    allProjectAdminCompensation += project.ProjectAdminCompensationLaunchTokens;
                }

                var launchTokenTransactions = await dbContext.LaunchTokenTransactions
                    .Where(t => t.CollaborativeId == id && t.UserId == userId && t.PaymentType != PaymentType.Network)
                    .Include(t => t.Project)
                    .Include(t => t.Milestone)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new
                    {
                        t.Id,
                        t.Amount,
                        Type = EnumUtility.GetEnumDisplayName(t.PaymentType),
                        Date = t.CreatedAt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                        Project = t.Project != null ? t.Project.Name : null,
                        Milestone = t.Milestone != null ? t.Milestone.Name : null,
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var allCollabMilestones = await dbContext.Milestones
                    .Where(m => m.Project.CollaborativeId == collab.Id)
                    .AsNoTracking()
                    .ToListAsync();

                var userMilestones = await dbContext.Milestones
                    .Where(m => m.Project.CollaborativeId == collab.Id && m.Assignee != null && m.Assignee.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync();

                var result = new
                {
                    collab.Id,
                    collab.Name,
                    collab.LogoUrl,
                    collab.LaunchTokensCreated,
                    TokensPriorWork = Math.Round(collab.LaunchTokensCreated.Value * collab.LaunchTokensPriorWorkPercent.Value, 2),
                    UserAssignedLaunchTokens = userMilestones.Sum(x => x.AllocatedLaunchTokens) + userProjectAdminCompensation,
                    AllAssignedLaunchTokens = allCollabMilestones.Sum(x => x.AllocatedLaunchTokens) + allProjectAdminCompensation,
                    launchTokenTransactions,
                    userIsCollabMember = true,
                };
                return Results.Ok(result);

            })
            .WithOpenApi();

            app.MapPatch("collaboratives/{id}", async (int id, [FromBody] UpdateCollaborativeDTO updateDTO, ApplicationDbContext dbContext) =>
            {
                var collaborative = await dbContext.Collaboratives
                    .Include(c => c.Skills)
                    .Include(c => c.Sectors)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found" });
                }

                string message = "Collaborative ";

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(updateDTO.Name))
                {
                    collaborative.Name = updateDTO.Name;
                    message += "Name, ";
                }
                if (!string.IsNullOrWhiteSpace(updateDTO.Description))
                {
                    collaborative.Description = updateDTO.Description;
                    message += "Description, ";
                }

                if (!string.IsNullOrWhiteSpace(updateDTO.WebsiteUrl))
                {
                    collaborative.WebsiteUrl = updateDTO.WebsiteUrl;
                    message += "WebsiteUrl, ";
                }

                if (!string.IsNullOrWhiteSpace(updateDTO.LogoUrl))
                {
                    collaborative.LogoUrl = updateDTO.LogoUrl;
                    message += "LogoUrl, ";
                }

                if (!string.IsNullOrWhiteSpace(updateDTO.City))
                {
                    collaborative.City = updateDTO.City;
                    message += "City, ";
                }

                if (!string.IsNullOrWhiteSpace(updateDTO.State))
                {
                    collaborative.State = updateDTO.State;
                    message += "State, ";
                }

                // Update skills if provided
                if (updateDTO.Skills != null)
                {
                    var incomingSkillIds = updateDTO.Skills;
                    var existingSkillIds = collaborative.Skills.Select(s => s.Id).ToList();

                    // Remove skills not in the incoming list
                    var skillsToRemove = collaborative.Skills.Where(s => !incomingSkillIds.Contains(s.Id)).ToList();
                    foreach (var skill in skillsToRemove)
                    {
                        collaborative.Skills.Remove(skill);
                    }

                    // Add new skills
                    var skillsToAdd = incomingSkillIds.Except(existingSkillIds).ToList();
                    foreach (var skillId in skillsToAdd)
                    {
                        var dbSkill = await dbContext.Skills.FindAsync(skillId);
                        if (dbSkill != null)
                        {
                            collaborative.Skills.Add(dbSkill);
                        }
                    }

                    message += "Skills, ";
                }

                // Update experience/sectors if provided
                if (updateDTO.Experience != null)
                {
                    var incomingSectorIds = updateDTO.Experience;
                    var existingSectorIds = collaborative.Sectors.Select(s => s.Id).ToList();

                    // Remove sectors not in the incoming list
                    var sectorsToRemove = collaborative.Sectors.Where(s => !incomingSectorIds.Contains(s.Id)).ToList();
                    foreach (var sector in sectorsToRemove)
                    {
                        collaborative.Sectors.Remove(sector);
                    }

                    // Add new sectors
                    var sectorsToAdd = incomingSectorIds.Except(existingSectorIds).ToList();
                    foreach (var sectorId in sectorsToAdd)
                    {
                        var dbSector = await dbContext.Sectors.FindAsync(sectorId);
                        if (dbSector != null)
                        {
                            collaborative.Sectors.Add(dbSector);
                        }
                    }

                    message += "Experience, ";
                }

                message += "updated successfully!";

                await dbContext.SaveChangesAsync();
                return Results.Ok(new { message });

            })
            .WithOpenApi();


            // Add Collab member
            app.MapPost("collaboratives/{id}/members", async (int id, [FromBody] NewMemberDTO newMember, ApplicationDbContext dbContext, HttpContext httpContext, UserManager<ApplicationUser> userManager ) =>
            {

                if (string.IsNullOrEmpty(newMember.UserId) || string.IsNullOrEmpty(newMember.UserRole))
                {
                    return Results.BadRequest(new { message = "UserId and UserRole are required." });
                }

                var collaborative = await dbContext.Collaboratives
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found." });
                }

                var member = await dbContext.CollaborativeMembers
                    .Where(m => m.CollaborativeId == id)
                    .FirstOrDefaultAsync(m => m.UserId == newMember.UserId);

                if (member != null)
                {
                    return Results.BadRequest(new { message = "Member already exists in this collaborative." });
                }

                var newCollabMember = new CollaborativeMember
                {
                    UserId = newMember.UserId,
                    Role = EnumUtility.GetEnumValueFromDisplayName<CollaborativeRole>(newMember.UserRole) ?? CollaborativeRole.CollaborativeMember,
                    CollaborativeId = collaborative.Id,
                    InviteStatus = InviteStatus.Invited
                };

                // if the collaborative is already active it needs to revert back to draft since
                // the new member needs to 1) accept the invite to the collab and 2) agree to the CSA
                if (collaborative.ApprovalStatus == ApprovalStatus.Active || collaborative.ApprovalStatus == ApprovalStatus.Declined)
                {
                    collaborative.ApprovalStatus = ApprovalStatus.Draft;
                    dbContext.Collaboratives.Update(collaborative);
                }

                dbContext.CollaborativeMembers.Add(newCollabMember);

                // revert readyForSubmittal flag to false
                collaborative.ReadyForSubmittal = false;

                await dbContext.SaveChangesAsync();

                var updatedCollab = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                        .ThenInclude(c => c.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                bool userIsCollabAdmin = false;

                // Determine if user is Collab Admin
                var user = httpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = userManager.GetUserId(user);

                    // check if user is a Collaborative Admin
                    // this info is necessary so front-end knows whether to display the "Edit" button or not
                    userIsCollabAdmin = updatedCollab.CollaborativeMembers
                        .Any(m => m.UserId == userId && m.Role == CollaborativeRole.CollaborativeAdmin);
                }

                var result = new
                {
                    updatedCollab.Id,
                    updatedCollab.Name,
                    updatedCollab.LogoUrl,
                    userIsCollabAdmin,
                    Members = updatedCollab.CollaborativeMembers
                        .Where(m => m.CollaborativeId == id)
                        .Select(m => new
                        {
                            m.User.Id,
                            m.User.FirstName,
                            m.User.LastName,
                            m.User.UserName,
                            m.User.AvatarUrl,
                            m.IsActive,
                            Role = EnumUtility.GetEnumDisplayName(m.Role),
                            InviteStatus = Enum.GetName(typeof(InviteStatus), m.InviteStatus) // Convert enum value to string
                        })
                        .ToList(),
                };

                return Results.Ok(result);
            })
            .WithOpenApi();


            // Update Collab member invite status or CSA acceptance status
            app.MapPatch("collaboratives/{id}/members/{userId}", async (int id, string userId, [FromBody] CollabMemberStatusDTO memberStatus, ApplicationDbContext dbContext) =>
            {
                var member = await dbContext.CollaborativeMembers
                    .Where(m => m.CollaborativeId == id)
                    .FirstOrDefaultAsync(m => m.UserId == userId);

                if (member == null)
                {
                    return Results.BadRequest(new { message = "Member not found in this collaborative." });
                }

                string ResultsMessage = "";

                // Handle InviteStatus
                if (!string.IsNullOrEmpty(memberStatus.InviteStatus))
                {

                    if (Enum.TryParse<InviteStatus>(memberStatus.InviteStatus, out var parsedInviteStatus))
                    {
                        // if member is being reinvited to the collab change inviteStatus and clear reasonForDecline
                        if (parsedInviteStatus == InviteStatus.Invited && member.InviteStatus == InviteStatus.Declined)
                        {
                            // remove any previous reason for decline
                            member.ReasonForCollabDecline = null;
                        }

                        if (parsedInviteStatus == InviteStatus.Declined && !string.IsNullOrWhiteSpace(memberStatus.ReasonForDecline))
                        {
                            member.ReasonForCollabDecline = memberStatus.ReasonForDecline;
                        }

                        member.InviteStatus = parsedInviteStatus;
                    }
                    else
                    {
                        return Results.BadRequest(new { message = "Invalid InviteStatus value" });
                    }

                    dbContext.CollaborativeMembers.Update(member);
                    await dbContext.SaveChangesAsync();

                    ResultsMessage = "Member invite status updated successfully!";
                }

                // update collaborative member's CSA acceptance status
                if (memberStatus.CSAId != null)
                {

                    var collaborative = await dbContext.Collaboratives
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (collaborative == null)
                    {
                        return Results.NotFound(new { message = "Collaborative not found." });
                    }

                    if (memberStatus.CSAId == collaborative.CurrentCSAId)
                    {

                        if (memberStatus.CSAAcceptedStatus == "Declined")
                        {
                            if (member != null)
                            {
                                // is user declines the CSA they're automatically kicked out of the collaborative
                                dbContext.CollaborativeMembers.Remove(member);
                            }

                            ResultsMessage = "CSA declined, user removed from collaborative.";
                            dbContext.SaveChanges();
                        }

                        if (memberStatus.CSAAcceptedStatus == "Accepted")
                        {
                            member.CSAAcceptedStatus = CSAAcceptedStatus.Accepted;
                            member.CSAAcceptedId = (int)memberStatus.CSAId;
                            member.CSAAcceptedAt = DateTime.UtcNow;
                            member.IsActive = true;

                            ResultsMessage = "CSA accepted.";
                            dbContext.CollaborativeMembers.Update(member);
                            dbContext.SaveChanges();

                            // now check and see if all collaborative members have accepted the CSA
                            var allCollaborativeMembers = await dbContext.CollaborativeMembers
                                .Where(m => m.CollaborativeId == id)
                                .ToListAsync();

                            var membersAcceptedCSA = allCollaborativeMembers
                                .Where(m => m.IsActive == true)
                                .Count();

                            if (membersAcceptedCSA == allCollaborativeMembers.Count())
                            {
                                // If all members have accepted the CSA, change collaborative ReadyForSubmittal flag to true
                                collaborative.ReadyForSubmittal = true;
                                dbContext.Collaboratives.Update(collaborative);
                                await dbContext.SaveChangesAsync();
                                ResultsMessage = "All members have accepted the CSA. Collaborative is ready for submittal.";
                            }
                            else
                            {
                                ResultsMessage = "CSA accepted, but not all members have accepted yet.";
                            }
                        }

                    }

                }

                return Results.Ok(new { message = ResultsMessage });
            })
            .WithOpenApi();

            app.MapGet("collaboratives/{id}/token-balance", async (int id, ApplicationDbContext dbContext) =>
            {
                var collab = await dbContext.Collaboratives
                    .Where(c => c.Id == id)
                    .Include(p => p.Projects)   
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (collab == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found" });
                }

                decimal launchTokensBalance = CalculateCollabTokenBalanceBasic(dbContext, collab);

                return Results.Ok(new { launchTokensBalance });
            })
            .WithOpenApi();

            // Change collab status: approve or decline
            app.MapPatch("collaboratives/{id}/status", async (int id,
                [FromBody] CollabStatusDTO collab,
                ApplicationDbContext dbContext,
                [FromServices] ITokenReleaseService tokenReleaseService) =>
            {
                var collaborative = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                        .ThenInclude(u => u.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found." });
                }

                if (string.IsNullOrEmpty(collab.Status))
                {
                    return Results.BadRequest(new { message = "Status is required." });
                }

                if (collab.Status == "approve")
                {
                    collaborative.ApprovalStatus = ApprovalStatus.Active;

                    // Now that collab is approved set SecondTokenReleaseDate
                    if (collaborative.SecondTokenReleaseDate == null)
                    {
                        collaborative.SecondTokenReleaseDate = DateTime.UtcNow.AddDays((double)collaborative.WeeksTillSecondTokenRelease * 7);
                    }

                    await dbContext.SaveChangesAsync();

                    // release tokens if necessary
                    if (tokenReleaseService.ReleaseTokens(collaborative))
                    {
                        // if token release happened we need to pay collab admin and network for last release
                        var collabAdmin = collaborative.CollaborativeMembers
                            .Where(cm => cm.Role == CollaborativeRole.CollaborativeAdmin)
                            .FirstOrDefault();

                        var lastTokenReleaseAmount = tokenReleaseService.CalculateTokensReleasedPerGivenCycleNumber(collaborative, 1);
                        var collabAdminComp = lastTokenReleaseAmount * collaborative.CollabAdminCompensationPercent;

                        var collabAdminPay = new LaunchTokenTransaction
                        {
                            Collaborative = collaborative,
                            User = collabAdmin.User,
                            PaymentType = PaymentType.CollabAdmin,
                            Amount = collabAdminComp,
                        };

                        var networkPay = new LaunchTokenTransaction
                        {
                            Collaborative = collaborative,
                            PaymentType = PaymentType.Network,
                            Amount = collabAdminComp * NetworkConstants.TransactionFee,
                        };

                        dbContext.LaunchTokenTransactions.Add(collabAdminPay);
                        dbContext.LaunchTokenTransactions.Add(networkPay);

                        await dbContext.SaveChangesAsync();
                    }

                    return Results.Ok(new { message = "Collaborative status updated to Approved successfully!" });
                }
                else if (collab.Status == "decline")
                {
                    collaborative.ApprovalStatus = ApprovalStatus.Declined;
                    collaborative.ReasonForDecline = collab.ReasonForDecline;

                    await dbContext.SaveChangesAsync();
                    return Results.Ok(new { message = "Collaborative status updated to Declined successfully!" });
                }
                else
                {
                    return Results.BadRequest(new { message = "Invalid status value" });
                }
            })
            .WithOpenApi();


            // Submit collab for approval
            app.MapPost("collaboratives/{id}/submit", async (int id, ApplicationDbContext dbContext) =>
            {
                var collaborative = await dbContext.Collaboratives
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found." });
                }

                if (collaborative.ApprovalStatus != ApprovalStatus.Draft && collaborative.ApprovalStatus != ApprovalStatus.Declined)
                {
                    return Results.BadRequest(new { message = "Only collaboratives in Draft or Declined status can be submitted for approval." });
                }

                if (!collaborative.ReadyForSubmittal)
                {
                    return Results.BadRequest(new { message = "Collaborative is not ready for submittal. Ensure all members have accepted the CSA." });
                }

                collaborative.ApprovalStatus = ApprovalStatus.Submitted;
                await dbContext.SaveChangesAsync();

                return Results.Ok(new { message = "Collaborative submitted for approval successfully!" });
            })
            .WithOpenApi();

            app.MapGet("collaboratives/{id}/CSAAgreement", async (int id, ApplicationDbContext dbContext) =>
            {
                var CSAAgreement = await dbContext.Collaboratives
                    .Where(c => c.Id == id)
                    .Include(c => c.CurrentCSA)
                    .Select(c => new
                    {
                        csaId = c.CurrentCSAId,
                        csaUrl = c.CurrentCSA.Url,
                        collabName = c.Name,
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return Results.Ok(CSAAgreement);

            })
            .WithOpenApi();

            // Add new CSA to Collab
            app.MapPost("collaboratives/{id}/csa", async (int id, [FromBody] CsaDocUrlDTO dto, ApplicationDbContext dbContext) =>
            {
                var collaborative = await dbContext.Collaboratives
                    .Include(c => c.CollaborativeMembers)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (collaborative == null)
                {
                    return Results.NotFound(new { message = "Collaborative not found." });
                }

                if (string.IsNullOrEmpty(dto.CsaDocUrl))
                {
                    return Results.BadRequest(new { message = "CSA document URL is required." });
                }

                await AddCsaToCollaborativeAsync(dbContext, collaborative, dto.CsaDocUrl);
                return Results.Ok(new { message = "New CSA document URL added successfully!" });
            })
            .WithOpenApi();

        }
        private static async Task AddCsaToCollaborativeAsync(ApplicationDbContext dbContext, Collaborative collaborative, string csaDocUrl)
        {
            // Create and add the new CSA
            var newCSA = new CSA
            {
                Url = csaDocUrl,
                CollaborativeId = collaborative.Id
            };

            dbContext.CSAs.Add(newCSA);
            await dbContext.SaveChangesAsync();

            // Set as current CSA
            collaborative.CurrentCSAId = newCSA.Id;

            var members = await dbContext.CollaborativeMembers
                .Where(m => m.CollaborativeId == collaborative.Id)
                .ToListAsync();

            if (members == null || !members.Any())
            {
                // No members to update, just save the collaborative changes
                dbContext.Collaboratives.Update(collaborative);
                await dbContext.SaveChangesAsync();
                return;
            }

            // Revert CSAAccepted to false for all collab members because the CSA has been updated
            foreach (var member in members)
            {
                member.CSAAcceptedStatus = CSAAcceptedStatus.NotAccepted;
                member.CSAAcceptedAt = null; // Reset the acceptance date
                member.IsActive = false;
                dbContext.CollaborativeMembers.Update(member);
            }

            await dbContext.SaveChangesAsync();
        }

        private static double GetCurrentCycleNum(Collaborative collab)
        {
            double currentCycleNum = 1;
            var now = DateTime.UtcNow;

            if (collab.SecondTokenReleaseDate != null && collab.SecondTokenReleaseDate <= now)
            {
                // we're past the SecondTokenReleaseDate and can start calculating cycles by date,
                // adding 2: 1 for the initial release at time of collaborative proposal and 2 at SecondTokenReleaseDate
                currentCycleNum = Math.Floor((now - collab.SecondTokenReleaseDate.Value).TotalDays / (collab.LaunchCyclePeriod.Value * 7) + 2);

            }

            return currentCycleNum;
        }

        private static DateTime? GetNextTokenReleaseDate(Collaborative collab, double currentCycleNum)
        {
            DateTime? nextTokenReleaseDate = DateTime.UtcNow;

            if (collab.LastTokenRelease != null && collab.SecondTokenReleaseDate != null && collab.LaunchCyclePeriod != null)
            {
                nextTokenReleaseDate = collab.LastTokenRelease.Value.AddDays(collab.LaunchCyclePeriod.Value * 7);

                if (currentCycleNum == 1 && collab.WeeksTillSecondTokenRelease != null)
                {
                    nextTokenReleaseDate += TimeSpan.FromDays((double)collab.WeeksTillSecondTokenRelease * 7);
                }

            }
            else
            {
                nextTokenReleaseDate = null;
            }

            return nextTokenReleaseDate;
        }

        private static decimal CalculateCumulativeReleasedTokens(Collaborative collab, double currentCycleNum)
        {
            var tokenReleaseRate = collab.TokenReleaseRate;
            var inverseLaunchTokensPriorWorkPercent = 1 - (collab.LaunchTokensPriorWorkPercent);

            // calculate geometric series sum for all releases up to currentCycleNum
            decimal r = 1 - tokenReleaseRate.Value;
            int n = (int)currentCycleNum;
            decimal a = collab.LaunchTokensCreated.Value * inverseLaunchTokensPriorWorkPercent.Value * tokenReleaseRate.Value;
            decimal cumulativeReleasedTokens = 0m;

            if (tokenReleaseRate > 0 && r != 1 && n > 0)
            {
                cumulativeReleasedTokens = a * (1 - (decimal)Math.Pow((double)r, n)) / (1 - r);
            }

            return cumulativeReleasedTokens;
        }

        public static decimal CalculateCollabTokenBalance(ApplicationDbContext dbContext, Collaborative collab)
        {
            var currentCycleNum = GetCurrentCycleNum(collab);
            var networkTransactionMultiplier = 1 + NetworkConstants.TransactionFee;

            // get sum of all token payments for completed milestones
            decimal tokensPaidForMilestones = collab.Projects
                .SelectMany(p => p.Milestones)
                .Where(m => m.ApprovalStatus == ApprovalStatus.Archived)
                .Sum(m => m.AllocatedLaunchTokens);

            tokensPaidForMilestones *= networkTransactionMultiplier;

            decimal collabAdminAndNetworkTransactionPayments = dbContext.LaunchTokenTransactions
                .Where(lt => lt.CollaborativeId == collab.Id &&
                    (lt.PaymentType == PaymentType.CollabAdmin || lt.PaymentType == PaymentType.Network))
                .Sum(lt => lt.Amount);

            var cumulativeReleasedTokens = CalculateCumulativeReleasedTokens(collab, currentCycleNum);

            var tokenBalance = cumulativeReleasedTokens - tokensPaidForMilestones - collabAdminAndNetworkTransactionPayments;

            return tokenBalance;
        }

        public static decimal CalculateCollabTokenBalanceBasic(ApplicationDbContext dbContext, Collaborative collab)
        {
            var currentCycleNum = GetCurrentCycleNum(collab);
            var networkTransactionMultiplier = 1 + NetworkConstants.TransactionFee;

            // get sum of all project token budgets
            decimal sumProjectBudgets = collab.Projects
                .Sum(m => m.LaunchTokenBudget);

            decimal collabAdminAndNetworkTransactionPayments = dbContext.LaunchTokenTransactions
                .Where(lt => lt.CollaborativeId == collab.Id &&
                    (lt.PaymentType == PaymentType.CollabAdmin || lt.PaymentType == PaymentType.Network))
                .Sum(lt => lt.Amount);

            var cumulativeReleasedTokens = CalculateCumulativeReleasedTokens(collab, currentCycleNum);

            var tokenBalance = cumulativeReleasedTokens - sumProjectBudgets - collabAdminAndNetworkTransactionPayments;

            return tokenBalance;
        }

        public record StakingTierDTO(string Tier, decimal ExchangeRate);

        public record UpdateCollaborativeDTO(
            string? CollabRole,
            string? UserId,
            string? UserRole,
            string? Name,
            string? Description,
            string? WebsiteUrl,
            string? LogoUrl,
            string? City,
            string? State,
            int[]? Skills,
            int[]? Experience
        );

        public record UpdateTreasuryDTO(
            int? TokensCreated,
            decimal? TokenReleaseRate,
            int? LaunchCyclePeriodWeeks,
            int? TokenSecondReleaseWeeks,
            decimal? TokensPriorWorkPercent,
            decimal? CollabAdminCompensationPercent,
            decimal? TokenValue
        );

        public record CollabStatusDTO(
            string Status,
            string? ReasonForDecline
        );

        public record NewMemberDTO(
            string UserId,
            string UserRole
        );

        public record CSAfeedbackDTO(
            int CSAId,
            string CSAAcceptedStatus
        );

        public record CollabMemberStatusDTO(
            string? InviteStatus,
            string? ReasonForDecline,
            int? CSAId,
            string? CSAAcceptedStatus
        );

        public record CsaDocUrlDTO(string CsaDocUrl);
    }
}

