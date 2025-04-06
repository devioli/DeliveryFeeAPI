using App.Endpoints.v1;
using Asp.Versioning;

namespace App.Endpoints;

public static class EndpointExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        // Add a redirect from root to Swagger UI, but exclude from API docs
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var versionedGroup = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);
        
        versionedGroup.MapDeliveryEndpoints();
    }
} 