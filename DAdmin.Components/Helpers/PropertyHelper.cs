using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DAdmin.Components.Helpers;

public static class PropertyHelper
{
    public static string GetInputType(PropertyInfo propertyInfo)
    {
        Type type = propertyInfo.PropertyType;

        // Numeric types
        if (ClassHelper.IsNumericType(type))
        {
            return "number";
        }

        // Date and time types
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return "date";
        }

        // Boolean type
        if (type == typeof(bool))
        {
            return "checkbox";
        }

        // Email
        if (propertyInfo.GetCustomAttributes(typeof(EmailAddressAttribute), true).Any())
        {
            return "email";
        }

        // Password (can be inferred from naming conventions or specific attributes)
        if (propertyInfo.Name.ToLowerInvariant().Contains("password"))
        {
            return "password";
        }

        // Default to text
        return "text";
    }

    public static bool NeedsValidation(PropertyInfo propertyInfo)
    {
        // Check if the property has any validation attributes
        return propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true).Any();
    }
}