using System;
using System.Xml.XPath;

namespace Faross.Util
{
    public static class XPathUtil
    {
        public static long? GetLongAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            long longValue;
            return !long.TryParse(stringValue, out longValue) ? (long?) null : longValue;
        }

        public static int? GetIntAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            int longValue;
            return !int.TryParse(stringValue, out longValue) ? (int?) null : longValue;
        }

        public static TimeSpan? GetTimeSpanAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            TimeSpan timeSpanValue;
            return !TimeSpan.TryParse(stringValue, out timeSpanValue) ? (TimeSpan?) null : timeSpanValue;
        }

        public static TEnum? GetEnumAttributeValue<TEnum>(this XPathNavigator navigator, string attributeName)
            where TEnum : struct
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            TEnum enumValue;
            return !Enum.TryParse(stringValue, out enumValue) ? (TEnum?) null : enumValue;
        }

        public static string GetStringAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            return navigator.GetAttribute(attributeName, "");
        }

        public static Uri GetUriAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            Uri uriValue;
            return !Uri.TryCreate(stringValue, UriKind.RelativeOrAbsolute, out uriValue) ? null : uriValue;
        }

        public static bool? GetBoolAttributeValue(this XPathNavigator navigator, string attributeName)
        {
            var stringValue = navigator.GetAttribute(attributeName, "");
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            bool boolValue;
            return !bool.TryParse(stringValue, out boolValue) ? (bool?) null : boolValue;
        }
    }
}