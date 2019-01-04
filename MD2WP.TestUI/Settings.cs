using System.IO;
using System.Windows;
using MD2WP.Shared.Helpers;
using Newtonsoft.Json;

namespace MD2WP.TestUI
{
    public class Settings
    {
        private const string SettingsFilename = "MD2WP.Settings.json";

        public string SiteUrl { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string VSTSAccount { get; set; }

        public string ProjectName { get; set; }

        public string RepoName { get; set; }

        public string Branch { get; set; }

        public string PAT { get; set; }

        public string MetadataFile { get; set; }

        public bool EmbedExternalImages { get; set; }

        public static Settings GetSettings(string passKey)
        {
            Settings settings = null;

            if (File.Exists(SettingsFilename))
            {
                var json = File.ReadAllText(SettingsFilename);

                if (!json.StartsWith("{"))
                {
                    if (string.IsNullOrEmpty(passKey))
                    {
                        // Settings are encrypted and no pass key was provided
                        MessageBox.Show("Your saved settings are encrypted. To restore your saved settings, enter your Pass Key and click Load.",
                            "Settings Encrypted", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Return default settings
                        settings = new Settings();
                    }
                    else
                    {
                        // We have a passkey
                        json = EncryptionHelper.DecryptStringAES(json, passKey);
                    }
                }

                if (settings == null)
                {
                    settings = JsonConvert.DeserializeObject<Settings>(json);
                }
            }
            else
            {
                settings = new Settings();
            }

            return settings;
        }

        public void SaveSettings(string passKey)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);

            // If a pass key is provided, let's encrypt!
            if (!string.IsNullOrEmpty(passKey))
            {
                json = EncryptionHelper.EncryptStringAES(json, passKey);
            }

            File.WriteAllText(SettingsFilename, json);
        }
    }
}