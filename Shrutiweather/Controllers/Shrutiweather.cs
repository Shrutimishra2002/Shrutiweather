using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Shrutiweather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShrutiweatherController : ControllerBase
    {
        [HttpPost("generate")]
        public IActionResult GenerateForecasts([FromQuery] int count,[FromBody] List<SchemaField> schema)
        {
            try
            {
                var service = new WeatherForecastService();
                var forecasts = service.GenerateForecasts(count,schema);
                return Ok(forecasts);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    public class WeatherForecastService
    {
        private static readonly Random random = new Random();

        public JObject GenerateForecasts(int count,List<SchemaField> schema)
        {
            var forecasts = new JArray();

            for (int i = 0; i < count; i++)
            {
                var forecast = GenerateForecastData(schema);
                forecasts.Add(forecast);
            }

            var response = new JObject();
            response["Report_Entry"] = forecasts;

            //return new JArray(response);
            return response;
        }

        private JObject GenerateForecastData(List<SchemaField> schema)
        {
            var forecast = new JObject();

            foreach (var field in schema)
            {
                var fieldName = field.fieldName;
                var fieldType = field.fieldType;
                List<string> enumValues = null;

                if (fieldType.ToLower() == "enum")
                {
                    if (field.enumValues == null || !field.enumValues.Any())
                        throw new ArgumentException($"Enum values are required for field: {fieldName}");

                    enumValues = field.enumValues;
                }

                var value = GenerateValue(fieldType, enumValues, field.schema);
                forecast[fieldName] = value;
            }

            return forecast;
        }

        private JToken GenerateValue(string type, List<string> enumValues = null, List<SchemaField> schema = null)
        {
            switch (type.ToLower())
            {
                case "string":
                    return GenerateRandomString();
                case "integer":
                    return GenerateRandomInteger();
                case "enum":
                    if (enumValues == null || !enumValues.Any())
                        throw new ArgumentException("Enum values are required.");
                    return JToken.FromObject(GenerateRandomEnum(enumValues));
                case "bool":
                    return GenerateRandomBoolean();
                case "object":
                    if (schema == null || !schema.Any())
                        throw new ArgumentException("Schema is required for object type.");
                    return GenerateForecastData(schema);
                case "array":
                    if (schema == null || !schema.Any())
                        throw new ArgumentException("Schema is required for array type.");
                    return GenerateRandomArray(schema);
                default:
                    throw new ArgumentException($"Unsupported field type: {type}");
            }
        }
        
        private string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int GenerateRandomInteger()
        {
            return random.Next(1, 100);
        }

        private string GenerateRandomEnum(List<string> enumValues)
        {
            return enumValues[random.Next(enumValues.Count)];
        }

        private bool GenerateRandomBoolean()
        {
            return random.Next(0, 2) == 1;
        }

        private JArray GenerateRandomArray(List<SchemaField> schema)
        {
            var array = new JArray();
            int arraySize = random.Next(1, 5);

            for (int i = 0; i < arraySize; i++)
            {
                array.Add(GenerateForecastData(schema));
            }

            return array;
        }
    }

    public class SchemaField
    {
        public string fieldName { get; set; }
        public string fieldType { get; set; }
        public List<string>? enumValues { get; set; }
        public List<SchemaField> ?schema { get; set; }
    }
}
