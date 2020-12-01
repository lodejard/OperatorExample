// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kubernetes.Resources;

namespace Microsoft.Kubernetes.Testing
{
    public class TestClusterStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddTransient<ResourceManager>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSingleton<ITestCluster, TestCluster>();
            services.AddTransient<IResourceSerializers, ResourceSerializers>();
        }

        public void Configure(IApplicationBuilder app, ITestCluster cluster)
        {
            app.Use(next => async context =>
            {
                // This is a no-op, but very convenient for setting a breakpoint to see per-request details.
                await next(context);
            });
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.Run(cluster.UnhandledRequest);
        }
    }
}
