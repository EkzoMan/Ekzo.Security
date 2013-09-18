using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ekzo.Web.Security.Utilization
{
    /// <summary>
    /// Атрибут обеспечивающий разграничение прав доступа к методам контроллеров
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class ActionAuthorization : AuthorizeAttribute
    {
        /// <summary>
        /// Имя метода контроллера
        /// </summary>
        public string ActionName { get; set; }
        /// <summary>
        /// Выполняет проверку на право доступа к методу
        /// </summary>
        /// <param name="httpContext">Контекст</param>
        /// <returns>Если право есть, выполняет метод. Иначе перенаправляет пользователя на страницу ошибки.</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool result = false;
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized) return false;
            Authorization.Action currentAction = new Authorization.Action(this.ActionName);

            //Если действие не добавлено в БД, сохраняем.
            if (!currentAction.IsExist()) currentAction.Save();

            //Если действие деактивировано, пускаем всех.
            if (!currentAction.Active) return true;

            string[] currentUserRoles = AuthLib.Helpers.RoleProviderHelper.GetUserGroups(httpContext.User.Identity.Name);
            foreach (string role in currentUserRoles)
            {
                if (currentAction.ActionGroups != null && currentAction.ActionGroups.Where(o => o.Roles.Select(n => n.Role).Contains(role)).Count() != 0) { result = true; break; }
            }
            
            return result;
        }
    }
}