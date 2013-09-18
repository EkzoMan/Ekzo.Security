using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Ekzo.Web.Security.Utilization.Authorization
{

    /// <summary>
    /// Класс описывающий метод. Используется для определения соответствий имени действия и роли, имеющей доступ к нему.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Идентификатор по БД
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// Группы, к которым относится метод
        /// </summary>
        private int[] _ActionGroups;
        /// <summary>
        /// Группы, к которым относится метод
        /// </summary>
        public ActionGroup[] ActionGroups
        {
            get
            {
                List<ActionGroup> Groups = new List<ActionGroup>();
                if (_ActionGroups != null)
                {
                    for (int i = 0; i < _ActionGroups.Count(); i++)
                        Groups.Add(new ActionGroup(_ActionGroups[i]));
                    return Groups.ToArray();
                }
                return null;
            }
        }
        /// <summary>
        /// Имя метода в системе разграничения достука
        /// </summary>
        public string ActionName { get; set; }
        /// <summary>
        /// Флаг индикатор активности правила
        /// </summary>
        public bool Active { get; private set; }

        #region ClassBuilder
        public Action() { }
        public Action(string ActionName)
        {
            InitClass(ActionName);
        }
        public Action(int id)
        {
            InitClass(null, id);
        }
        private void InitClass(string name, int id = 0, int actionGroup = 0)
        {
            if (id != 0) this.id = id;
            if (!string.IsNullOrEmpty(name)) this.ActionName = name;
            //this._ActionGroups = actionGroup;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM Authorization_Actions WHERE id=@id OR Name=@actionName", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@actionName", string.IsNullOrEmpty(name) ? "" : name);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.ActionName = reader.GetString(1);
                            this.Active = reader.GetBoolean(2);
                        }
                        reader.Close();
                        List<int> actionGroups = new List<int>();
                        cmd.CommandText = "SELECT GroupID FROM Authorization_ActionToGroup WHERE ActionID=@id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                            actionGroups.Add(reader.GetInt32(0));

                        this._ActionGroups = actionGroups.ToArray();
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
        /// Сохранение метода
        /// </summary>
        public void Save()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"
                        IF((SELECT COUNT(*) FROM Authorization_Actions WHERE id=@id OR Name=@name)=0) 
                        BEGIN
                            INSERT INTO Authorization_Actions(Name) VALUES(@name)
                            INSERT INTO Authorization_ActionToGroup(ActionID,GroupID) VALUES((SELECT TOP(1) id FROM Authorization_Actions WHERE Name=@name),0) 
                        END
                        ELSE 
                            UPDATE Authorization_Actions SET Name=@name, Active=@active 
                        
                        SELECT * FROM Authorization_Actions WHERE Name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        if (!string.IsNullOrEmpty(this.ActionName) && this.id == 0) cmd.CommandText = cmd.CommandText.Replace("WHERE id=@id OR Name=@name", "WHERE Name=@name");
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.Parameters.AddWithValue("@name", this.ActionName);
                        cmd.Parameters.AddWithValue("@active", this.Active);
                        //cmd.Parameters.AddWithValue("@groupID", this._ActionGroup);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        List<int> actionGroups = new List<int>();
                        while (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                            this.ActionName = reader.GetString(1);
                            this.Active = reader.GetBoolean(2);
                        }
                        reader.Close();
                        cmd.CommandText = "SELECT GroupID FROM Authorization_ActionToGroup WHERE ActionID=@id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                            actionGroups.Add(reader.GetInt32(0));

                        this._ActionGroups = actionGroups.ToArray();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Сохранение действия в БД] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Удаление метода
        /// </summary>
        public void Delete()
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM Authorization_Actions WHERE id=@id", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", this.id);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM Autorization_ActionToGroup WHERE ActionID=@id";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Удаление действия из БД] ", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Проверка на существование метода
        /// </summary>
        /// <returns></returns>
        public bool IsExist()
        {
            return IsExist(this.ActionName);
        }
        /// <summary>
        /// Проверка существования метода
        /// </summary>
        /// <param name="ActionName">Имя метода</param>
        /// <returns></returns>
        public static bool IsExist(string ActionName)
        {
            bool result = false;
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM Authorization_Actions WHERE Name=@name", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@name", ActionName);
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result = reader.GetInt32(0) == 0 ? false : true;
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Проверка существования действия в БД] ", ex);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получение списка всех доступных методов
        /// </summary>
        /// <returns></returns>
        public static List<Action> GetAllActions()
        {
            List<Action> result = new List<Action>();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT id FROM Authorization_Actions ORDER BY Name ", conn))
                {
                    try
                    {
                        conn.Open();
                        System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            result.Add(new Action(reader.GetInt32(0)));
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Получение всех действий] ", ex);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Добавление метода в группу
        /// </summary>
        /// <param name="groupID"></param>
        public void AddToGroup(int groupID)
        {

            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString))
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"
                        IF((SELECT COUNT(*) FROM Authorization_ActionToGroup WHERE ActionID=@action AND GroupID=@group)=0)
                           INSERT INTO Authorization_ActionToGroup(ActionID,GroupID) VALUES(@action,@group)", conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@action", groupID);
                        cmd.Parameters.AddWithValue("@group", this.id);
                        cmd.ExecuteNonQuery();
                        new Action(this.id);
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Добавление действия в группу] ", ex);
                    }
                }
            }
        }
    }
}