using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ekzo.Web.Security.Utilization.Authorization
{
    /// <summary>
    /// Класс описывает группы методов
    /// </summary>
    public class ActionGroup
    {
        /// <summary>
        /// Идентификатор группы в БД
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Список ролей, которым разрешен доступ к группе
        /// </summary>
        public SystemRole[] Roles { get; private set; }
        /// <summary>
        /// Название группы
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Идентификатор активности группы. Если группа не активны, правила перестают работать.
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Методы группы.
        /// </summary>
        public Action[] GroupActions { get; private set; }
        #region ClassBuilder
        public ActionGroup(int id)
        {
            InitClass(null, id);
        }
        public ActionGroup(string name) { InitClass(name); }
        private void InitClass(string name, int id = 0)
        {
            if (id != 0) this.id = id;
            if (!string.IsNullOrEmpty(name)) this.Name = name;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_ActionGroups WHERE id=@id OR Name=@groupName", conn))
                {
                    try
                    {
                        conn.Open();
                        if (!string.IsNullOrEmpty(name) && id == 0) cmd.CommandText = "SELECT * FROM Authorization_ActionGroups WHERE Name=@groupName";
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@groupName", this.Name == null ? "" : this.Name);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            if (string.IsNullOrEmpty(this.Name)) this.Name = reader.GetString(1);
                            this.Active = reader.GetBoolean(2);
                        }
                        reader.Close();
                        cmd.CommandText = "SELECT * FROM Authorization_RoleToActionGroup WHERE ActionGroup=@groupID";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@groupID", this.id);
                        reader = cmd.ExecuteReader();
                        List<SystemRole> rolesList = new List<SystemRole>();
                        while (reader.Read())
                            rolesList.Add(new SystemRole(this.id, reader.GetString(2)));
                        this.Roles = rolesList.ToArray();
                        reader.Close();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        List<Action> actionsList = new List<Action>();
                        cmd.CommandText = "SELECT ActionID FROM Authorization_ActionToGroup WHERE GroupID=@id";
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            actionsList.Add(new Action(reader.GetInt32(0)));
                        }
                        this.GroupActions = actionsList.Distinct().ToArray();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация класса действия] ", ex);
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// Сохранение группы
        /// </summary>
        public void Save()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"
                        IF((SELECT COUNT(*) FROM Authorization_ActionGroups WHERE Name=@name)=0) 
                            INSERT INTO Authorization_ActionGroups(Name) VALUES(@name) 
                        ELSE 
                            UPDATE Authorization_ActionGroups SET Name=@name, Active=@active WHERE id=@id
                     
                        SELECT * FROM Authorization_ActionGroups WHERE Name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.Parameters.AddWithValue("@active", this.Active);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.Name = reader.GetString(1);
                            this.Active = reader.GetBoolean(2);
                        }
                        reader.Close();
                        cmd.CommandText = "SELECT * FROM Authorization_RoleToActionGroup WHERE ActionGroup=@id";
                        reader = cmd.ExecuteReader();
                        List<SystemRole> rolesList = new List<SystemRole>();
                        while (reader.Read())
                            rolesList.Add(new SystemRole(reader.GetString(2)));
                        this.Roles = rolesList.ToArray();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение группы действий] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление группы.
        /// </summary>
        /// <remarks>При удалении группы также выполняется удаление всех связанных элементов.</remarks>
        public void Delete()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_ActionGroups WHERE id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM Authorization_RoleToActionGroup WHERE ActionGroup=@id";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM Authorization_ActionToGroup WHERE GroupID=@id";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление группы действий] ", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Удаление метода из группы
        /// </summary>
        /// <param name="actionID">Идентификатор метода</param>
        public void DeleteAction(int actionID)
        {

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_ActionToGroup WHERE ActionID=@action AND GroupID=@group", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@action", actionID);
                        cmd.Parameters.AddWithValue("@group", this.id);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление действия из группы] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление роли из группы
        /// </summary>
        /// <param name="roleID">Идентификатор роли</param>
        public void DeleteRole(int roleID)
        {
            string roleName = "";
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_name FROM groups WHERE group_id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", roleID);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            roleName = reader.GetString(0);
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление роли из справочника] ", ex);
                    }
                }
            }

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_RoleToActionGroup WHERE ActionGroup=@group AND Role=@roleName", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@roleName", roleName);
                        cmd.Parameters.AddWithValue("@group", this.id);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление роли из группы действий] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Получение списка всех групп
        /// </summary>
        /// <returns></returns>
        public static List<ActionGroup> GetAllgroups()
        {
            List<ActionGroup> result = new List<ActionGroup>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT id FROM Authorization_ActionGroups", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(new ActionGroup(reader.GetInt32(0)));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка групп действий] ", ex);
                    }
                }
            }
            return result;
        }
    }
}
