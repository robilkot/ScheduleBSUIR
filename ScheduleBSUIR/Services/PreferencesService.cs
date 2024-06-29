using ScheduleBSUIR.Helpers.Constants;

namespace ScheduleBSUIR.Services
{
    public class PreferencesService
    {
        public PreferencesService()
        {
            InitPreferences();
        }

        public void InitPreferences()
        {
            InitColorPreferences();
        }
        public void ResetPreferences()
        {
            ResetColorPreferences();

            SetClearCacheInterval(7d);
            SetClearCacheLastDate(DateTime.MinValue);
            SetSubgroupTypePreference(SubgroupType.All);
            SetPinnedIdPreference(string.Empty);
        }

        #region Technical preferences

        public void SetClearCacheInterval(double value)
        {
            Preferences.Set(PreferencesKeys.CacheClearInterval, value);
        }
        public double GetClearCacheInterval()
        {
            return Preferences.Get(PreferencesKeys.CacheClearInterval, 7d);
        }

        public void SetClearCacheLastDate(DateTime value)
        {
            Preferences.Set(PreferencesKeys.CacheClearLastDate, value.ToString());
        }
        public DateTime? GetClearCacheLastDate()
        {
            if(DateTime.TryParse(Preferences.Get(PreferencesKeys.CacheClearLastDate, string.Empty), out DateTime lastClearDate))
            {
                return lastClearDate;
            } else
            {
                return null;
            }
        }

        #endregion

        #region Timetable preferences
        public void SetSubgroupTypePreference(SubgroupType value)
        {
            Preferences.Set(PreferencesKeys.SelectedSubgroupType, (int)value);
        }
        public SubgroupType GetSubgroupTypePreference()
        {
            return (SubgroupType)Preferences.Get(PreferencesKeys.SelectedSubgroupType, 0);
        }

        public void SetPinnedIdPreference(string value)
        {
            Preferences.Set(PreferencesKeys.SelectedSubgroupType, value);
        }
        public string GetPinnedIdPreference()
        {
            return Preferences.Get(PreferencesKeys.FavoriteTimetableId, string.Empty);
        }

        #endregion

        #region Color preferences
        public void InitColorPreferences()
        {
            foreach (var (Key, DefaultColor) in PreferencesKeys.ColorPreferencesKeys.Zip(LessonsColorsHelper.AvailableColors))
            {
                string? hexColor = Preferences.Get(Key, null);

                Color newColor = hexColor is null ? DefaultColor : Color.FromRgba(hexColor);

                App.Current!.Resources[Key] = newColor;
            }
        }

        public void ResetColorPreferences()
        {
            foreach (var (Key, DefaultColor) in PreferencesKeys.ColorPreferencesKeys.Zip(LessonsColorsHelper.AvailableColors))
            {
                App.Current!.Resources[Key] = DefaultColor;
            }
        }

        public void SetColorPreference(string colorPreferenceKey, Color value)
        {
            if (!PreferencesKeys.ColorPreferencesKeys.Contains(colorPreferenceKey))
                throw new ArgumentException("Wrong color preference key");

            App.Current!.Resources[colorPreferenceKey] = value;

            Preferences.Set(colorPreferenceKey, value.ToHex());
        }

        public Color GetColorPreference(string colorPreferenceKey)
        {
            if (!PreferencesKeys.ColorPreferencesKeys.Contains(colorPreferenceKey))
                throw new ArgumentException("Wrong color preference key");

            return (Color)App.Current!.Resources[colorPreferenceKey];
        }

        #endregion
    }
}
