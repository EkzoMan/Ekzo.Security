using System;

namespace AuthLib.Core.Authorization
{

    public class RoleProvider : System.Web.Security.RoleProvider
    {
        /// <summary>
        /// Adds the specified user names to the specified roles.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>The name of the application to store role information for.</returns>
        /// <remarks>Setting application name will always throw exection</remarks>
        public override string ApplicationName
        {
            get
            { 
                return AuthLib.Helpers.Settings.ApplicationName;
            }
            set
            {
                throw new Exception("Application name cannot be set outside core library.");
            }
        }

        /// <summary>
        /// Adds a new role to the data source.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (!AuthLib.Helpers.RoleProviderHelper.AddGroup(roleName))
                throw new Exception("Ошибка при создании роли.");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all the roles.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source.
        /// </returns>
        public override string[] GetAllRoles()
        {
           return AuthLib.Helpers.RoleProviderHelper.GetGroups();
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            return AuthLib.Helpers.RoleProviderHelper.GetUserGroups(username);
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            return AuthLib.Helpers.RoleProviderHelper.GetGroupUsers(roleName); 
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
                    return AuthLib.Helpers.RoleProviderHelper.IsUserInGroup(username, roleName);
          
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            return AuthLib.Helpers.RoleProviderHelper.GroupExists(roleName);
        }
    }
}
