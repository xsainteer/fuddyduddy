using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Infrastructure.Configuration;

namespace FuddyDuddy.Core.Infrastructure.AI;

public class OllamaAiService : IOllamaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OllamaAiService> _logger;
    private readonly IOptions<OllamaOptions> _ollamaOptions;

    public OllamaAiService(IHttpClientFactory httpClientFactory, ILogger<OllamaAiService> logger, IOptions<OllamaOptions> ollamaOptions)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _ollamaOptions = ollamaOptions;
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        T sample,
        CancellationToken cancellationToken = default) where T : class
    {
        var request = new
        {
            model = _ollamaOptions.Value.Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = userInput
                }
            },
            format = JsonSchemaGenerator.GenerateJsonSchema(typeof(T)),
            stream = false,
            options = new
            {
                temperature = _ollamaOptions.Value.Temperature,
                num_ctx = _ollamaOptions.Value.MaxTokens
            }
        };

        using var httpClient = _httpClientFactory.CreateClient(Constants.OLLAMA);
        var response = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        var summary = result?.Message?.Content;
        if (summary == null)
        {
            _logger.LogError("No summary generated, response: {Response}", response.Content);
            return null;
        }

        _logger.LogInformation("Summary generated: {Summary}", summary);
        return JsonSerializer.Deserialize<T>(summary);
    }

    private class OllamaResponse
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

}

internal static class JsonSchemaGenerator
{
    public static object GenerateJsonSchema(Type type)
    {
        return new
        {
            type = "object",
            properties = GetProperties(type),
            required = GetRequiredProperties(type)
        };
    }

    private static object GetProperties(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyDict = new Dictionary<string, object>();

        foreach (var prop in properties)
        {
            propertyDict[GetPropertyName(prop)] = GetPropertySchema(prop);
        }

        return propertyDict;
    }

    private static object GetPropertySchema(PropertyInfo prop)
    {
        var schema = new Dictionary<string, object>();

        switch (Type.GetTypeCode(Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType))
        {
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                schema["type"] = "integer";
                break;
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                schema["type"] = "number";
                break;
            case TypeCode.Boolean:
                schema["type"] = "boolean";
                break;
            case TypeCode.DateTime:
                schema["type"] = "string";
                schema["format"] = "date-time";
                break;
            case TypeCode.String:
                schema["type"] = "string";
                break;
            default:
                if (prop.PropertyType.IsArray || prop.PropertyType.IsGenericType && 
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    schema["type"] = "array";
                    var elementType = prop.PropertyType.IsArray ? 
                        prop.PropertyType.GetElementType() : 
                        prop.PropertyType.GetGenericArguments()[0];
                    schema["items"] = GetPropertySchema(elementType);
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    return GenerateJsonSchema(prop.PropertyType);
                }
                break;
        }

        return schema;
    }

    private static object GetPropertySchema(Type type)
    {
        var schema = new Dictionary<string, object>();

        switch (Type.GetTypeCode(Nullable.GetUnderlyingType(type) ?? type))
        {
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                schema["type"] = "integer";
                break;
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                schema["type"] = "number";
                break;
            case TypeCode.Boolean:
                schema["type"] = "boolean";
                break;
            case TypeCode.DateTime:
                schema["type"] = "string";
                schema["format"] = "date-time";
                break;
            case TypeCode.String:
                schema["type"] = "string";
                break;
            default:
                if (type.IsClass && type != typeof(string))
                {
                    return GenerateJsonSchema(type);
                }
                break;
        }

        return schema;
    }

    private static string[] GetRequiredProperties(Type type)
    {
        return type.GetProperties()
            .Where(p => !IsNullable(p))
            .Select(p => GetPropertyName(p))
            .ToArray();
    }

    private static bool IsNullable(PropertyInfo prop)
    {
        return Nullable.GetUnderlyingType(prop.PropertyType) != null ||
            !prop.PropertyType.IsValueType ||
            prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(JsonIgnoreAttribute));
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    private static string GetPropertyName(PropertyInfo prop)
    {
        // Look for JsonPropertyName attribute
        var jsonPropertyAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (jsonPropertyAttr != null)
        {
            return jsonPropertyAttr.Name;
        }

        // Fallback to property name in camelCase if no attribute found
        return ToCamelCase(prop.Name);
    }
}