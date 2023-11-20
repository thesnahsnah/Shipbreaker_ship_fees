using BBI.Unity.Game;
using BepInEx;
using HarmonyLib;
using static PayShips.Ships_fee_hooks;

namespace PayShips
{
    [BepInPlugin("Ships_fee", "A mod for Shipbreaker where Lynx charges for new wrecks", "0.5")]
    public class Ships_fee : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource LoggerInstance;

        void Awake()
        {
            LoggerInstance = Logger;
            LoggerInstance.LogInfo($"Ships_fee settings trying to load");

            // Plugin startup logic
            Settings.Load();
            if (Settings.settings.enabled)
            {
                new Harmony("Ships_fee").PatchAll();
                LoggerInstance.LogInfo($"Ships_fee is patched");
                LoggerInstance.LogInfo($"Ships_fee is loaded!");
            }
            else
            {
                LoggerInstance.LogInfo($"Ships_fee is disabled");
            }
        }
    }
}
