using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ekzo.Web.Security.Utilization.Authorization
{
    /// <summary>
    /// Класс описывает привязку ролей к группе контролов
    /// </summary>
    public class ControlsGroupRole
    {
        /// <summary>
        ///  Идентификатор роли по БД 
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Имя роли
        /// </summary>
        public string Role
        {
            get
            {
                string result = null;
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_name FROM groups WHERE group_id=@id", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@id", this.RoleID);
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                result = reader.GetString(0);
                        }
                        catch (Exception ex)
                        {
                            if (Configuration.s_log != null)
                                Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение названия группы для роли] ", ex);
                        }
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// Идентификатор роли
        /// </summary>
        public int RoleID { get; set; }
        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public int GroupID { get; set; }

        public ControlsGroupRole(int id, int groupID)
        {
            this.id = id;
            this.GroupID = groupID;
            InitClass(id, groupID);
        }

        private void InitClass(int id, int groupID)
        {
            this.RoleID = id;
            this.GroupID = groupID;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_RoleToControlGroup WHERE RoleID=@role AND GroupID=@group", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@role", id);
                        cmd.Parameters.AddWithValue("@group", groupID);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            this.id = reader.GetInt32(0);
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация класса роли группы элементов управления] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Сохранение привязки роли к группе
        /// </summary>
        public void Save()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"IF((SELECT COUNT(*) FROM Authorization_RoleToControlGroup WHERE RoleID=@id AND GroupID=@group)=0)
                                                                                                         INSERT INTO Authorization_RoleToControlGroup(RoleID,GroupID) VALUES(@id,@group)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@group", this.GroupID);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение роли группы элементов управления] ", ex);
                    }
                }
            }
        }


    }
}
