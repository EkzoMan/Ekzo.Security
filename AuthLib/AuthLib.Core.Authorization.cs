using System;
using System.Linq;
using System.Collections.Generic;

namespace AuthLib.Core.Authorization
{
   public static class Helper
	{
        public static AuthLib.Core.IUser AuthorizeUser(System.Web.HttpContext context)
        {
            try
            {
                //Если не пришел заголовок авторизации, читаем другие заголовки
                    return new DomainUser(context.Request.ServerVariables["AUTH_USER"]);
            }
            catch (Exception ex)
            {
                Core.Logging.WriteEntry(ex.Message + Environment.NewLine + ex.StackTrace);
                return null;
            }
        }
	}

}
