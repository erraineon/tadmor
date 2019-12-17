namespace Tadmor.Services.Commands
{
    public class CommandUsagePermission
    {
        public CommandUsagePermission(ulong scopeId, CommandUsagePermissionScopeType scopeType, string commandName)
        {
            ScopeId = scopeId;
            ScopeType = scopeType;
            CommandName = commandName;
        }

        public ulong ScopeId { get; }
        public CommandUsagePermissionScopeType ScopeType { get; }
        public PermissionType PermissionType { get; set; }
        public string CommandName { get; }

        public override string ToString()
        {
            return $"{PermissionType} {ScopeType} {ScopeId} for command {CommandName}";
        }
    }
}