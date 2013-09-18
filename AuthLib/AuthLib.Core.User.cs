using System;
using System.Collections.Generic;

namespace AuthLib.Core
{
    public class User
    {
        public string Name { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public string Description { get; private set; }
        public bool isRICUser { get; private set; }
        public string EMail { get; private set; }
        public List<KeyValuePair<string, bool>> Rights { get; private set; }
        public int? id { get; private set; }

        public User(){}

        public User(string Name, string Login, string Password, string Email, List<KeyValuePair<string, bool>> _rights=null, string Description = null, bool isRICUser = false,int? id=null)
        {
            this.id = id;
            this.Name = Name;
            this.Login = Login;
            this.Password = Password;
            this.EMail = Email;
            this.isRICUser = isRICUser;
            this.Description = Description;
            this.Rights = _rights;
        }
    }

    #region intefraces
    public interface IUser
    {
        int id { get; set; }
        string Name { get; set; }
        string Email { get; set; }
        Credentials Credentials { get; set; }
        
    }
    #endregion

    #region HelperSubClasses
    public class Credentials
    {
        private string password = "";
        public string Login { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Password { get {
            if (string.IsNullOrEmpty(password))
                throw new NotImplementedException("Свойство не поддерживается для данного пользователя.");
            return this.password;
        }
            private set {
                this.password = value;
            }
        }
        public Credentials() { }

        public Credentials(string login, string password)
        {
            this.Login = login;
            this.password = password;
        }
    }
    #endregion

    #region MainClasses
    /// <summary>
    /// Сотрудник РИЦ
    /// </summary>
    public class DomainUser : IUser
    {
        /// <summary>
        /// Идентификатор пользователя в справочнике сотрудника
        /// </summary>
        public int id
        {
            get;
            set;
        }
        /// <summary>
        /// ФИО сотрудника
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Адрес электронной почты сотрудника
        /// </summary>
        public string Email
        {
            get;
            set;
        }
        /// <summary>
        /// Описание сотрудника
        /// </summary>
        public string Description { get; 
            private set; }
        public Credentials Credentials
        {
            get;
            set;
        }

        public DomainUser() { }
        public DomainUser(int id) {
            InitClass(id, null);
        }
        public DomainUser(string login)
        {
            InitClass(null, login);
        }


        private void InitClass(int? id, string login)
        {
            if (id == null && string.IsNullOrEmpty(login)) throw new Exception("Невозможно инициализировать пользователя без предоставления хотя бы одного из ключевых полей.");
            try
            {
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Helpers.Settings.DomainUsersConnectionString))
                {
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT [employee_id],[employee_name],[email],[login],[employee_description] FROM [].[dbo].[employee] WHERE employee_id=@id OR login=@login", conn))
                    {
                        try
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@id", id == null ? 0 : id);
                            cmd.Parameters.AddWithValue("@login", string.IsNullOrEmpty(login) ? "" :
                                login.Contains(@"\") ? 
                                      login.Split('\\')[1] :
                                      login);
                            using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    this.id = reader.GetInt32(0);
                                    this.Name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                    this.Email = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                    this.Credentials = new Credentials(reader.GetString(3), null);
                                    this.Description = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
    #endregion

}