using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ekzo.Web.Security
{
    /// <summary>
    /// Аттрибут разграничения доступа к контролу
    /// </summary>
    public class ControlAccesSecurity
    {
        /// <summary>
        /// Проверка права доступа сотрудника к контролу
        /// </summary>
        /// <param name="controlName">Название контрола, описывающее его</param>
        /// <returns>Если пользователь имеет право доступа - возвращает true, иначе false</returns>
        public static bool HasControlAccess(string controlName)
        {
            BaseClasses.Employee employee = new BaseClasses.Employee(AuthLib.Helpers.RoleProviderHelper.GetUserId(HttpContext.Current));
            Utilization.Authorization.PageControl currentControl = new Utilization.Authorization.PageControl(controlName);
            foreach (Utilization.Authorization.PageControlsGroup group in currentControl.Groups)
                if (group.Roles.Where(o => Utilization.Authorization.SystemRole.GetEmployeeRoles(employee.Id).Contains(o.Role)).Count() != 0)
                    return true;

            return false;
        }
    }
}

namespace Ekzo.Web.Security.SecurityExtensions
{
    /// <summary>
    /// Расширения для базовых классов, обеспечивающие разграничение правд доступа
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Проверка права доступа сотрудника к контролу
        /// </summary>
        /// <param name="controlName">Название контрола, описывающее его</param>
        /// <returns>Если пользователь имеет право доступа - возвращает контро, иначе пустую строку</returns>
        public static MvcHtmlString HasControlAccess(this MvcHtmlString s, string controlName)
        {
            BaseClasses.Employee employee = new BaseClasses.Employee(AuthLib.Helpers.RoleProviderHelper.GetUserId(HttpContext.Current));
            Utilization.Authorization.PageControl currentControl = new Utilization.Authorization.PageControl(controlName);
            foreach (Utilization.Authorization.PageControlsGroup group in currentControl.Groups)
                if (group.Roles.Where(o => Utilization.Authorization.SystemRole.GetEmployeeRoles(employee.Id).Contains(o.Role)).Count() != 0)
                    return s;

            return MvcHtmlString.Create("");
        }

        /// <summary>
        /// Проверка права доступа сотрудника к контролу
        /// </summary>
        /// <param name="controlName">Название контрола, описывающее его</param>
        /// <returns>Если пользователь имеет право доступа - возвращает контро, иначе пустую строку</returns>
        public static IHtmlString HasControlAccess(this IHtmlString s, string controlName)
        {
            BaseClasses.Employee employee = new BaseClasses.Employee(AuthLib.Helpers.RoleProviderHelper.GetUserId(HttpContext.Current));
            Utilization.Authorization.PageControl currentControl = new Utilization.Authorization.PageControl(controlName);
            foreach (Utilization.Authorization.PageControlsGroup group in currentControl.Groups)
                if (group.Roles.Where(o => Utilization.Authorization.SystemRole.GetEmployeeRoles(employee.Id).Contains(o.Role)).Count() != 0)
                    return s;

            return MvcHtmlString.Create(string.Empty);
        }

        /// <summary>
        /// Проверка права доступа сотрудника к контролу
        /// </summary>
        /// <param name="controlName">Название контрола, описывающее его</param>
        /// <returns>Если пользователь имеет право доступа - возвращает контро, иначе пустую строку</returns>
        public static string HasControlAccess(this string s, string controlName)
        {
            BaseClasses.Employee employee = new BaseClasses.Employee(AuthLib.Helpers.RoleProviderHelper.GetUserId(HttpContext.Current));
            Utilization.Authorization.PageControl currentControl = new Utilization.Authorization.PageControl(controlName);
            foreach (Utilization.Authorization.PageControlsGroup group in currentControl.Groups)
                if (group.Roles.Where(o => Utilization.Authorization.SystemRole.GetEmployeeRoles(employee.Id).Contains(o.Role)).Count() != 0)
                    return s;

            return String.Empty;
        }

    }
    /// <summary>
    /// Расширения для TagBuilder'a обеспечивающие разграничение прав доступа
    /// </summary>
    public static class TagBuilderExtensions
    {
        /// <summary>
        /// Проверка права доступа сотрудника к контролу
        /// </summary>
        /// <param name="controlName">Название контрола, описывающее его</param>
        /// <returns>Если пользователь имеет право доступа - возвращает контро, иначе пустую строку</returns>
        public static  TagBuilder HasControlAccess(this TagBuilder s, string controlName)
        {
            BaseClasses.Employee employee = new BaseClasses.Employee(AuthLib.Helpers.RoleProviderHelper.GetUserId(HttpContext.Current));
            Utilization.Authorization.PageControl currentControl = new Utilization.Authorization.PageControl(controlName);
            foreach (Utilization.Authorization.PageControlsGroup group in currentControl.Groups)
                if (group.Roles.Where(o => Utilization.Authorization.SystemRole.GetEmployeeRoles(employee.Id).Contains(o.Role)).Count() != 0)
                    return s;

            return new TagBuilder("b");
        }
    }
}