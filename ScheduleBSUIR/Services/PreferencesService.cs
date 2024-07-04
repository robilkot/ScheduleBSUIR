using CommunityToolkit.Mvvm.Messaging;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models.Messaging;
using static ScheduleBSUIR.Helpers.Constants.ColorsHelper;
using static ScheduleBSUIR.Helpers.Constants.PreferencesKeys;

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
            Preferences.Set(CacheClearInterval, value);
        }
        public double GetClearCacheInterval()
        {
            return Preferences.Get(CacheClearInterval, 7d);
        }

        public void SetClearCacheLastDate(DateTime value)
        {
            Preferences.Set(CacheClearLastDate, value.ToString());
        }
        public DateTime? GetClearCacheLastDate()
        {
            if (DateTime.TryParse(Preferences.Get(CacheClearLastDate, string.Empty), out DateTime lastClearDate))
            {
                return lastClearDate;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Timetable preferences
        public void SetSubgroupTypePreference(SubgroupType value)
        {
            Preferences.Set(SelectedSubgroupType, (int)value);
        }
        public SubgroupType GetSubgroupTypePreference()
        {
            return (SubgroupType)Preferences.Get(SelectedSubgroupType, 0);
        }

        public void SetPinnedIdPreference(string value)
        {
            Preferences.Set(FavoriteTimetableId, value);
        }
        public string GetPinnedIdPreference()
        {
            return Preferences.Get(FavoriteTimetableId, string.Empty);
        }

        #endregion

        #region Color preferences
        public void InitColorPreferences()
        {
            ColorPreferencesKeys.ForEach(key =>
            {
                string? hexColor = Preferences.Get(key, null);

                if (hexColor is null)
                {
                    SetDefaultColorPreference(key);
                }
                else
                {
                    App.Current!.Resources[key] = Color.FromRgba(hexColor);
                }
            });
        }

        public void ResetColorPreferences()
        {
            ColorPreferencesKeys.ForEach(SetDefaultColorPreference);
        }

        // A place to set default colors
        private void SetDefaultColorPreference(string key)
        {
            App.Current!.Resources[key] = key switch
            {
                LectureColor => Green,
                PracticeColor => Red,
                LabColor => Yellow,
                ExamColor => Blue,
                ConsultColor => Brown,
                CreditColor => Lightblue,
                AnnouncementColor => Gray,
                UnknownColor => Gray,
                _ => Gray,
            };
        }

        public void SetColorPreference(string colorPreferenceKey, Color value)
        {
            if (!ColorPreferencesKeys.Contains(colorPreferenceKey))
                throw new ArgumentException("Wrong color preference key");

            App.Current!.Resources[colorPreferenceKey] = value;

            Preferences.Set(colorPreferenceKey, value.ToHex());

            WeakReferenceMessenger.Default.Send(new ColorPreferenceUpdatedMessage((colorPreferenceKey, value)));
        }

        public Color GetColorPreference(string colorPreferenceKey)
        {
            if (!ColorPreferencesKeys.Contains(colorPreferenceKey))
                throw new ArgumentException("Wrong color preference key");

            return (Color)App.Current!.Resources[colorPreferenceKey];
        }

        #endregion
    }
}
