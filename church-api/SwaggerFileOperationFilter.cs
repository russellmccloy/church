using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace church_api;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Name == "file" && context.ApiDescription.HttpMethod == "POST")
            {
                parameter.In = ParameterLocation.Query;
                parameter.Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
        }
    }
}
