using UnityEngine;

namespace SlimUI.ModernMenu
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;
        public static SettingsManager Instance
        {
            get { return instance; }
        }

        // Video Settings
        public bool IsFullscreen { get; private set; }
        public bool VSync { get; private set; }
        public bool MotionBlur { get; private set; }
        public bool AmbientOcclusion { get; private set; }
        public int ShadowQuality { get; private set; } // 0: Off, 1: Low, 2: High
        public int TextureQuality { get; private set; } // 0: Low, 1: Medium, 2: High

        // Game Settings
        public bool IsNormalDifficulty { get; private set; }

        // Control Settings
        public float MouseSensitivityX { get; private set; }
        public float MouseSensitivityY { get; private set; }
        public float MouseSmoothing { get; private set; }
        public float MusicVolume { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSettings();
        }

        private void LoadAllSettings()
        {
            // Load settings from PlayerPrefs
            IsFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            VSync = PlayerPrefs.GetInt("VSync", 1) == 1;
            MotionBlur = PlayerPrefs.GetInt("MotionBlur", 0) == 1;
            AmbientOcclusion = PlayerPrefs.GetInt("AmbientOcclusion", 0) == 1;
            ShadowQuality = PlayerPrefs.GetInt("Shadows", 1);
            TextureQuality = PlayerPrefs.GetInt("Textures", 1);

            IsNormalDifficulty = PlayerPrefs.GetInt("NormalDifficulty", 1) == 1;

            MouseSensitivityX = PlayerPrefs.GetFloat("XSensitivity", 2.0f);
            MouseSensitivityY = PlayerPrefs.GetFloat("YSensitivity", 2.0f);
            MouseSmoothing = PlayerPrefs.GetFloat("MouseSmoothing", 0.05f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);

            ApplySettings();
        }

        public void ApplySettings()
        {
            // Apply video settings
            Screen.fullScreen = IsFullscreen;
            QualitySettings.vSyncCount = VSync ? 1 : 0;

            // Apply shadow settings
            switch (ShadowQuality)
            {
                case 0:
                    QualitySettings.shadowCascades = 0;
                    QualitySettings.shadowDistance = 0;
                    break;
                case 1:
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.shadowDistance = 75;
                    break;
                case 2:
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 500;
                    break;
            }

            // Apply texture settings
            QualitySettings.globalTextureMipmapLimit = 2 - TextureQuality; // 0=High, 2=Low

            // Other settings like motion blur, ambient occlusion, etc., can be applied dynamically in the game.
        }

        public void SaveSetting(string key, object value)
        {
            switch (value)
            {
                case bool boolValue:
                    PlayerPrefs.SetInt(key, boolValue ? 1 : 0);
                    break;
                case int intValue:
                    PlayerPrefs.SetInt(key, intValue);
                    break;
                case float floatValue:
                    PlayerPrefs.SetFloat(key, floatValue);
                    break;
            }

            PlayerPrefs.Save();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            IsFullscreen = isFullscreen;
            SaveSetting("Fullscreen", isFullscreen);
            Screen.fullScreen = isFullscreen;
        }

        public void SetVSync(bool enableVSync)
        {
            VSync = enableVSync;
            SaveSetting("VSync", enableVSync);
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        }

        public void SetMouseSensitivity(float x, float y)
        {
            MouseSensitivityX = x;
            MouseSensitivityY = y;
            SaveSetting("XSensitivity", x);
            SaveSetting("YSensitivity", y);
        }

        public void SetMouseSmoothing(float smoothing)
        {
            MouseSmoothing = smoothing;
            SaveSetting("MouseSmoothing", smoothing);
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
            SaveSetting("MusicVolume", volume);
        }

        public void SetShadowQuality(int quality)
        {
            ShadowQuality = quality;
            SaveSetting("Shadows", quality);
            ApplySettings();
        }

        public void SetTextureQuality(int quality)
        {
            TextureQuality = quality;
            SaveSetting("Textures", quality);
            ApplySettings();
        }

        public void SetDifficulty(bool isNormal)
        {
            IsNormalDifficulty = isNormal;
            SaveSetting("NormalDifficulty", isNormal);
        }
        public void ApplyMusicVolume(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.volume = MusicVolume;
            }
        }
    }
}
