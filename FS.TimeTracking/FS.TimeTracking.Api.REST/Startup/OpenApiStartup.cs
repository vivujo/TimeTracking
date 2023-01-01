﻿using FS.TimeTracking.Api.REST.Filters;
using FS.TimeTracking.Api.REST.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using AssemblyExtensions = FS.TimeTracking.Core.Extensions.AssemblyExtensions;

namespace FS.TimeTracking.Api.REST.Startup;

internal static class OpenApiStartup
{
    private const string OPEN_API_UI_ROUTE = "openapi/";
    private const string SWAGGER_UI_ROUTE = "swagger/";
    private const string OPEN_API_SPEC = "openapi.json";

    public static IServiceCollection RegisterOpenApiController(this IServiceCollection services)
        => services
            .AddSwaggerGenNewtonsoftSupport()
            .AddSwaggerGen(options =>
            {
                const string documentName = ApiV1ControllerAttribute.API_VERSION;
                var productName = AssemblyExtensions.GetProgramProduct();
                options.SwaggerDoc(documentName, new OpenApiInfo { Title = $"{productName} API", Version = ApiV1ControllerAttribute.API_VERSION });

                options.OperationFilter<AddCSharpActionFilter>();
                options.DocumentFilter<FeatureGateDocumentFilter>();

                var restXmlDoc = Path.Combine(AppContext.BaseDirectory, "FS.TimeTracking.Api.REST.xml");
                var abstractionsXmlDoc = Path.Combine(AppContext.BaseDirectory, "FS.TimeTracking.Abstractions.xml");
                options.IncludeXmlComments(restXmlDoc);
                options.IncludeXmlComments(abstractionsXmlDoc);

                options.AddFilterExpressionCreators(restXmlDoc, abstractionsXmlDoc);
            });

    public static WebApplication RegisterOpenApiRoutes(this WebApplication webApplication)
    {
        webApplication
            .UseSwagger(c => c.RouteTemplate = $"{ApiControllerAttribute.API_PREFIX}/{{documentName}}/{OPEN_API_SPEC}")
            .UseSwaggerUI(options =>
            {
                options.RoutePrefix = ApiControllerAttribute.API_PREFIX;
                options.SwaggerEndpoint($"{ApiV1ControllerAttribute.API_VERSION}/{OPEN_API_SPEC}", $"API version {ApiV1ControllerAttribute.API_VERSION}");
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.EnableTryItOutByDefault();
                options.ConfigObject.AdditionalItems.Add("requestSnippetsEnabled", true);

                options.OAuthClientId("timetracking-api");
                options.OAuthUsePkce();
                options.InjectStylesheet("../assets/styles.openapi.css");
                options.EnablePersistAuthorization();
            });

        return webApplication;
    }

    public static WebApplication RegisterOpenApiUiRedirects(this WebApplication app)
    {
        app.MapGet($"/{OPEN_API_UI_ROUTE}{{**path}}", redirectToOpenApiUi);
        app.MapGet($"/{SWAGGER_UI_ROUTE}{{**path}}", redirectToOpenApiUi);

        static IResult redirectToOpenApiUi(HttpContext httpContext, string path)
        {
            var query = httpContext.Request.QueryString;
            var redirectUrl = $"/{ApiControllerAttribute.API_PREFIX}/{path}{query}";
            return Results.Redirect(redirectUrl, true);
        }

        return app;
    }

    public static void GenerateOpenApiSpec(this IHost host, string outFile)
    {
        if (string.IsNullOrWhiteSpace(outFile))
            throw new ArgumentException("No destination file for generated OpenAPI document given.");

        var openApiProvider = host.Services.GetRequiredService<ISwaggerProvider>();
        var openApiDocument = openApiProvider.GetSwagger(ApiV1ControllerAttribute.API_VERSION);
        var openApiJson = openApiDocument.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

        File.WriteAllText(outFile, openApiJson);
    }
}