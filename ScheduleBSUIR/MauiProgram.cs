using CommunityToolkit.Maui;
using DevExpress.Maui;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using ScheduleBSUIR.Viewmodels;

namespace ScheduleBSUIR
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseDevExpress()
                .UseDevExpressControls()
                .UseDevExpressCollectionView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.UseLeakDetection(collectionTarget =>
            {
                // This callback will run any time a leak is detected.
                var logger = App.Current.MainPage.Handler.MauiContext.Services.GetRequiredService<ILoggingService>();

                logger?.LogInfo($"Leaked {collectionTarget.Name}", displayCaller: false);
            });
            
            //builder.UseLeakDetection();

            builder.Services.AddTransient<GroupListPage>();
            builder.Services.AddTransient<TimetablePage>();
            builder.Services.AddTransient<DebugLogPage>();

            builder.Services.AddTransient<GroupListPageViewModel>();
            builder.Services.AddTransient<TimetablePageViewModel>();
            builder.Services.AddTransient<DebugLogPageViewModel>();

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
