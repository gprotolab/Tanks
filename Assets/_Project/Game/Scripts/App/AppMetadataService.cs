using System;
using UnityEngine;

namespace Game.App
{
    public class AppMetadataService
    {
        private const string FirstLaunchKey = "app.first_launch_done";
        private const string InstallDateKey = "app.install_date";
        private const string SessionCountKey = "app.session_count";

        public bool IsFirstLaunch => !PlayerPrefs.HasKey(FirstLaunchKey);

        public string InstallDateUtc => PlayerPrefs.GetString(InstallDateKey, string.Empty);

        public int SessionCount => PlayerPrefs.GetInt(SessionCountKey, 0);

        public void RecordFirstLaunch()
        {
            if (!IsFirstLaunch)
            {
                return;
            }

            PlayerPrefs.SetString(FirstLaunchKey, "1");
            PlayerPrefs.SetString(InstallDateKey, DateTime.UtcNow.ToString("O"));
            PlayerPrefs.Save();
        }

        public int IncrementSessionCount()
        {
            int sessionCount = SessionCount + 1;
            PlayerPrefs.SetInt(SessionCountKey, sessionCount);
            PlayerPrefs.Save();
            return sessionCount;
        }
    }
}