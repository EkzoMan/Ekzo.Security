using System;
using log4net;

namespace AuthLib.Core
{
    public class Logging
    {
        //public static readonly log4net.ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly ILog log = LogManager.GetLogger(typeof(AuthLib.Core.Authorization.RoleProvider));

        public static void WriteEntry(string message, Exception InnerException=null, int logLevel = 1)
        {
            using (System.IO.StreamWriter sWrite = new System.IO.StreamWriter(@"C:\Log\ConsAuthLib.log", true))
                sWrite.WriteLine(message);
        }
    }
}
