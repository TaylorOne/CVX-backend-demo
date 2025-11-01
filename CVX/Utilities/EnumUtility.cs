using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CVX.Utilities
{
    public class EnumUtility
    {
        /// <summary>
        /// Retrieves the display name of an enum value, if defined. Otherwise, returns the enum value as a string.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The display name or the enum value as a string.</returns>
        public static string GetEnumDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                ?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }

        public static T? GetEnumValueFromDisplayName<T>(string displayName) where T : struct, Enum
        {
            var type = typeof(T);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute?.Name == displayName)
                {
                    return (T)field.GetValue(null);
                }
            }

            return null; // Return null if no match is found
        }
    }
}
