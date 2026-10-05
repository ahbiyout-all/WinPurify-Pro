using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    public class CustomProfile
    {
        public string Name { get; set; } = "My Profile";
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> SelectedTaskIds { get; set; } = new();
    }

    public static class ProfilePresetManager
    {
        private static readonly string ProfilesDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinPurifyPro",
            "Profiles"
        );

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        static ProfilePresetManager()
        {
            try
            {
                if (!Directory.Exists(ProfilesDir))
                {
                    Directory.CreateDirectory(ProfilesDir);
                }
            }
            catch { }
        }

        public static async Task<bool> SaveProfileAsync(CustomProfile profile)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string safeName = string.Join("_", profile.Name.Split(Path.GetInvalidFileNameChars()));
                    string file = Path.Combine(ProfilesDir, $"{safeName}.json");
                    string json = JsonSerializer.Serialize(profile, Options);
                    File.WriteAllText(file, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public static async Task<List<CustomProfile>> LoadAllProfilesAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<CustomProfile>();
                try
                {
                    if (!Directory.Exists(ProfilesDir)) return list;

                    foreach (var file in Directory.GetFiles(ProfilesDir, "*.json"))
                    {
                        string json = File.ReadAllText(file);
                        var profile = JsonSerializer.Deserialize<CustomProfile>(json, Options);
                        if (profile != null) list.Add(profile);
                    }
                }
                catch { }
                return list;
            });
        }
    }
}
