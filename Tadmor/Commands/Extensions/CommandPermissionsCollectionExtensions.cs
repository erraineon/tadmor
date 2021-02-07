using System.Collections.Generic;
using System.Linq;
using Tadmor.Commands.Models;

namespace Tadmor.Commands.Extensions
{
    public static class CommandPermissionsCollectionExtensions
    {
        public static void AddOrUpdate<TPermission>(this ICollection<TPermission> permissions, TPermission permission)
            where TPermission : CommandPermission
        {
            var existingPermission = permissions.SingleOrDefault(p => p.CommandName == permission.CommandName);
            if (existingPermission != null) permissions.Remove(existingPermission);
            permissions.Add(permission);
        }
    }
}