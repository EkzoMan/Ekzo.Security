using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using log4net;

namespace Ekzo.Web
{
    public static class Configuration
    {
        /// <summary>
        /// Название строки подключения к БД в файле web.config, указывающей на базу с таблицами авторизации.
        /// Поумолчанию DataSource
        /// </summary>
        public static string ConnectionStringName = "DataSource";
        /// <summary>
        /// Название строки подключения к БД в файле web.config, указывающей на базу 
        /// Поумолчанию 
        /// </summary>
        public static string UsersStringName = "";
        /// <summary>
        /// Название проекта
        /// </summary>
        public static string ProjectName = "Project Name";

        /// <summary>
        /// Объект лога log4net
        /// </summary>
        public static ILog s_log = null;

        /// <summary>
        /// Список таблиц системы безопасности
        /// </summary>
        private static string[] tables = { "Authorization_ActionGroups", 
                                           "Authorization_Actions",
                                           "Authorization_ActionToGroup",
                                           "Authorization_Controls",
                                           "Authorization_ControlsGroup",
                                           "Authorization_ControlToGroup",
                                           "Authorization_RoleToActionGroup",
                                           "Authorization_RoleToControlGroup"};

        /// <summary>
        /// Проверка на существование всех таблиц в БД
        /// </summary>
        /// <returns>Если хотя бы одна таблица не найдена в БД, значение становится ложным</returns>
        public static bool BaseHasTables()
        {
            List<string> dbTables = new List<string>();
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select TABLE_NAME from information_schema.tables WHERE TABLE_NAME LIKE 'Authorization_%'", conn))
                {
                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                dbTables.Add(reader.GetString(0));
                        }
                        foreach (string securityTable in tables)
                            if (dbTables.Contains(securityTable))
                                dbTables.Remove(securityTable);
                    }
                    catch (Exception ex)
                    {
                        Configuration.s_log.Error("[Ошибка модуля авторизации] [Проверка таблиц безопасности] ", ex);
                    }
                }
            }
            if (dbTables.Count == 0) return false; else return true;
        }
        /// <summary>
        /// Выполняет создание таблиц безопасности в БД
        /// </summary>
        /// <param name="superAdminGroup">Имя группы в справочнике, пользователи которой будут назначены суперадминистраторами системы</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static void CreateSecurityTables(string superAdminGroup = null)
        {
            List<string> dbTables = new List<string>();
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT TABLE_NAME FROM information_schema.tables WHERE TABLE_NAME LIKE 'Authorization_%'", conn))
                {
                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                dbTables.Add(reader.GetString(0));
                        }
                        foreach (string securityTable in tables)
                            if (dbTables.Contains(securityTable))
                                dbTables.Remove(securityTable);

                        cmd.Parameters.AddWithValue("@database", conn.Database);

                        if (dbTables.Count == 0)
                            foreach (string table in tables)
                                dbTables.Add(table);

                        if (dbTables.Count != 0) 
                            foreach (string table in dbTables)
                            {
                                switch (table)
                                {
                                    case "Authorization_ControlsGroup":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                           SET ANSI_NULLS ON
                                                           SET QUOTED_IDENTIFIER ON
                                                           SET ANSI_PADDING ON
                                                           CREATE TABLE [dbo].[Authorization_ControlsGroup](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [Name] [varchar](500) NOT NULL,
                                                             CONSTRAINT [PK_Authorization_ControlsGroup] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]
                                                            SET ANSI_PADDING OFF
                                                            SET IDENTITY_INSERT [dbo].[Authorization_ControlsGroup] ON
                                                            INSERT INTO [dbo].[Authorization_ControlsGroup](id,Name) VALUES(-1,'Контролы суперадмина')
                                                            SET IDENTITY_INSERT [dbo].[Authorization_ControlsGroup] OFF";
                                        break;
                                    case "Authorization_Actions":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                            SET ANSI_NULLS ON
                                                            SET QUOTED_IDENTIFIER ON
                                                            SET ANSI_PADDING ON
                                                            CREATE TABLE [dbo].[Authorization_Actions](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [Name] [varchar](500) NOT NULL,
	                                                            [Active] [bit] NOT NULL,
                                                             CONSTRAINT [PK_Authorization_Actions] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]
                                                            SET ANSI_PADDING OFF
                                                            ALTER TABLE [dbo].[Authorization_Actions] ADD  CONSTRAINT [DF_Authorization_Actions_Active]  DEFAULT ((1)) FOR [Active]";
                                        break;
                                    case "Authorization_ActionToGroup":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                                SET ANSI_NULLS ON
                                                                SET QUOTED_IDENTIFIER ON
                                                                CREATE TABLE [dbo].[Authorization_ActionToGroup](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [ActionID] [int] NOT NULL,
	                                                            [GroupID] [int] NOT NULL,
                                                             CONSTRAINT [PK_Authorization_ActionToGroup] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]";
                                        break;
                                    case "Authorization_Controls":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                                SET ANSI_NULLS ON
                                                                SET QUOTED_IDENTIFIER ON
                                                                SET ANSI_PADDING ON
                                                                CREATE TABLE [dbo].[Authorization_Controls](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [Name] [varchar](500) NOT NULL,
                                                             CONSTRAINT [PK_Authorize_Controls] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]
                                                            SET ANSI_PADDING OFF";
                                        break;
                                    case "Authorization_ControlToGroup":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                               SET ANSI_NULLS ON
                                                               SET QUOTED_IDENTIFIER ON
                                                               CREATE TABLE [dbo].[Authorization_ControlToGroup](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [ControlID] [int] NOT NULL,
	                                                            [GroupID] [int] NOT NULL,
                                                             CONSTRAINT [PK_Authorization_ControlToGroup] PRIMARY KEY CLUSTERED 
                                                            (	[id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]";
                                        break;
                                    case "Authorization_RoleToActionGroup":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                             SET ANSI_NULLS ON
                                                             SET QUOTED_IDENTIFIER ON
                                                             SET ANSI_PADDING ON
                                                             CREATE TABLE [dbo].[Authorization_RoleToActionGroup](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [ActionGroup] [int] NOT NULL,
	                                                            [Role] [varchar](500) NOT NULL,
                                                             CONSTRAINT [PK_Authorization_RoleToActionGroup] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]
                                                            SET ANSI_PADDING OFF
                                                            ALTER TABLE [dbo].[Authorization_RoleToActionGroup] ADD  CONSTRAINT [DF_Authorization_RoleToActionGroup_ActionGroup]  DEFAULT ((-1)) FOR [ActionGroup]";
                                        break;
                                    case "Authorization_RoleToControlGroup":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                             SET ANSI_NULLS ON
                                                             SET QUOTED_IDENTIFIER ON
                                                             CREATE TABLE [dbo].[Authorization_RoleToControlGroup](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [RoleID] [int] NOT NULL,
	                                                            [GroupID] [int] NOT NULL,
                                                             CONSTRAINT [PK_Authorization_RoleToControlGroup] PRIMARY KEY CLUSTERED 
                                                            ([id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]";
                                        break;
                                    case "Authorization_ActionGroups":
                                        cmd.CommandText = "USE " + conn.Database + @"
                                                               SET ANSI_NULLS ON
                                                               SET QUOTED_IDENTIFIER ON
                                                               SET ANSI_PADDING ON
                                                               CREATE TABLE [dbo].[Authorization_ActionGroups](
	                                                            [id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [Name] [varchar](500) NOT NULL,
	                                                            [active] [bit] NOT NULL,
                                                             CONSTRAINT [PK_Authorization_ActionGroups] PRIMARY KEY CLUSTERED 
                                                            (
	                                                            [id] ASC
                                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                                            ) ON [PRIMARY]
                                                               SET ANSI_PADDING OFF
                                                               ALTER TABLE [dbo].[Authorization_ActionGroups] ADD  CONSTRAINT [DF_Authorization_ActionGroups_active]  DEFAULT ((1)) FOR [active]
                                                               SET IDENTITY_INSERT [dbo].[Authorization_ActionGroups] ON
                                                               INSERT INTO [dbo].[Authorization_ActionGroups](id,Name) VALUES(0,'Все разделы')
                                                               SET IDENTITY_INSERT [dbo].[Authorization_ActionGroups] OFF";
                                        break;
                                }
                                cmd.ExecuteNonQuery();
                            }
                        if (!string.IsNullOrEmpty(superAdminGroup) && Web.Security.Utilization.Authorization.SystemRole.IntranetRoleID(superAdminGroup) != null)
                        {
                            cmd.CommandText = @"INSERT INTO Authorization_RoleToActionGroup(ActionGroup,Role) VALUES(0,@group)
                                                INSERT INTO Authorization_RoleToControlGroup(RoleID,GroupID) VALUES(@role,-1)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@group", superAdminGroup);
                            cmd.Parameters.AddWithValue("@role", Web.Security.Utilization.Authorization.SystemRole.IntranetRoleID(superAdminGroup));
                            cmd.ExecuteNonQuery();
                        }
                            
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка модуля авторизации] [Создание таблиц безопасности] ", ex);
                    }
                }
            }
        }

        public static void RecreateTables(string superAdminGroup = null)
        {
            string command = "DROP TABLE {0}";
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();
                        foreach (string table in tables)
                        {
                            cmd.CommandText = string.Format(command, table);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Configuration.s_log != null)
                            Configuration.s_log.Error("[Ошибка молудя авторизации] [Ошибка пересоздания таблиц безопасности] ", ex);
                    }
                }
            }
            CreateSecurityTables(superAdminGroup);
        }

    }
}