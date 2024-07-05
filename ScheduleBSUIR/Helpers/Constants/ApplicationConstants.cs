namespace ScheduleBSUIR.Helpers.Constants
{
    public class ApplicationConstants
    {   
        private static bool? s_isProduction;
        public static bool IsProduction
        {
            get
            {
                s_isProduction ??= GetIsProduction();

                return s_isProduction.GetValueOrDefault();
            }
        }
        public static bool IsDev => !IsProduction;

        public static string CurrentVersion => (IsDev ? "Dev " : string.Empty) + VersionTracking.CurrentVersion;

        // Odd number in version means dev environment
        private static bool GetIsProduction()
        {
            var version = VersionTracking.CurrentVersion;

            var lastDigit = version.Last();

            var versionInt = Convert.ToInt32(lastDigit);

            return versionInt % 2 == 0;
        }
    }
}
