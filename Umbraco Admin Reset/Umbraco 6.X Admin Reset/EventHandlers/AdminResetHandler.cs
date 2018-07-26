using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco_6.X_Admin_Reset.Exceptions;

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
            adminUsername = GetValueFromConfigFile("UmbracoAdminResetUsername", adminUsername);
            adminPassword = GetValueFromConfigFile("UmbracoAdminResetPassword", adminPassword);

            // Check for password modification permission
            try
            {
                CheckForManualPasswordChange();
            } catch (MembershipProviderMissingException mpme)
            {
                LogHelper.Error(GetType(), mpme.Message, mpme);
                return;
            }

            // Update admin info
            if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminPassword))
            {
                try
                {
                    IUserService userService = ApplicationContext.Current.Services.UserService;
                    IUser adminUser = userService.GetUserById(0);

                    if (adminUser != null)
                    {
                        userService.SavePassword(adminUser, adminPassword);
                        adminUser.Username = adminUsername;
                        userService.Save(adminUser, false);
                    }
                    else
                    {
                        LogHelper.Warn(GetType(), "How can you not have a user with ID=0?!.");
                    }
                }
                catch (NotSupportedException nse)
                {
                    LogHelper.Error(GetType(), "Failed to save the admin's password.", nse);
                }
            }
            else
            {
                LogHelper.Warn(GetType(), "Something went wrong. Empty username or password");
            }

            // Delete .dll file
            DeleteMyDll();
        }


        private string GetValueFromConfigFile(string settingName, string currentValue)
        {
            string settingValue = string.Empty;

            try
            {
                settingValue = ConfigurationManager.AppSettings[settingName];

                if (!string.IsNullOrEmpty(settingValue))
                {
                    return settingValue;
                }
            } catch (ConfigurationErrorsException cee)
            {
                LogHelper.Error(GetType()
                                , string.Format("Failed reading \"{0}\" from the AppSettings section of the web.config file.", settingName)
                                , cee);
            }

            return currentValue;
        }


        private void CheckForManualPasswordChange()
        {
            try
            {
                //Create a new xml document
                XmlDocument document = new XmlDocument
                {
                    //Keep current indentions format
                    PreserveWhitespace = true
                };

                //Load the web.config file into the xml document
                string webconfigPath = HttpContext.Current.Server.MapPath("~/web.config");
                document.Load(webconfigPath);

                XmlNode userMembershipNode =
                    document.SelectSingleNode(
                        "//configuration/system.web/membership/providers/add[@name='UsersMembershipProvider']");

                if (userMembershipNode == null) {
                    throw new MembershipProviderMissingException("Couldn't find the UsersMembershipProvider tag in the web.config file.");
                }

                XmlAttribute att = userMembershipNode.Attributes["allowManuallyChangingPassword"];

                if (att == null)
                {
                    att = document.CreateAttribute("allowManuallyChangingPassword");
                    userMembershipNode.Attributes.Append(att);
                }
                if (!att.Value.Equals("true"))
                {
                    att.Value = "true";
                }

                document.Save(webconfigPath);
            }
            catch (MembershipProviderMissingException mpme) {
                throw mpme;
            }
            catch (Exception ex)
            {
                LogHelper.Error(GetType(), "Error while checking for \"allowManuallyChangingPassword\".", ex);
            }
        }


        private void DeleteMyDll()
        {
            string dllFilePath = HttpContext.Current.Server.MapPath("~/bin/Umbraco626AdminReset.dll");

            if (!string.IsNullOrEmpty(dllFilePath) && File.Exists(dllFilePath))
            {
                try
                {
                    File.Delete(dllFilePath);
                }catch (Exception exc)
                {
                    LogHelper.Error(GetType(), "I could not delete my DLL file.", exc);
                }                
            }

            return;
        }
    }
}