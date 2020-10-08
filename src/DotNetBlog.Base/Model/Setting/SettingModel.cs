using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

namespace DotNetBlog.Model.Setting
{
    public class SettingModel
    {

        #region Default(s)

        public const string DefaultHost = "http://dotnetblog.com";
        public const string DefaultTitle = "DotNetBlog";
        public const string DefaultTheme = "default";
        public const int DefaultTopicPerPage = 10;
        public const bool DefaultOnlyShowSummary = false;
        public const string DefaultSmtpEmailAddress = "name@example.com";
        public const string DefaultSmtpServer = "mail.example.com";
        public const string DefaultSmtpUser = "username";
        public const string DefaultSmtpPassword = "password";
        public const int DefaultSmtpPort = 25;
        public const bool DefaultSmtpEnableSSL = false;
        public const bool DefaultSendEmailWhenComment = true;
        public const bool DefaultAllowComment = true;
        public const bool DefaultVerifyComment = true;
        public const bool DefaultTrustAuthenticatedCommentUser = true;
        public const bool DefaultEnableCommentWebSite = true;
        public const int DefaultCloseCommentDays = 0;
        public const string DefaultErrorPageTitle = "Sorry the system has made an error";
        public const string DefaultErrorPageContent = "Request page is wrong please try again later";
        public const string DefaultHeaderScript = "";
        public const string DefaultFooterScript = "";
        public const bool DefaultRegistration = false;

        #endregion

        #region Properties and Contractor

        private Version _currentVersion;

        public Dictionary<string, string> Settings { get; private set; }

        internal IStringLocalizer<SettingModel> L { get; set; }

        public SettingModel(Dictionary<string, string> settings, IStringLocalizer<SettingModel> localizer)
        {
            Settings = settings;
            L = localizer;
        }

        public Version ApplicationVersion =>
            _currentVersion ??= GetType().Assembly.GetName().Version;

        /// <summary>
        /// URL address of the blog
        /// </summary>
        public string Host
        {
            get
            {
                return GetStringValue(nameof(Host), L[DefaultHost].Value);
            }
            set
            {
                SetValue(nameof(Host), value);
            }
        }

        /// <summary>
        /// Site name
        /// </summary>
        public string Title
        {
            get
            {
                return GetStringValue(nameof(Title), L[DefaultTitle].Value);
            }
            set
            {
                SetValue(nameof(Title), value);
            }
        }

        /// <summary>
        /// Site summary
        /// </summary>
        public string Description
        {
            get
            {
                return GetStringValue(nameof(Description), "");
            }
            set
            {
                SetValue(nameof(Description), value);
            }
        }

        /// <summary>
        /// Selected language
        /// </summary>
        public string Language
        {
            get
            {
                return GetStringValue(nameof(Language), "en-GB");
            }
            set
            {
                SetValue(nameof(Language), value);
            }
        }
        /// <summary>
        /// Selected Theme
        /// </summary>
        public string Theme
        {
            get
            {
                return GetStringValue(nameof(Theme), DefaultTheme);
            }
            set
            {
                SetValue(nameof(Theme), value);
            }
        }
        /// <summary>
        /// Articles per page
        /// </summary>
        public int TopicsPerPage
        {
            get
            {
                return GetIntValue(nameof(TopicsPerPage), DefaultTopicPerPage);
            }
            set
            {
                SetValue(nameof(TopicsPerPage), value.ToString());
            }
        }

        /// <summary>
        /// Show article summary only
        /// </summary>
        public bool OnlyShowSummary
        {
            get
            {
                return GetBooleanValue(nameof(OnlyShowSummary), DefaultOnlyShowSummary);
            }
            set
            {
                SetValue(nameof(OnlyShowSummary), value.ToString());
            }
        }

        /// <summary>
        /// Email address for sending mail
        /// </summary>
        public string SmtpEmailAddress
        {
            get
            {
                return GetStringValue(nameof(SmtpEmailAddress), L[DefaultSmtpEmailAddress].Value);
            }
            set
            {
                SetValue(nameof(SmtpEmailAddress), value);
            }
        }

        /// <summary>
        /// SMTP server
        /// </summary>
        public string SmtpServer
        {
            get
            {
                return GetStringValue(nameof(SmtpServer), L[DefaultSmtpServer].Value);
            }
            set
            {
                SetValue(nameof(SmtpServer), value);
            }
        }

        /// <summary>
        /// SMTP server user name
        /// </summary>
        public string SmtpUser
        {
            get
            {
                return GetStringValue(nameof(SmtpUser), L[DefaultSmtpUser].Value);
            }
            set
            {
                SetValue(nameof(SmtpUser), value);
            }
        }

        /// <summary>
        /// SMTP server password
        /// </summary>
        public string SmtpPassword
        {
            get
            {
                return GetStringValue(nameof(SmtpPassword), L[DefaultSmtpPassword].Value);
            }
            set
            {
                SetValue(nameof(SmtpPassword), value);
            }
        }

