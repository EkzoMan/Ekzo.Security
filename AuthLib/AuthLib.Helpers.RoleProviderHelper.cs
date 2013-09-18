using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthLib.Helpers
{
    public class RoleProviderHelper
    {
       private static string connString = AuthLib.Helpers.Settings.DomainUsersConnectionString;

       public static bool AddGroup(string intranetGroupName)
        {
            try
            {
                int groupID = 0;
                //Получаем идентификатор группы
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO groups(group_id,group_name) VALUES((SELECT MIN(group_id)-1 FROM groups WHERE id>-1000), @name)", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name", intranetGroupName);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Core.Logging.WriteEntry("Ошибка добавления группы.", ex, 1);
                return false;
            }
        }

        public static string[] GetGroupUsers(string intranetGroup)
        {
            List<string> result = new List<string>();
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"SELECT employee.employee_name FROM groups INNER JOIN employee2group ON 
                                                                                                        groups.group_id=employee2group.group_id INNER JOIN
                                                                                                        employee ON employee2group.employee_id=employee.employee_id
                                                                                                        WHERE group_name=@name ORDER BY employee.employee_name", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name",intranetGroup);
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while(reader.Read())
                                result.Add(reader.GetString(0));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return result.ToArray();
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка получения списка пользователей в роли.",ex);
                throw new Exception("Ошибка получения списка пользователей в роли.", ex);
            }
        }

        public static int GetUserId(System.Web.HttpContext HttpContext)
        {
            int result = 0;
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT employee_id FROM employee WHERE login LIKE @name", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name", HttpContext.User.Identity.Name.Remove(0,3));
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                result = reader.GetInt32(0);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка при получении идентификатора пользователя", ex);
                throw new Exception("Ошибка при получении идентификатора пользователя", ex);
            }
        }

        public static string[] GetGroups()
        {
            List<string> groups = new List<string>();
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT group_name from groups ORDER BY group_name", conn))
                    {
                        try
                        {
                            conn.Open();
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                groups.Add(reader.GetString(0));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return groups.ToArray();
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка при получении списка групп", ex);
                throw new Exception("Ошибка при получении списка групп",ex);
            }
        }

        public static string[] GetUserGroups(string user)
        {
            if (string.IsNullOrEmpty(user)) return new List<string>().ToArray();
            List<string> result = new List<string>();
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"SELECT groups.group_name FROM groups INNER JOIN employee2group ON 
                                                                                                        groups.group_id=employee2group.group_id INNER JOIN
                                                                                                        employee ON employee2group.employee_id=employee.employee_id
                                                                                                        WHERE employee.login LIKE @name
                                                                                                        ORDER BY group_name", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name", user.Remove(0,3));
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                result.Add(reader.GetString(0));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return result.ToArray();
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка получения списка групп для пользователя",ex);
                throw new Exception("Ошибка получения списка групп для пользователя", ex);
            }
        }

        public static bool IsUserInGroup(string user,string group)
        {
            bool result = false;
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(@"SELECT COUNT(*) FROM employee2group INNER JOIN employee ON 
                                                                                                        employee2group.employee_id = employee.employee_id INNER JOIN 
                                                                                                        groups ON employee2group.group_id=groups.group_id
                                                                                                        WHERE 
                                                                                                        employee.login LIKE @name AND groups.group_name LIKE @group", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name", user.Remove(0, 3));
                            cmd.Parameters.AddWithValue("@group", group);
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                result = reader.GetInt32(0) != 0;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка определения вхождения пользователя в группу.", ex);
                throw new Exception("Ошибка определения вхождения пользователя в группу.", ex);
            }
        }

        public static bool GroupExists(string groupName)
        {
            bool result = false;
            try
            {
                
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM groups WHERE group_name LIKE @name", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@name", groupName);
                            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                result = reader.GetInt32(0) != 0;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                AuthLib.Core.Logging.WriteEntry("Ошибка определения существования группы.", ex);
                throw new Exception("Ошибка определения существования группы.", ex);
            }
        }

    }
}
