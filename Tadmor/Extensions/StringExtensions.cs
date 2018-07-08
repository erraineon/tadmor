using System;
using CronExpressionDescriptor;

namespace Tadmor.Extensions
{
    public static class StringExtensions
    {
        public static string ToCronDescription(this string cron)
        {
            string description;
            try
            {
                description = ExpressionDescriptor.GetDescription(cron).ToLower();
            }
            catch (Exception)
            {
                throw new Exception("invalid cron expression");
            }

            return description;
        }
    }
}