namespace KestrelTestService
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.AspNetCore.Hosting;

    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            var hostBuilder = CreateHostBuilder();
            hostBuilder.Build().Run();
        }

        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .UseServiceFabricHost()
                .UseServiceFabricRuntime()
                .ConfigureWebHost(b => b.UseStartup<Startup>())
                ;
        }
    }
}
