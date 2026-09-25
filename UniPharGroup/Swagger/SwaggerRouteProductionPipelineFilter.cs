using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace UniPharGroup.Swagger;

public class SwaggerRouteProductionPipelineFilter : SwaggerRouteTemplatePipelineFilter
{
    public SwaggerRouteProductionPipelineFilter() : base("umbraco") { }

    protected override bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder) => true;
}

public static class SwaggerProductionUmbracoBuilderExtensions
{
    public static IUmbracoBuilder ConfigureProductionSwaggerRoute(this IUmbracoBuilder builder)
    {
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            // Remove Umbraco's default filter (which hides Swagger in Production)
            options.PipelineFilters.RemoveAll(filter => filter is SwaggerRouteTemplatePipelineFilter);
            // Replace it with our own, always-enabled version
            options.PipelineFilters.Add(new SwaggerRouteProductionPipelineFilter());
        });

        return builder;
    }
}