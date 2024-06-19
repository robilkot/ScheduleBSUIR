using CommunityToolkit.Maui;
using DevExpress.Maui;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using ScheduleBSUIR.View.Controls;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            BorderPerformanceWorkaround.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseDevExpress()
                .UseDevExpressCollectionView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();

            builder.UseLeakDetection(collectionTarget =>
            {
                // This callback will run any time a leak is detected.
                var logger = App.Current.MainPage.Handler.MauiContext.Services.GetRequiredService<ILoggingService>();

                logger?.LogInfo($"leaked {collectionTarget.Name}");
            });
#endif

            builder.Services.AddTransient<GroupListPage>();
            builder.Services.AddTransient<TimetablePage>();

            builder.Services.AddTransient<GroupListPageViewModel>();
            builder.Services.AddTransient<TimetablePageViewModel>();

            builder.Services.AddSingleton<GroupsService>();
            builder.Services.AddSingleton<TimetableService>();
            builder.Services.AddSingleton<DbService>();
            builder.Services.AddSingleton<WebService>();
            builder.Services.AddSingleton<ILoggingService, LoggingService>();
            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProviderService>();

            return builder.Build();
        }
    }
}
