using DAdmin.Dto;
using DAdmin.Helpers;

namespace DAdmin.Extensions;

public static class DataGenerationExtension
{
    public static Task<Dictionary<string, string>> GenerateTestData(this IEnumerable<DataProperty> properties)
    {
        var data = new Dictionary<string, string>();

        var faker = new Bogus.Faker();
        foreach (var tableProperty in properties)
        {
            Type propType = tableProperty.EntityPropertyInfo.PropertyType;

            if (propType == typeof(string))
            {
                // Generate a realistic random string
                data[tableProperty.Name] = faker.Random.Words();
            }
            else if (ClassHelper.IsNumericType(propType))
            {
                // Generate a realistic random number
                data[tableProperty.Name] = faker.Random.Number(0, 1000).ToString();
            }
            else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                // Generate a realistic random date and time
                data[tableProperty.Name] = faker.Date.Past(10).ToString("o"); // ISO 8601 format
            }
            else
            {
                // For unknown types, still generate a GUID as a placeholder
                data[tableProperty.Name] = Guid.NewGuid().ToString();
            }
        }

        return Task.FromResult(data);
    }
}