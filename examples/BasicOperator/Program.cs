// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BasicOperator.Models;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BasicOperator
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureHostConfiguration(hostConfiguration =>
            {
                hostConfiguration.AddCommandLine(args);
            });

            hostBuilder.ConfigureServices(services =>
            {
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                });

                services.AddCustomResourceDefinitionUpdater<V1alpha1HelloWorld>(options =>
                {
                    options.Scope = "Namespaced";
                });

                services.AddOperator<V1alpha1HelloWorld>(settings =>
                {
                    settings.WithRelatedResource<V1Deployment>();
                    settings.WithRelatedResource<V1ServiceAccount>();
                    settings.WithRelatedResource<V1Service>();
                    settings.WithRelatedResource<V1Ingress>();
                });
            });

            await hostBuilder.RunConsoleAsync();
            return 0;
        }
    }
}
