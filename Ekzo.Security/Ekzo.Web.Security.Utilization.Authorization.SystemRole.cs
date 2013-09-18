using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Ekzo.Web.Security.Utilization.Authorization
{
    /// <summary>
    /// Описывает роли из БД 
    /// </summary>
    public class SystemRole
    {
        /// <summary>
        /// Идентификатор роли из базы разграничения прав
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Идентификатор роли из БД 
        /// </summary>
        public int baseID { get; private set; }
        /// <summary>
        /// Имя роли
        /// </summary>
        public string Role { get; private set; }
        public int ActionGroup { get; private set; }

        public SystemRole() { }

        public SystemRole(string role)
        {
            InitClass(role);
        }

        public SystemRole(int actionGorup, string role)
        {
            InitClass(role, actionGorup);
        }

        public SystemRole(int baseID, int actionGroup = 0)
        {

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_name FROM groups WHERE group_id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", baseID);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            this.Role = reader.GetString(0);
                        this.baseID = baseID;
                        this.ActionGroup = actionGroup;
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация роли из справочника] ", ex);
                    }
                }
            }
        }

        private void InitClass(string role, int actionGroup = -1)
        {
            this.ActionGroup = actionGroup;
            this.Role = role;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_RoleToActionGroup WHERE ActionGroup=@groupID AND Role=@role", conn))
                {
                    try
                    {
                        if (actionGroup == -1) cmd.CommandText = "SELECT * FROM Authorization_RoleToActionGroup WHERE Role=@role";
                        conn.Open();
                        cmd.Parameters.AddWithValue("@groupID", actionGroup);
                        cmd.Parameters.AddWithValue("@role", role);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.Role = reader.GetString(2);
                            this.ActionGroup = reader.GetInt32(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация роли] ", ex);
                    }
                }
            }

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_id FROM groups WHERE group_name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", this.Role);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            this.baseID = reader.GetInt32(0);
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация роли. Получение роли из справочника] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Создание новой роли
        /// </summary>
        /// <param name="Name">Название роли</param>
        /// <returns></returns>
        public static SystemRole CreateRole(string Name)
        {
            SystemRole role = new SystemRole();
            role.Role = Name;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO groups(group_id,group_name) SELECT MIN(group_id)-1,@name FROM groups WHERE group_id>-1000", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", Name);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT group_id FROM groups WHERE group_name=@name";
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            role.baseID = reader.GetInt32(0);
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Добавление роли в справочник] ", ex);
                    }
                }
            }

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO Authorization_RoleToActionGroup(ActionGroup,Role) VALUES(@group,@name)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", role.Role);
                        cmd.Parameters.AddWithValue("@group", -1);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Добавление роли в таблицу безопасности] ", ex);
                    }
                }
            }
            role.ActionGroup = -1;
            return role;
        }
        /// <summary>
        /// Получает список ролей пользователя
        /// </summary>
        /// <param name="employeeID">Идентификатор сотрудника</param>
        /// <returns></returns>
        public static string[] GetEmployeeRoles(int employeeID)
        {
            List<string> roles = new List<string>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_name FROM employee2group INNER JOIN groups ON intranet_employee2group.group_id = groups.group_id WHERE employee_id=@employeeID ORDER BY group_name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@employeeID", employeeID);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            roles.Add(reader.GetString(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение ролей пользователя в системе] ", ex);
                    }
                }
            }
            List<string> roleToDelete = new List<string>();
            foreach (string role in roles)
                if (!HttpContext.Current.User.IsInRole(role)) roleToDelete.Add(role);
            for (int i = 0; i < roleToDelete.Count; i++)
                roles.Remove(roleToDelete[i]);


            return roles.ToArray();
        }
        /// <summary>
        /// Сохранение роли
        /// </summary>
        public void Save()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"
                        IF((SELECT COUNT(*) FROM Authorization_RoleToActionGroup WHERE id=@id OR Role=@name AND ActionGroup=@groupID)=0) 
                            INSERT INTO Authorization_RoleToActionGroup(ActionGroup,Role) VALUES(@groupID, @name) 
                        ELSE 
                            UPDATE Authorization_RoleToActionGroup SET ActionGroup=@groupID, Role=@name WHERE id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@name", this.Role);
                        cmd.Parameters.AddWithValue("@groupID", this.ActionGroup);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT * FROM Authorization_RoleToActionGroup WHERE Role=@name AND ActionGroup=@groupID";
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.ActionGroup = reader.GetInt32(1);
                            this.Role = reader.GetString(2);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение роли в системе безопасности] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление роли
        /// </summary>
        /// <remarks>Удаление производится как из таблицы базы раграничения прав, так и из </remarks>
        public void Delete()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_RoleToActionGroup WHERE id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM Authorization_RoleToActionGroup WHERE ActionGroup=@id";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление роли из системы безопасности] ", ex);
                    }
                }
            }

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM employee2group WHERE group_id=@groupID", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@groupID", this.baseID);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.AddWithValue("@name", this.Role);
                        cmd.CommandText = "DELETE FROM groups WHERE group_name=@name";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление привязки пользователей к роли в справочнике] ", ex);
                    }
                }
            }
        }
        #region StaticFields
        /// <summary>
        /// Получение списка всех ролей системы безопасности
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllRoles()
        {
            List<string> result = new List<string>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT Role FROM Authorization_RoleToActionGroup ORDER BY Role", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(reader.GetString(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка ролей в системе безопасности] ", ex);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получение списка всех полей интранета
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllIntranetRoles()
        {
            List<string> result = new List<string>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT group_name FROM groups ORDER BY group_name", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(reader.GetString(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка ролей из справочника] ", ex);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получение списка пользователей для роли
        /// </summary>
        /// <param name="role">Имя роли</param>
        /// <returns></returns>
        public static List<BaseClasses.Employee> EmployeesInRole(string role)
        {
            List<BaseClasses.Employee> result = new List<BaseClasses.Employee>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT intranet_employee2group.employee_id FROM groups INNER JOIN intranet_employee2group on groups.group_id=intranet_employee2group.group_id WHERE group_name=@groupName", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@groupName", role);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(new BaseClasses.Employee(reader.GetInt32(0)));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка пользователей, имеющих роль] ", ex);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получение списка ролей для группы
        /// </summary>
        /// <param name="GroupName">Имя группы</param>
        /// <returns></returns>
        public static SystemRole[] GetGroupRoles(string GroupName)
        {
            ActionGroup group = new ActionGroup(GroupName);
            return GetGroupRoles(group.id);
        }
        /// <summary>
        /// Получение списка ролей для группы
        /// </summary>
        /// <param name="groupID">Идентификатор группы по БД разграничения правд доступа</param>
        /// <returns></returns>
        public static SystemRole[] GetGroupRoles(int groupID)
        {
            List<SystemRole> rolesList = new List<SystemRole>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_RoleToActionGroup WHERE ActionGroup=@groupID", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@groupID", groupID);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            rolesList.Add(new SystemRole(reader.GetInt32(1), reader.GetString(2)));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение ролей группы действий] ", ex);
                    }
                }
            }
            return rolesList.ToArray();
        }
        public static int? IntranetRoleID(string roleName)
        {
            int? result = null;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_id FROM groups WHERE group_name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", roleName);
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                            while (reader.Read())
                                result = reader.GetInt32(0);
                    }
                    catch (Exception ex)
                    {
                        Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение идентификатора роли]", ex);
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
