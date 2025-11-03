using UnityEngine;

namespace REIW
{
    // This is the extension method.
    // The first parameter takes the "this" modifier
    // and specifies the type for which the method is defined.
    public static class StringExtension
    {
        public static string Bold(this string str) => "<b>" + str + "</b>";
        
        /// <summary> clr ex : "#FF00FF" </summary>
        public static string Color(this string str, string clr) => $"<color={clr}>{str}</color>";

        public static string Color(this string str, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{hex}>{str}</color>";
        }
        
        public static string Italic(this string str) => "<i>" + str + "</i>";
        public static string Size(this string str, int size) => $"<size={size}>{str}</size>";
    }
}
