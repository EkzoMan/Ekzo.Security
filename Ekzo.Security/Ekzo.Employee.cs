using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace Ekzo.BaseClasses
{
    public class Employee : IEquatable<Employee>
    {
        /// <summary>
        /// Идентификатор сотрудника по БД 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ФИО сотрудника
        /// </summary>
        public string Name { get; set; }      
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="id">Идентификатор из БД </param>
        public Employee(int id)
        {
            InitClass(id);
        }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="employeeName">ФИО сотрудника</param>
        public Employee(string employeeName)
        {
           
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT employee_id FROM employee WHERE employee_name LIKE @employeeName+'%' AND date_fired IS NULL", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@employeeName", string.Join("%", employeeName.Split(char.Parse(" "))));
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                this.Id = reader.GetInt32(0);
                            }
                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            if (Ekzo.Web.Configuration.s_log != null)
                                Ekzo.Web.Configuration.s_log.Error("[Ошибка модуля авторизации] [Ошибка создания класса пользователя]", ex);
                        }
                    }
                }
            if (this.Id != 0)
                InitClass(this.Id);
        }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Employee() { }
        /// <summary>
        /// Инициализация полей класса
        /// </summary>
        /// <param name="id">Идентификатор сотрудника по БД </param>
        private void InitClass(int id)
        {
                this.Id = id;
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[Ekzo.Web.Configuration.UsersStringName].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT employee_name FROM employee WHERE employee_id=@employeeID AND date_fired IS NULL", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@employeeID", this.Id);
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                this.Name = reader.GetString(0);
                            }
                            reader.Close();

                        }
                        catch (Exception ex)
                        {
                            if (Ekzo.Web.Configuration.s_log != null)
                                Ekzo.Web.Configuration.s_log.Error("[Ошибка модуля авторизации] [Ошибка инициализации класса пользователя]", ex);
                        }
                    }
                }

        }

        public bool Equals(Employee x, Employee y)
        {
            if (x.Name == y.Name && x.Id == y.Id)
                return true;
            else
                return false;
        }
        public override int GetHashCode()
        {
            int hasEmployeeName = this.Name == null ? 0 : this.Name.GetHashCode();
            int hasID = this.Id == 0 ? 0 : this.Id.GetHashCode();

            return hasEmployeeName ^ hasID;
        }
        public bool Equals(Employee other)
        {
            if (this.Name == other.Name && this.Id == other.Id)
                return true;
            else
                return false;
        }

        bool IEquatable<Employee>.Equals(Employee other)
        {
            if (this.Name == other.Name && this.Id == other.Id)
                return true;
            else
                return false;
        }
    }
}