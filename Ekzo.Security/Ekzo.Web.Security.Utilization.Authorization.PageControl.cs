using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ekzo.Web.Security.Utilization.Authorization
{
    /// <summary>
    /// Класс описывает контролы, которые имеют разграничение доступа
    /// </summary>
    public class PageControl
    {
        /// <summary>
        /// Идентификатор контрола по БД разграничения прав
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Название контрола в БД
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Группы, к которым привязан контрол
        /// </summary>
        public List<PageControlsGroup> Groups { get; set; }

        #region ClassBuilder
        public PageControl(int id)
        {
            InitClass(id, null);
        }
        public PageControl(string name)
        {
            InitClass(0, name);
        }
        private void InitClass(int id, string Name)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                //Если контрол еще не занесен в БД, добавляем его в список
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"IF((SELECT COUNT(*) FROM Authorization_Controls WHERE Name=@name OR id=@id)=0) 
                                                                                                                            INSERT INTO Authorization_Controls(Name) VALUES (@name)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", Name == null ? "" : Name);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT id,Name FROM Authorization_Controls WHERE Name=@name OR id=@id";
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.Name = reader.GetString(1);
                        }
                        reader.Close();
                        //Если контрол еще не добавлен в группу контролов суперадмина, добавляем
                        //Группа суперадмина -1
                        cmd.CommandText = "IF((SELECT COUNT(*) FROM Authorization_ControlToGroup WHERE ControlID=@id AND GroupID=-1)=0) INSERT INTO Authorization_ControlToGroup(ControlID,GroupID) VALUES(@id,-1)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация класса элемента управления] ", ex);
                    }
                }
            }

            this.Groups = new List<PageControlsGroup>();
            List<int> groupsIDs = new List<int>();

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT GroupID FROM Authorization_ControlToGroup WHERE ControlID=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            groupsIDs.Add(reader.GetInt32(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Инициализация класса элемента управления] ", ex);
                    }
                }
            }

            for (int i = 0; i < groupsIDs.Count; i++)
                this.Groups.Add(new PageControlsGroup(groupsIDs[i]));

        }
        #endregion

        /// <summary>
        /// Добавление контрола в группу
        /// </summary>
        /// <param name="groupID"></param>
        public void AddToGroup(int groupID)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"IF((SELECT COUNT(*) FROM Authorization_ControlToGroup WHERE ControlID=@id AND GroupID=@group)=0) 
                                                                                                           INSERT INTO Authorization_ControlToGroup(ControlID,GroupID) VALUES(@id,@group)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@group", groupID);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Добавление элемента управления в группу] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Сохранение контрола
        /// </summary>
        public void Save()
        {
            string commandText = this.id == 0 ? "INSERT INTO Authorization_Controls (Name) VALUES(@name)" : "UPDATE Authorization_Controls SET Name=@name WHERE id=@id";
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(commandText, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "IF((SELECT COUNT(*) FROM Authorization_ControlToGroup WHERE ControlID=(SELECT TOP(1) id FROM Authorization_Controls WHERE Name=@name) AND GroupID=-1)=0) INSERT INTO Authorization_ControlToGroup(ControlID,GroupID) VALUES(SELECT TOP(1) id FROM Authorization_Controls WHERE Name=@name,-1)";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение элемента управления] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление контрола
        /// </summary>
        public void Delete()
        {
            if (this.id == 0) return;

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_Controls WHERE id=@id", conn))
                {
                    try
                    {
                        conn.Open();
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
        /// Получение списка всех контролов
        /// </summary>
        /// <returns></returns>
        public static PageControl[] GetAllControls()
        {
            List<PageControl> result = new List<PageControl>();
            List<int> controlsIDs = new List<int>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT id FROM Authorization_Controls ORDER BY Name", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            controlsIDs.Add(reader.GetInt32(0));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение списка элементов управления] ", ex);
                    }
                }
            }
            for (int i = 0; i < controlsIDs.Count; i++)
                result.Add(new PageControl(controlsIDs[i]));
            return result.ToArray();
        }

    }
}
