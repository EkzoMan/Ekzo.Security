using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ekzo.Web.Security.Utilization.Authorization
{
    /// <summary>
    /// Класс описывает группу контролов
    /// Группа контролов суперадмина должна иметь идентификатор -1
    /// </summary>
    public class PageControlsGroup : IEquatable<PageControlsGroup>, IEqualityComparer<PageControlsGroup>
    {
        /// <summary>
        /// Идентификатор группы по БД разграничения  правд
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Имя группы
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Список ролей, принадлежащих группе
        /// </summary>
        public List<ControlsGroupRole> Roles { get; set; }

        #region ClassBuilder
        public PageControlsGroup(int id)
        {
            InitClass(id, "");
        }
        public PageControlsGroup(string Name)
        {
            this.Name = Name;
            InitClass(0, Name);
        }
        public PageControlsGroup(int id, string Name)
        {
            this.id = id;
            this.Name = Name;
            this.Roles = new PageControlsGroup(id).Roles;
        }

        private void InitClass(int id, string name)
        {
            //this.Controls = new List<PageControl>();
            this.Roles = new List<ControlsGroupRole>();
            List<int> pageControls = new List<int>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_ControlsGroup WHERE id=@id OR Name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", name);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.Name = reader.GetString(1);
                        }
                        reader.Close();
                        cmd.CommandText = @"SELECT Authorization_Controls.id AS ControlID
                                            FROM   Authorization_ControlToGroup INNER JOIN
                                            Authorization_ControlsGroup ON Authorization_ControlToGroup.GroupID = Authorization_ControlsGroup.id INNER JOIN
                                            Authorization_Controls ON Authorization_ControlToGroup.ControlID = Authorization_Controls.id
                                            WHERE Authorization_ControlsGroup.id=@id OR Authorization_ControlsGroup.Name=@name";
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                            pageControls.Add(reader.GetInt32(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация класса группы элементов управления] ", ex);
                    }
                }
            }


            //for(int i=0;i<pageControls.Count;i++)
            //    this.Controls.Add(new PageControl(pageControls[i]));

            List<int> groupRoles = new List<int>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT RoleID FROM Authorization_RoleToControlGroup WHERE GroupID=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            groupRoles.Add(reader.GetInt32(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка ролей группы элементов управления] ", ex);
                    }
                }
            }
            for (int i = 0; i < groupRoles.Count; i++)
                this.Roles.Add(new ControlsGroupRole(groupRoles[i], this.id));
        }
        #endregion

        /// <summary>
        /// Получени списка всех групп
        /// </summary>
        /// <returns></returns>
        public static PageControlsGroup[] GetAllGroups()
        {
            List<PageControlsGroup> result = new List<PageControlsGroup>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_ControlsGroup", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(new PageControlsGroup(reader.GetInt32(0), reader.GetString(1)));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка групп элементов управления] ", ex);
                    }
                }
            }

            return result.ToArray();
        }
        /// <summary>
        /// Сохранение группы
        /// </summary>
        public void Save()
        {

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("IF((SELECT COUNT(*) FROM Authorization_ControlsGroup WHERE Name=@name)=0) INSERT INTO Authorization_ControlsGroup(Name) VALUES(@name)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение группы элементов управления] ", ex);
                    }
                }
            }

            PageControlsGroup newGroup = new PageControlsGroup(this.Name);
            this.id = newGroup.id;
            this.Roles = newGroup.Roles;
            newGroup = null;

        }
        /// <summary>
        /// Удаление группы
        /// </summary>
        public void Delete()
        {
            if (this.id != 0)
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_ControlsGroup WHERE id=@id", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@id", this.id);
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "DELETE FROM Authorization_RoleToControlGroup WHERE GroupID=@id";
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            if (Configuration.s_log != null)
                                Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление группы элементов управления] ", ex);
                        }
                    }
                }
        }
        /// <summary>
        /// Удаление контрола из группы
        /// </summary>
        /// <param name="controlID">Идентификатор контрола</param>
        public void DeleteControl(int controlID)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_ControlToGroup WHERE ControlID=@control AND GroupID=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@control", controlID);
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление элемента управления] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление роли
        /// </summary>
        /// <param name="roleID">Идентификатор роли</param>
        public void DeleteRole(int roleID)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_RoleToControlGroup WHERE RoleID=@role AND GroupID=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@role", roleID);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление роли элемента управления] ", ex);
                    }
                }
            }
        }
        #region InterfaceImplementation
        bool IEquatable<PageControlsGroup>.Equals(PageControlsGroup other)
        {
            return this.id == other.id & this.Name == other.Name;
        }
        public bool Equals(PageControlsGroup x, PageControlsGroup y)
        {
            return x.id == y.id & x.Name == y.Name;
        }
        public int GetHashCode(PageControlsGroup obj)
        {
            return this.id.GetHashCode() + this.Name.GetHashCode() + this.Roles.GetHashCode();
        }
        #endregion
    }
}
