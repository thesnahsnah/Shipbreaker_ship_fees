using Newtonsoft.Json;

namespace PayShips
{
    public class Settings
    {
        public bool enabled;
        public float cost_of_wrecks;
        public double cost_of_spares;
        public bool isFirstShiftWithCurrentShip;
        public bool debugLogChanges;

        public static Settings settings;

        public static void Load()
        {
            Ships_fee.LoggerInstance.LogInfo($"Ships_fee settings loading");
            var settingsText = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "settings.json"));
            settings = JsonConvert.DeserializeObject<Settings>(settingsText);

            settings.isFirstShiftWithCurrentShip = false;
            Ships_fee.LoggerInstance.LogInfo($"Ships_fee settings loaded");
        }
    }
}
