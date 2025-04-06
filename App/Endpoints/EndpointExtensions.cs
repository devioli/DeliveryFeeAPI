using App.Endpoints.v1;

namespace App.Endpoints;

public static class EndpointExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        // Add a redirect from root to Swagger UI, but exclude from API docs
        app.MapGet("/", () => Results.Redirect("/swagger"))
           .ExcludeFromDescription();
        
        // Map versioned endpoints
        app.MapDeliveryEndpoints();
    }
} 