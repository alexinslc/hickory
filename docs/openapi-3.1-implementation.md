# OpenAPI 3.1 Implementation Guide

## Overview

Hickory Help Desk API now supports OpenAPI 3.1 specification with both JSON and YAML format endpoints. This document describes the implementation and available endpoints.

## Features

### OpenAPI 3.1 Compliance
- Full OpenAPI 3.1 specification support
- JSON Schema draft 2020-12 compliance
- Enhanced schema validation capabilities
- Industry standard API documentation

### Multiple Format Support
- **JSON Format**: Traditional Swagger/OpenAPI JSON format
- **YAML Format**: Human-readable YAML format via .NET 10 built-in support

## Available Endpoints

### Swagger UI (Interactive Documentation)
- **URL**: `https://localhost:5000/api-docs`
- **Description**: Interactive API documentation with "Try it out" functionality
- **Features**:
  - JWT Bearer authentication support
  - Request/response examples
  - Schema validation
  - Multiple format endpoints accessible from UI

### OpenAPI JSON Endpoint (Swashbuckle)
- **URL**: `https://localhost:5000/swagger/v1/swagger.json`
- **Format**: JSON
- **Version**: OpenAPI 3.0/3.1 (auto-detected by Swashbuckle v10)
- **Use Case**: API client generation, CI/CD validation

### OpenAPI YAML Endpoint (.NET 10 Built-in)
- **URL**: `https://localhost:5000/openapi/v1.json` (JSON)
- **URL**: `https://localhost:5000/openapi/v1.yaml` (YAML)
- **Format**: JSON or YAML
- **Version**: OpenAPI 3.1
- **Use Case**: Human-readable documentation, version control

## Implementation Details

### Dependencies
- `Swashbuckle.AspNetCore` v10.1.0 - Swagger UI and OpenAPI generation
- `Microsoft.AspNetCore.OpenApi` v10.0.0 - Built-in .NET 10 OpenAPI support
- `Microsoft.OpenApi` v2.3.0 - OpenAPI specification library

### Configuration

#### Swashbuckle Configuration
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hickory Help Desk API",
        Version = "v1",
        Description = "API for Hickory Help Desk System with OpenAPI 3.1 support",
        Contact = new OpenApiContact
        {
            Name = "Hickory Support",
            Email = "support@hickory.dev"
        }
    });

    // JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
});
```

#### .NET 10 Built-in OpenAPI
```csharp
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
});

// In middleware pipeline
app.MapOpenApi();
```

## Usage Examples

### Accessing OpenAPI Spec in YAML
```bash
curl https://localhost:5000/openapi/v1.yaml > openapi-spec.yaml
```

### Accessing OpenAPI Spec in JSON
```bash
curl https://localhost:5000/swagger/v1/swagger.json > openapi-spec.json
```

### Generating API Clients
```bash
# Using OpenAPI Generator with YAML format
openapi-generator generate \
  -i https://localhost:5000/openapi/v1.yaml \
  -g typescript-axios \
  -o ./generated-client

# Using NSwag with JSON format
nswag openapi2ts client \
  /input:https://localhost:5000/swagger/v1/swagger.json \
  /output:./generated-client/api.ts
```

## Benefits

### For Developers
- **Better IntelliSense**: Enhanced schema definitions improve IDE support
- **Accurate Validation**: OpenAPI 3.1's JSON Schema integration provides precise validation
- **Modern Standards**: Aligned with latest API documentation standards

### For API Consumers
- **Multiple Formats**: Choose JSON or YAML based on preference
- **Client Generation**: Generate type-safe API clients in any language
- **Interactive Documentation**: Test endpoints directly from Swagger UI

### For DevOps
- **CI/CD Integration**: Validate API contracts in pipelines
- **Version Control**: Track API changes with human-readable YAML
- **Contract Testing**: Ensure API implementations match specifications

## Migration Notes

### Breaking Changes
- None - This is an enhancement to existing functionality
- Existing `/swagger/v1/swagger.json` endpoint remains unchanged
- New `/openapi/v1.yaml` endpoint adds YAML support

### Backward Compatibility
- All existing API clients continue to work
- Swagger UI functionality unchanged
- Authentication mechanisms unchanged

## Future Enhancements

- [ ] Add OpenAPI spec to version control
- [ ] Integrate spec validation in CI/CD pipeline
- [ ] Generate API client libraries automatically
- [ ] Add example requests/responses to all endpoints
- [ ] Implement webhook documentation

## References

- [OpenAPI 3.1 Specification](https://spec.openapis.org/oas/v3.1.0)
- [Swashbuckle v10 Migration Guide](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- [.NET 10 OpenAPI Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi)
- [JSON Schema 2020-12](https://json-schema.org/draft/2020-12/json-schema-core.html)

## Support

For issues or questions about the API documentation:
- Email: support@hickory.dev
- GitHub Issues: https://github.com/alexinslc/hickory/issues
