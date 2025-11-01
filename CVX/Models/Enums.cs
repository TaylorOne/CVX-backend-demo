using System.ComponentModel.DataAnnotations;

namespace CVX.Models
{
    public enum ApprovalStatus
    {
        Draft,
        Submitted,
        Active,
        Closed,
        Declined,
        Archived,
    }

    public enum NetworkMemberStatus
    {
        [Display(Name = "Applicant")]
        Applicant,
        [Display(Name = "Denied Applicant")]
        DeniedApplicant,
        [Display(Name = "Network Admin")]
        NetworkAdmin,
        [Display(Name = "Network Contributor")]
        NetworkContributor,
    }

    public enum CollaborativeRole
    {
        [Display(Name = "Collaborative Admin")]
        CollaborativeAdmin,
        [Display(Name = "Collaborative Member")]
        CollaborativeMember,
    }

    public enum ProjectRole
    {
        [Display(Name = "Project Admin")]
        ProjectAdmin,
        [Display(Name = "Project Member")]
        ProjectMember,
    }

    public enum InviteStatus
    {
        Invited,
        Accepted,
        Declined
    }

    public enum AssigneeStatus
    {
        Assigned,
        Accepted,
        Declined
    }
}
