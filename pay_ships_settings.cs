using Newtonsoft.Json;

namespace PayShips
{
    public class Settings
    {
        public bool enabled;
        public float cost_of_destroyed_salvage;
        public float cost_of_abandoned_salvage;
        public float profit_share_of_salvage;
        public double cost_of_spares;

        public static Settings settings;

        public static void Load()
        {
            Ships_fee.LoggerInstance.LogInfo($"Ships_fee settings loading");
            var settingsText = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Shipbreaker_ShipFee_settings.json"));
            settings = JsonConvert.DeserializeObject<Settings>(settingsText);

            Ships_fee.LoggerInstance.LogInfo($"Ships_fee settings loaded");
        }
    }

    public class Data
    {
        public static double salvageAbandonedCost = 0.0d;
        public static double salvageDestroyedCost = 0.0d;
    }
}
