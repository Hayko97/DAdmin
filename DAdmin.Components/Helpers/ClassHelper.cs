using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace DAdmin.Components.Helpers;

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

        try
        {
            prop.SetValue(item, value);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public static bool IsNullOrDefaultValue(PropertyInfo prop, object value)
    {
        // If the value is null, we immediately know it's either the default value
        // for a reference type, or an explicitly set null for a nullable value type.
        if (value == null) return true;

        // Get the type of the property
        Type type = prop.PropertyType;

        // If it's a value type and not a nullable type, compare with the default value.
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            object defaultValue = Activator.CreateInstance(type);
            return value.Equals(defaultValue);
        }

        // For nullable value types, if it's not null, it's not the default value.
        // For reference types, we've already checked for null.
        return false;
    }

    public static object GetPropertyValue(object obj, string propName)
    {
        return obj.GetType().GetProperty(propName)?.GetValue(obj);
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
    
    public static Dictionary<string, object> ExtractParameters<TComponent>(TComponent component) 
        where TComponent : class
    {
        var parameters = new Dictionary<string, object>();

        foreach (var property in typeof(TComponent).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var parameterAttribute = property.GetCustomAttribute(typeof(ParameterAttribute));
            if (parameterAttribute != null)
            {
                var value = property.GetValue(component);
                if (value != null)
                {
                    parameters.Add(property.Name, value);
                }
            }
        }

        return parameters;
    }
}