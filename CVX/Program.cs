using CVX.Endpoints;
using CVX.Models;
using CVX.Services;
using CVX.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using System.Data;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(360);
    options.SlidingExpiration = true;
});
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();
builder.Services.AddScoped<ITokenReleaseService, TokenReleaseService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddAuthorization();
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                    "https://vite-react-omega-gilt.vercel.app",
                    "https://next-front-cvx.vercel.app",
                    "https://c-vx-mantine-ui.vercel.app",
                    "https://c-vx-mantine-ui-knwx.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

app.UseHttpsRedirection();

app.MapCollaborativeEndpoints();
app.MapProjectEndpoints();
app.MapMilestoneEndpoints();
app.MapMemberEndpoints();
app.MapProfileEndpoints();
app.MapIdentityEndpoints();

app.MapGet("/heartbeat", (HttpRequest request) =>
{
    var keepAliveSecret = request.Headers["x-keepalive-secret"].FirstOrDefault();
    var expectedSecret = builder.Configuration["KeepAliveSecret"];

    if (string.IsNullOrEmpty(expectedSecret) || keepAliveSecret != expectedSecret)
    {
        return Results.Unauthorized();
    }

    Log.Information("Heartbeat received at {Time}", DateTime.UtcNow);

    return Results.Ok(new { status = "alive" });
})
.WithOpenApi();

