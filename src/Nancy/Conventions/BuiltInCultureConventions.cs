﻿namespace Nancy.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Nancy.Configuration;
    using Nancy.Session;

    /// <summary>
    /// Built in functions for determining current culture
    /// <seealso cref="DefaultCultureConventions"/>
    /// </summary>
    public static class BuiltInCultureConventions
    {
        /// <summary>
        /// Checks to see if the Form has a CurrentCulture key.
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo if found in Form otherwise null</returns>
        public static CultureInfo FormCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
            if (context.Request.Form["CurrentCulture"] != null)
            {
                string cultureLetters = context.Request.Form["CurrentCulture"];

                if (!IsValidCultureInfoName(cultureLetters, configuration))
                {
                    return null;
                }

                return new CultureInfo(cultureLetters);
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the first argument in the Path can be used to make a CultureInfo.
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo if found in Path otherwise null</returns>
        public static CultureInfo PathCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
            var segments =
                context.Request.Url.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var firstSegment =
                segments.FirstOrDefault();

            if (firstSegment != null && IsValidCultureInfoName(firstSegment, configuration))
            {
                context.Request.Url.Path =
                    string.Concat("/", string.Join("/", segments.Skip(1)));

                return new CultureInfo(firstSegment);
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the AcceptLanguage in the Headers can be used to make a CultureInfo. Uses highest weighted if multiple defined.
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo if found in Headers otherwise null</returns>
        public static CultureInfo HeaderCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
            if (context.Request.Headers.AcceptLanguage.Any())
            {
                var cultureLetters = context.Request.Headers.AcceptLanguage.First().Item1;

                if (!IsValidCultureInfoName(cultureLetters, configuration))
                {
                    return null;
                }

                return new CultureInfo(cultureLetters);
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the Session has a CurrentCulture key
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo if found in Session otherwise null</returns>
        public static CultureInfo SessionCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
            var sessionType = context.Request.Session as NullSessionProvider;
            if (sessionType == null && context.Request.Session["CurrentCulture"] != null)
            {
                return (CultureInfo)context.Request.Session["CurrentCulture"];
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the Cookies has a CurrentCulture key
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo if found in Cookies otherwise null</returns>
        public static CultureInfo CookieCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
            string cookieCulture = null;

            if (context.Request.Cookies.TryGetValue("CurrentCulture", out cookieCulture))
            {
                if (!IsValidCultureInfoName(cookieCulture, configuration))
                {
                    return null;
                }

                return new CultureInfo(cookieCulture);
            }

            return null;
        }

        /// <summary>
        /// Uses the Thread.CurrentThread.CurrentCulture
        /// </summary>
        /// <param name="context">NancyContext</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>CultureInfo from CurrentThread</returns>
        public static CultureInfo ThreadCulture(NancyContext context, GlobalizationConfiguration configuration)
        {
#if DOTNET5_4
            CultureInfo.CurrentCulture = new CultureInfo(configuration.SupportedCultureNames.First());
            return CultureInfo.CurrentCulture;
#else
            return Thread.CurrentThread.CurrentCulture;
#endif
        }

        /// <summary>
        /// Validates culture name
        /// </summary>
        /// <param name="name">Culture name eg\en-GB</param>
        /// <param name="configuration">Culture configuration that contains allowed cultures</param>
        /// <returns>True/False if valid culture</returns>
        public static bool IsValidCultureInfoName(string name, GlobalizationConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return configuration.SupportedCultureNames.Contains(name);
        }
    }
}
