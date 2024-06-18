using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using System.Diagnostics;

namespace ScheduleBSUIR.Helpers.MarkupExtensions
{
    [ContentProperty(nameof(Uri))]
    public partial class CachedImageSourceExtension : BindableObject, IMarkupExtension<CacheableImageSource>
    {
        private readonly DbService _dbService;

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            nameof(Uri), typeof(string), typeof(CachedImageSourceExtension), string.Empty);

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public CachedImageSourceExtension()
        {
            _dbService = App.Current.Handler.MauiContext.Services.GetRequiredService<DbService>();
        }
        public CacheableImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Uri))
            {
                return null!;
            }

            CacheableImageSource image;

            var cachedImage = _dbService.Get<CacheableImageSource>(Uri);

            if (cachedImage is null)
            {
                image = (CacheableImageSource)ImageSource.FromUri(new Uri(Uri));
                image.Source = Uri;

                Debug.WriteLine($"Using NOT cached image for {Uri}");
            }
            else
            {
                image = cachedImage;

                Debug.WriteLine($"Using cached image for {Uri}");
            }

            // Not used, actually
            image.UpdatedAt = DateTime.Now;
            image.AccessedAt = DateTime.Now;

            _dbService.AddOrUpdate(image);
            Debug.WriteLine("Updated image in DB");

            return image;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
