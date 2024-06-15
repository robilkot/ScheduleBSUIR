using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
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

            builder.Services.AddTransient<TimetablePage>();
            builder.Services.AddTransient<ExamsPage>();

            builder.Services.AddSingleton<TimetableViewModel>();
            builder.Services.AddSingleton<ExamsPageViewModel>();

            //builder.Services.AddSingleton<GroupsService>();
            builder.Services.AddSingleton<TimetableService>();
            builder.Services.AddSingleton<WebService>();

            return builder.Build();
        }
    }
}