// NOT ALL DATA PASSED THROUGH ENDPOINT IF USER ISN'T A NETWORK ADMIN
app.MapGet("/dashboard", async (HttpContext httpContext, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext) =>
{
    var user = httpContext.User;

    if (user.Identity?.IsAuthenticated == true)
    {
        var username = user.Identity.Name;
        var applicationUser = await userManager.FindByNameAsync(username);

        var allCollabMemberships = await dbContext.CollaborativeMembers
            .Where(c => c.UserId == applicationUser.Id)
            .Include(c => c.Collaborative)
                .ThenInclude(collab => collab.CurrentCSA)
            .AsNoTracking()
            .ToListAsync();

        var allProjectMemberships = await dbContext.ProjectMembers
            .Where(pm => pm.UserId == applicationUser.Id)
            .Include(pm => pm.Project)
                .ThenInclude(c => c.Collaborative)
            .AsNoTracking()
            .ToListAsync();

        // get user info
        var currentUser = new
        {
            applicationUser.Id,
            Username = applicationUser.UserName,
            applicationUser.FirstName,
            applicationUser.LastName,
            CreatedAt = applicationUser.CreatedAt.ToString("MM-dd-yyyy"),
            MemberStatus = EnumUtility.GetEnumDisplayName(applicationUser.NetworkMemberStatus),
            applicationUser.Bio,
            applicationUser.LinkedIn,
            applicationUser.AvatarUrl,
        };

        // Get any collaborative member invites
        var collabInvites = allCollabMemberships
            .Where(c => c.InviteStatus == InviteStatus.Invited)
            .Select(c => new
            {
                CollabId = c.CollaborativeId,
                CollabName = c.Collaborative.Name,
                CollabLogoUrl = c.Collaborative.LogoUrl,
                c.UserId,
                UserRole = EnumUtility.GetEnumDisplayName(c.Role),
                InviteStatus = EnumUtility.GetEnumDisplayName(c.InviteStatus)
            })
            .ToList();

        // Get any project member invites
        var projectInvites = allProjectMemberships
            .Where(pm => pm.InviteStatus == InviteStatus.Invited)
            .Select(pm => new
            {
                ProjectId = pm.Project.Id,
                ProjectName = pm.Project.Name,
                CollabId = pm.Project.CollaborativeId,
                CollabLogoUrl = dbContext.Collaboratives.Find(pm.Project.CollaborativeId).LogoUrl,
                pm.UserId,
                UserRole = EnumUtility.GetEnumDisplayName(pm.Role),
                InviteStatus = EnumUtility.GetEnumDisplayName(pm.InviteStatus)
            })
            .ToList();

        // Get any milestone assignments requiring acceptance by their assignees
        var milestoneAssignments = await dbContext.Milestones
            .Where(m => m.Assignee != null && m.Assignee.UserId == applicationUser.Id && m.AssigneeStatus == AssigneeStatus.Assigned)
            .Include(m => m.Project)
                .ThenInclude(p => p.Collaborative)
            .AsNoTracking()
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Description,
                DueDate = m.DueDate.HasValue
                    ? m.DueDate.Value.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)
                    : null,
                LaunchTokens = m.AllocatedLaunchTokens,
                ProjectName = m.Project.Name,
                CollabName = m.Project.Collaborative.Name,
                CollabLogoUrl = m.Project.Collaborative.LogoUrl
            })
            .ToListAsync();

        // Get all project IDs where the user is a Project Admin
        var adminProjectIds = await dbContext.ProjectMembers
            .Where(pm => pm.UserId == applicationUser.Id && pm.Role == ProjectRole.ProjectAdmin)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        // Milestones in projects where the user is a Project Admin and need completion approval
        var milestoneCompletions = await dbContext.Milestones
            .Where(m =>
                adminProjectIds.Contains(m.ProjectId) &&
                m.AssigneeStatus == AssigneeStatus.Accepted &&
                m.IsComplete &&
                m.ApprovalStatus == ApprovalStatus.Submitted)
            .Include(m => m.Project)
                .ThenInclude(p => p.Collaborative)
            .AsNoTracking()
                    .Select(m => new
            {
                m.Id,
                m.Name,
                m.Description,
                ProjectId = m.Project.Id,
                ProjectName = m.Project.Name,
                CollabId = m.Project.CollaborativeId,
                CollabName = m.Project.Collaborative.Name,
                CollabLogoUrl = m.Project.Collaborative.LogoUrl
            })
            .ToListAsync();

        // Get all user collaboratives for display on their dashboard
        var collabs = allCollabMemberships
            .Select(c => new
            {
                Id = c.CollaborativeId,
                c.Collaborative.Name,
                c.Collaborative.Description,
                c.Collaborative.LogoUrl,
                Status = EnumUtility.GetEnumDisplayName(c.Collaborative.ApprovalStatus),
            })
            .ToList();

        // Get all user projects for display on their dashboard
        var projects = allProjectMemberships
            .Select(p => new
            {
                Id = p.ProjectId,
                CollabId = p.Project.CollaborativeId,
                CollabName = p.Project.Collaborative.Name,
                p.Project.Name,
                p.Project.Description,
                ApprovalStatus = EnumUtility.GetEnumDisplayName(p.Project.ApprovalStatus),
                CreatedAt = p.Project.CreatedAt.ToString("MM-dd-yyyy"),
                Budget = p.Project.LaunchTokenBudget,
            })
            .ToList();

        // Get any CSA approval requests
        var csaApprovalRequests = allCollabMemberships
            .Where(c => c.InviteStatus == InviteStatus.Accepted
                    && c.CSAAcceptedStatus == CSAAcceptedStatus.NotAccepted
                    && c.Collaborative.CurrentCSAId != null)
            .Select(c => new
            {
                CollabId = c.CollaborativeId,
                CollabName = c.Collaborative.Name,
                CollabLogoUrl = c.Collaborative.LogoUrl,
                CurrentCSA = c.Collaborative.CurrentCSAId,
                CurrentCSAUrl = c.Collaborative.CurrentCSA.Url,
                c.UserId
            })
            .ToList();

        // Get any project approval requests
        var projectsNeedingApproval = allProjectMemberships
            .Where(pm => pm.InviteStatus == InviteStatus.Accepted
                    && (pm.Project.ApprovalStatus == ApprovalStatus.Submitted || pm.Project.ApprovalStatus == ApprovalStatus.Declined)
                    && pm.Role == ProjectRole.ProjectMember
                    && pm.IsActive == false
                    )
            .Select(pm => new  
            {
                Id = pm.ProjectId,
                pm.Project.Name,
                CollabId = pm.Project.CollaborativeId,
                CollabName = dbContext.Collaboratives.Find(pm.Project.CollaborativeId).Name,
                CollabLogoUrl = dbContext.Collaboratives.Find(pm.Project.CollaborativeId).LogoUrl,
                pm.UserId
            })
            .ToList();

        // If the user is a Network Admin, fetch additional data
        if (applicationUser.NetworkMemberStatus == NetworkMemberStatus.NetworkAdmin)
        {
            var usersNeedingApproval = await dbContext.Users
                .Where(c => c.NetworkMemberStatus == NetworkMemberStatus.Applicant)
                .Select(user => new
                {
                    user.Id,
                    Username = user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Bio,
                    user.City,
                    user.State,
                    user.PhoneNumber,
                    user.LinkedIn,
                    user.AvatarUrl,
                    CreatedAt = user.CreatedAt.ToString("MM-dd-yyyy"),
                    MemberStatus = EnumUtility.GetEnumDisplayName(user.NetworkMemberStatus)
                })
                .AsNoTracking()
                .ToListAsync();

            // Get all NetworkMemberStatus enum values
            var roles = Enum.GetValues(typeof(NetworkMemberStatus))
                .Cast<NetworkMemberStatus>()
                .Select(status => EnumUtility.GetEnumDisplayName(status))
                .ToList();

            // Get all collabs that need approval from Network Owner to go live
            var collabsNeedingApproval = await dbContext.Collaboratives
                .Where(c => c.ApprovalStatus == ApprovalStatus.Submitted)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.LogoUrl,
                })
                .AsNoTracking()
                .ToListAsync();

            // Combine the data into a single response
            var dashboardData = new
            {
                currentUser,
                usersNeedingApproval,
                roles,
                collabs,
                collabsNeedingApproval,
                collabInvites,
                csaApprovalRequests,
                projects,
                projectsNeedingApproval,
                projectInvites,
                milestoneAssignments,
                milestoneCompletions,
            };

            return Results.Ok(dashboardData);
        }

        // If the user is not a Network Admin, return only necessary data
        return Results.Ok(new
        {
            currentUser,
            collabs,
            collabInvites,
            csaApprovalRequests,
            projects,
            projectInvites,
            projectsNeedingApproval,
            milestoneAssignments,
            milestoneCompletions
        });
    }
    return Results.Unauthorized();

})
.WithOpenApi();