        /// <summary>
        /// SMTP server port
        /// </summary>
        public int SmtpPort
        {
            get
            {
                return GetIntValue(nameof(SmtpPort), DefaultSmtpPort);
            }
            set
            {
                SetValue(nameof(SmtpPort), value.ToString());
            }
        }

        /// <summary>
        /// Whether the SMTP server uses SSL
        /// </summary>
        public bool SmtpEnableSSL
        {
            get
            {
                return GetBooleanValue(nameof(SmtpEnableSSL), DefaultSmtpEnableSSL);
            }
            set
            {
                SetValue(nameof(SmtpEnableSSL), value.ToString());
            }
        }

        /// <summary>
        /// Send comment email
        /// </summary>
        public bool SendEmailWhenComment
        {
            get
            {
                return GetBooleanValue(nameof(SendEmailWhenComment), DefaultSendEmailWhenComment);
            }
            set
            {
                SetValue(nameof(SendEmailWhenComment), value.ToString());
            }
        }

        /// <summary>
        /// Whether to allow comments
        /// </summary>
        public bool AllowComment
        {
            get
            {
                return GetBooleanValue(nameof(AllowComment), DefaultAllowComment);
            }
            set
            {
                SetValue(nameof(AllowComment), value.ToString());
            }
        }

        /// <summary>
        /// Whether to review comments
        /// </summary>
        public bool VerifyComment
        {
            get
            {
                return GetBooleanValue(nameof(VerifyComment), DefaultVerifyComment);
            }
            set
            {
                SetValue(nameof(VerifyComment), value.ToString());
            }
        }

        /// <summary>
        /// Trust review users who passed the review
        /// </summary>
        public bool TrustAuthenticatedCommentUser
        {
            get
            {
                return GetBooleanValue(nameof(TrustAuthenticatedCommentUser), DefaultTrustAuthenticatedCommentUser);
            }
            set
            {
                SetValue(nameof(TrustAuthenticatedCommentUser), value.ToString());
            }
        }

        /// <summary>
        /// Enable website in comments
        /// </summary>
        public bool EnableCommentWebSite
        {
            get
            {
                return GetBooleanValue(nameof(EnableCommentWebSite), true);
            }
            set
            {
                SetValue(nameof(EnableCommentWebSite), value.ToString());
            }
        }

        /// <summary>
        /// Automatically close comments
        /// </summary>
        public int CloseCommentDays
        {
            get
            {
                return GetIntValue(nameof(CloseCommentDays), 0);
            }
            set
            {
                SetValue(nameof(CloseCommentDays), value.ToString());
            }
        }

        /// <summary>
        /// Error page title
        /// </summary>
        public string ErrorPageTitle
        {
            get
            {
                return GetStringValue(nameof(ErrorPageTitle), L[DefaultErrorPageTitle].Value);
            }
            set
            {
                SetValue(nameof(ErrorPageTitle), value.ToString());
            }
        }

        /// <summary>
        /// Error page content
        /// </summary>
        public string ErrorPageContent
        {
            get
            {
                return GetStringValue(nameof(ErrorPageContent), L[DefaultErrorPageContent].Value);
            }
            set
            {
                SetValue(nameof(ErrorPageContent), value.ToString());
            }
        }

        /// <summary>
        /// Top area script
        /// </summary>
        public string HeaderScript
        {
            get
            {
                return GetStringValue(nameof(HeaderScript), string.Empty);
            }
            set
            {
                SetValue(nameof(HeaderScript), value.ToString());
            }
        }

        /// <summary>
        /// Bottom area script
        /// </summary>
        public string FooterScript
        {
            get
            {
                return GetStringValue(nameof(FooterScript), DefaultFooterScript);
            }
            set
            {
                SetValue(nameof(FooterScript), value.ToString());
            }
        }

        /// <summary>
        /// Enable registration
        /// </summary>
        public bool Registration
        {
            get
            {
                return GetBooleanValue(nameof(Registration), DefaultRegistration);
            }
            set
            {
                SetValue(nameof(Registration), value.ToString());
            }
        }

        #endregion

        #region Private Methods

        private string GetStringValue(string key, string defaultValue)
        {
            if (Settings.ContainsKey(key))
            {
                return Settings[key];
            }
            else
            {
                return defaultValue;
            }
        }

        private int GetIntValue(string key, int defaultValue)
        {
            int result;

            if (Settings.ContainsKey(key))
            {
                if (!int.TryParse(Settings[key], out result))
                {
                    result = defaultValue;
                }
            }
            else
            {
                result = defaultValue;
            }

            return result;
        }

        private bool GetBooleanValue(string key, bool defaultValue)
        {
            bool result;

            if (Settings.ContainsKey(key))
            {
                if (!bool.TryParse(Settings[key], out result))
                {
                    result = defaultValue;
                }
            }
            else
            {
                result = defaultValue;
            }

            return result;
        }

        private void SetValue(string key, string value)
        {
            Settings[key] = value;
        }

        #endregion
    }
}
