using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody == null) return;

        if (operation.RequestBody.Content.ContainsKey("multipart/form-data"))
        {
            operation.RequestBody.Content["multipart/form-data"].Schema = new OpenApiSchema
            {
                Type = "object",
                Properties =
                {
                    { "Title", new OpenApiSchema { Type = "string" } },
                    { "Author", new OpenApiSchema { Type = "string" } },
                    { "IsAvailable", new OpenApiSchema { Type = "boolean" } },
                    { "pdfFile", new OpenApiSchema { Type = "string", Format = "binary" } },
                },
            };
        }
    }
}