app.MapGet("/skills-and-experience", async (ApplicationDbContext dbContext) =>
{
    var sectors = await dbContext.Sectors
        .Select(sector => new
        {
            id = sector.Id,
            value = sector.Sector,
        })
        .AsNoTracking()
        .ToListAsync();

    var skills = await dbContext.Skills
        .AsNoTracking()
        .ToListAsync();

    var skillGroupsByNameId = await dbContext.SkillGroups
        .AsNoTracking()
        .ToDictionaryAsync(g => g.Id, g => g.GroupName);

    var skillsWithGroupName = skills.Select(skill => new
    {
        id = skill.Id,
        value = (skillGroupsByNameId.TryGetValue(skill.SkillGroupsId, out var groupName) ? groupName : "") + ": " + skill.Skill
    }).ToList();


    var result = new
    {
        skills = skillsWithGroupName,
        experience = sectors
    };

    return Results.Json(result);

})
.WithOpenApi();

app.MapPost("/invite", async ([FromBody] InviteRequest request, HttpContext httpContext, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IEmailService emailService) =>
{
    var user = httpContext.User;

    if (user.Identity?.IsAuthenticated == true)
    {
        var username = user.Identity.Name;
        var applicationUser = await userManager.FindByNameAsync(username);

        if (applicationUser == null)
        {
            return Results.Unauthorized();
        }

        // Generate token, store it, and send email
        var token = Guid.NewGuid().ToString();

        var invitation = new Invitation
        {
            Email = request.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync();

        var inviteLink = $"https://c-vx-mantine-ui-knwx.vercel.app/invite?token={token}";
        await emailService.SendInvitationEmailAsync(request.Email, inviteLink, applicationUser.Email, applicationUser.FullName);

        return Results.Ok(new { message = "Invite sent to " + request.Email });
    }
    return Results.Unauthorized();
})
.WithOpenApi();

app.MapPost("accept-invite", async ([FromBody] FormData formData, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) =>
{
    // Validate token and accept invite
    var invitation = await dbContext.Invitations
        .FirstOrDefaultAsync(i => i.Token == formData.Token && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);

    if (invitation == null)
    {
        return Results.BadRequest(new { message = "Invalid or expired invitation token." });
    }

    // Create a new user or update existing user with the provided data
    var user = await userManager.FindByEmailAsync(invitation.Email);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = invitation.Email,
            FirstName = formData.FirstName,
            LastName = formData.LastName,
            Email = invitation.Email,
            Bio = formData.Bio,
            City = formData.City,
            State = formData.State,
            PhoneNumber = formData.PhoneNumber,
            LinkedIn = formData.LinkedIn,
            NetworkMemberStatus = NetworkMemberStatus.NetworkContributor,
        };

        var result = await userManager.CreateAsync(user, formData.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = result.Errors.FirstOrDefault()?.Description });
        }
    } else
    {
        // Mark the invitation as used
        invitation.IsUsed = true;
        await dbContext.SaveChangesAsync();

        return Results.BadRequest(new { message = "This user is already a member." } );
    }

    // Mark the invitation as used
    invitation.IsUsed = true;
    await dbContext.SaveChangesAsync();
    return Results.Ok(new { message = "Invitation accepted successfully." });
})
.WithOpenApi();

app.Run();

public record FormData(
    string Token,
    string Password,
    string FirstName,
    string LastName,
    string Bio,
    string City,
    string State,
    string PhoneNumber,
    string LinkedIn
);

public record InviteRequest(string Email, string InviterUserId);
