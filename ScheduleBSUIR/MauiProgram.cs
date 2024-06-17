using CommunityToolkit.Maui;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<GroupListPage>();
            builder.Services.AddTransient<ExamsPage>();
            builder.Services.AddTransient<TimetablePage>();

            builder.Services.AddSingleton<GroupListPageViewModel>();
            builder.Services.AddSingleton<ExamsPageViewModel>();
            builder.Services.AddSingleton<TimetableViewModel>();

            builder.Services.AddSingleton<GroupsService>();
            builder.Services.AddSingleton<TimetableService>();
            builder.Services.AddSingleton<DbService>();
            builder.Services.AddSingleton<WebService>();

            builder.Services.AddSingleton<IDateTimeProvider, DateTimeProviderService>();

            return builder.Build();
        }
    }
}
