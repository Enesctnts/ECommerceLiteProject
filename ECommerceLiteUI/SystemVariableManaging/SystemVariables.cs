using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
namespace ECommerceLiteUI.SystemVariableManaging
{
    public static class SystemVariables
    {
        public static string EMAIL
        {
            get
            {
                try
                {
                    //return System.Configuration.ConfigurationManager.AppSettings["ECommerceLiteEmail"].ToString();
                    return ConfigurationManager.AppSettings["ECommerceLiteEmail"].ToString();
                }
                catch (Exception ex)
                {

                    throw new Exception("Webconfig dosyasında email bilgisi bulunamadı!");
                }
            }
        }
    }
} 