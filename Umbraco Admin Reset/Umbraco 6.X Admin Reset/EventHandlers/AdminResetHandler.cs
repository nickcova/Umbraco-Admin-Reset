using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco_6.X_Admin_Reset.EventHandlers
{
    /// <summary>
    /// A class to reset the admin user's information.
    /// </summary>
    /// <remarks>This class is based on Richard Soeteman's Umbraco
    /// Admin Reset Github project.</remarks>
    public class AdminResetHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            string adminUsername = "admin";
            string adminPassword = "admin12345";

            // Check for settings in the web.config file
            adminUsername = GetValueFromConfigFile("newAdminUsername");
            adminPassword = GetValueFromConfigFile("newAdminPassword");

            // Check for user modification permissions

            // Update admin info
            if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminPassword))
            {
                try {
                    IUserService userService = ApplicationContext.Current.Services.UserService;
                    IUser adminUser = userService.GetUserById(0);
                    userService.SavePassword(adminUser, adminPassword);
                    adminUser.Username = adminUsername;
                    userService.Save(adminUser, false);
                } catch (NotSupportedException nse)
                {
                    LogHelper.Error(GetType(), "Failed to save the admin's password.", nse);
                }                
            } else 
            {
                LogHelper.Warn(GetType(), "Something went wrong. Empty username or password");
            }

            // Delete .dll file
        }

        private string GetValueFromConfigFile(string settingName)
        {
            throw new NotImplementedException();
        }
    }
}