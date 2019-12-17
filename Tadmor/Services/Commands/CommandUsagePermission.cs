namespace Tadmor.Services.Commands
{
    public class CommandUsagePermission
    {
        public ulong ScopeId { get; set; }
        public CommandUsagePermissionScopeType ScopeType { get; set; }
        public PermissionType PermissionType { get; set; }
        public string CommandName { get; set; }

        public override string ToString()
        {
            return $"{PermissionType} {ScopeType} {ScopeId} for command {CommandName}";
        }
    }
}