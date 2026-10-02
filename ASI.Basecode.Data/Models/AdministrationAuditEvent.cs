using System;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Records administrative account and role changes without storing secrets.
    /// </summary>
    public class AdministrationAuditEvent
    {
        public long AdministrationAuditEventId { get; set; }
        public string ActorUserId { get; set; }
        public string TargetUserId { get; set; }
        public string TargetRoleId { get; set; }
        public string Action { get; set; }
        public string DetailsJson { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
