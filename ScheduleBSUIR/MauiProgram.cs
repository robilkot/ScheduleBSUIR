using CommunityToolkit.Maui;
using DevExpress.Maui;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.UnitTests.Services;
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
                var logger = App.Current?.MainPage?.Handler?.MauiContext?.Services.GetRequiredService<ILoggingService>();

                logger?.LogInfo($"Leak {collectionTarget.Name}", displayCaller: false);
            });

            builder.UseGlobalExceptionHandler(MauiExceptions_UnhandledException);

            builder.Services.AddTransient<EmployeesListPage>();
            builder.Services.AddTransient<GroupListPage>();
            builder.Services.AddTransient<TimetablePage>();
            builder.Services.AddTransient<DebugPage>();

            builder.Services.AddTransient<EmployeesListPageViewModel>();
            builder.Services.AddTransient<GroupListPageViewModel>();
            builder.Services.AddTransient<TimetablePageViewModel>();
            builder.Services.AddTransient<DebugPageViewModel>();

            builder.Services.AddSingleton<GroupsService>();
            builder.Services.AddSingleton<EmployeesService>();
            builder.Services.AddSingleton<TimetableService>();
            builder.Services.AddSingleton<DbService>();
            builder.Services.AddSingleton<WebService>();
            builder.Services.AddSingleton<ILoggingService, LoggingService>();
#if DEBUG
            builder.Services.AddSingleton<IDateTimeProvider, FixedDateTimeProvider>(); // This breaks caching logic if using real web service to get last update dates
#else
            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProviderService>();
#endif

            return builder.Build();
        }

        private static void MauiExceptions_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = App.Current?.MainPage?.Handler?.MauiContext?.Services.GetRequiredService<ILoggingService>();

            var exception = e.ExceptionObject as Exception;

            string info = $"Unhandled exception: {exception?.Message}\n{exception?.StackTrace}";

            logger?.LogError(info, displayCaller: false);
        }
    }
}
