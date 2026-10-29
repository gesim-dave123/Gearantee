namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Marks that a role's initial permissions have been provisioned.
    /// Once marked, normal seeding must not overwrite administrator choices.
    /// </summary>
    public class RolePermissionSeed
    {
        public string RoleId { get; set; }
    }
}
