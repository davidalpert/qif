﻿using System;
using QifApi.Config;
using System.Diagnostics;
using System.Globalization;
using QifApi.Helpers;
using QifApi.Transactions;

namespace QifApi
{

    internal static class Extensions
    {
        internal static BasicTransaction BuildBasicTransaction(this AccountListTransaction account)
        {
            return new BasicTransaction
                {
                    Account = account
                };
        }

        internal static InvestmentTransaction BuildInvestmentTransaction(this AccountListTransaction account)
        {
            return new InvestmentTransaction
                {
                    Account = account
                };
        }

        internal static string GetDateString(this DateTime @this, Configuration config)
        {
            string result = null;

            if (config.WriteDateFormatMode == WriteDateFormatMode.Default)
            {
                result = @this.ToShortDateString();
            }
            else
            {
                Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomWriteDateFormat));
                result = @this.ToString(config.CustomWriteDateFormat);
            }

            return result;
        }

        internal static string GetDecimalString(this decimal @this, Configuration config)
        {
            string result = null;

            if (config.WriteDecimalFormatMode == WriteDecimalFormatMode.Default)
            {
                result = @this.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomWriteDecimalFormat));
                result = @this.ToString(config.CustomWriteDecimalFormat);
            }

            return result;
        }

        internal static DateTime ParseDateString(this string @this, Configuration config)
        {
            // Prepare the return value
            var result = new DateTime();
            var success = false;

            using (new SpoofCulture(config.CustomReadCultureInfo ?? CultureInfo.CurrentCulture))
            {
                if (config.ReadDateFormatMode == ReadDateFormatMode.Default)
                {
                    // If parsing the date string fails
                    success = DateTime.TryParse(GetRealDateString(@this), CultureInfo.CurrentCulture, config.ParseDateTimeStyles, out result);
                }
                else
                {
                    Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomReadDateFormat));
                    success = DateTime.TryParseExact(GetRealDateString(@this), config.CustomReadDateFormat, CultureInfo.CurrentCulture, config.ParseDateTimeStyles, out result);
                }
            }

            // If parsing failed
            if (!success)
            {
                // Identify that the value couldn't be formatted
                throw new InvalidCastException(Resources.InvalidDateFormat);
            }

            // Return the date value
            return result;
        }

        internal static decimal ParseDecimalString(this string @this, Configuration config)
        {
            var result = 0m;
            var success = false;

            if (config.ReadDecimalFormatMode == ReadDecimalFormatMode.Default)
            {
                success = decimal.TryParse(@this, out result);
            }
            else
            {
                success = decimal.TryParse(@this, config.ParseNumberStyles, CultureInfo.CurrentCulture, out result);
            }

            // If parsing failed
            if (!success)
            {
                throw new InvalidCastException(Resources.InvalidDecimalFormat);
            }

            return result;
        }

        internal static bool ParseBooleanString(this string @this, Configuration config)
        {
            bool result = false;

            if (!bool.TryParse(@this, out result) && !string.IsNullOrWhiteSpace(@this))
            {
                throw new InvalidCastException(Resources.InvalidBooleanFormat);
            }

            return result;
        }

        private static string GetRealDateString(string qifDateString)
        {
            // Find the apostraphe
            int i = qifDateString.IndexOf("'", StringComparison.Ordinal);

            // Prepare the return string
            string result = "";

            // If the apostraphe is present
            if (i != -1)
            {
                // Extract everything but the apostraphe
                result = qifDateString.Substring(0, i) + "/" + qifDateString.Substring(i + 1);

                // Replace spaces with zeros
                result = result.Replace(" ", "0");

                // Return the new string
                return result;
            }
            else
            {
                // Otherwise, just return the raw value
                return qifDateString;
            }
        }
    }
}
