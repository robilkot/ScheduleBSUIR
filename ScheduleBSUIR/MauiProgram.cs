using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ScheduleBSUIR.Services;
using ScheduleBSUIR.View;
using ScheduleBSUIR.ViewModel;

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
            builder.Services.AddSingleton<AddGroupPage>();
            builder.Services.AddSingleton<TimetablePage>();
            builder.Services.AddSingleton<ExamsPage>();

            builder.Services.AddSingleton<AddGroupViewModel>();
            builder.Services.AddSingleton<TimetableViewModel>();

            builder.Services.AddSingleton<TimetableService>();
            builder.Services.AddSingleton<GroupsService>();

            return builder.Build();
        }
    }
}
