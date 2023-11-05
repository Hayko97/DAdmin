using System.Reflection;
using System.Text;

namespace DynamicAdmin.Components.Helpers;

public static class ClassHelper
{
    public static List<object> GetPropertyValues(object entity, int numberOfProperties = 10)
    {
        var propertyValues = new List<object>();
        var properties = entity.GetType().GetProperties().Take(numberOfProperties);

        foreach (var prop in properties)
        {
            propertyValues.Add(prop.GetValue(entity));
        }

        return propertyValues;
    }

    public static void SetStringValue<T>(object value, PropertyInfo prop, T item)
    {
        Type propertyType = prop.PropertyType;

        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            propertyType = Nullable.GetUnderlyingType(propertyType);
        }

        if (IsNumericType(propertyType))
        {
            value = Convert.ChangeType(value, propertyType);
        }
        else if (propertyType == typeof(DateTime))
        {
            if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
            {
                value = parsedDate;
            }
        }
        else if (propertyType == typeof(bool))
        {
            if (bool.TryParse(value.ToString(), out bool parsedBool))
            {
                value = parsedBool;
            }
        }
        else if (propertyType == typeof(string))
        {
            value = value.ToString();
        }
        else if (propertyType == typeof(Guid))
        {
            if (Guid.TryParse(value.ToString(), out Guid parsedGuid))
            {
                value = parsedGuid;
            }
        }
        else if (propertyType == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(value.ToString(), out TimeSpan parsedTimeSpan))
            {
                value = parsedTimeSpan;
            }
        }
        else if (propertyType == typeof(byte[]))
        {
            value = Encoding.UTF8.GetBytes(value.ToString()); // Or any suitable encoding.
        }
        else if (propertyType == typeof(char))
        {
            value = value.ToString()[0];
        }

        prop.SetValue(item, value);
    }

    public static bool IsNumericType(Type type)
    {
        return type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(ushort) ||
               type == typeof(int) ||
               type == typeof(uint) ||
               type == typeof(long) ||
               type == typeof(ulong) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(decimal);
    }
}