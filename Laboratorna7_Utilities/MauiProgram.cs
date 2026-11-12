using Microsoft.Extensions.Logging;
using Laboratorna7_Utilities.Data;
using Microsoft.EntityFrameworkCore;

namespace Laboratorna7_Utilities
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            const string connectionString =
    "Server=localhost;Database=Utilities;User ID=root;Password=bohdanna1106;";

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
#endif
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ServicesPage>();

            return builder.Build();
        }
    }
}
