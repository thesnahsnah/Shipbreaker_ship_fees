using UnityEngine;
using HarmonyLib;
using BBI.Unity.Game;
using TMPro;
using Carbon.Core.Events;

namespace PayShips
{
    public class Ships_fee_hooks
    {
        [HarmonyPatch]
        public class ShiftCycleController_ReportOnlineStatistic
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(ShiftCycleController), "ReportOnlineStatistics")]
            public static void ReportOnlineStatistics(
              object instance,
              float debtRemain,
              float totalShiftExpenses,
              float valueEarned)
            {
                // its a stub so it has no initial content
                throw new NotImplementedException("It's a stub");
            }
        }
        [HarmonyPatch(typeof(ShiftCycleController), "RefreshEntries")]
        public class ShiftCycleController_RefreshEntries
        {
            [HarmonyPrefix]
            public static bool prefix(
                ref ShiftCycleController __instance,
                ref GameObject ___m_DeductionsEntryPrefab,
                ref Transform ___m_DeductionsList,
                ref TextMeshProUGUI ___m_TotalDeductionsText,
                ref TextMeshProUGUI ___m_NewDebtText,
                ref TextMeshProUGUI ___m_SalvageEarnedText,
                ref TextMeshProUGUI ___m_SalvageDollarSignText,
                ref LocalizedTextMeshProUGUI ___m_SalvageTitleText
                )
            {
                PlayerProfile profile = PlayerProfileService.Instance.Profile;
                double num1 = 0.0;

                //
                //changes
                //

                double wreckcost = 0.0;

                ShipPreview currentShipPreview = ModuleService.Instance?.CurrentShipPreview;
                if (currentShipPreview != null && PayShips.Settings.settings.isFirstShiftWithCurrentShip)
                {
                    wreckcost = (currentShipPreview.TotalCreditValue) * (PayShips.Settings.settings.cost_of_wrecks);
                    PayShips.Settings.settings.isFirstShiftWithCurrentShip = false;
                }

                PayShips.Ships_fee.LoggerInstance.LogInfo($"wreckcost is {wreckcost}");

                if (wreckcost > 0.0)
                {
                    num1 += wreckcost;
                    PostMissionDeductionUIEntry component = UnityEngine.Object.Instantiate<GameObject>(___m_DeductionsEntryPrefab, ___m_DeductionsList).GetComponent<PostMissionDeductionUIEntry>();
                    if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                    {
                        component.Initialize("new ship fee", wreckcost.ToString("N"), (PayShips.Settings.settings.cost_of_wrecks * 100).ToString() + "%");
                    }
                }

                //
                //changes end here
                //

                int numDeaths = GameSession.NumDeaths;
                if (numDeaths > 0)
                {
                    ShiftCycleCostAsset.ShiftCycleCostData data = profile.DifficultyMode.ClonesCostAsset.Data;
                    double num2 = 0.0;
                    if (data.Costs != null && data.Costs.Count > 0)
                        num2 = Main.Instance.MainSettings.SessionSettings.PenalizeDeath ? PayShips.Settings.settings.cost_of_spares : 0.0;
                    double num3 = (double)numDeaths * num2;
                    num1 += num3;
                    PostMissionDeductionUIEntry component = UnityEngine.Object.Instantiate<GameObject>(___m_DeductionsEntryPrefab, ___m_DeductionsList).GetComponent<PostMissionDeductionUIEntry>();
                    if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                    {
                        if (numDeaths == 0)
                        {
                            string empty = string.Empty;
                        }
                        else
                            numDeaths.ToString();
                        component.Initialize(data.Name, num3.ToString("N"), numDeaths.ToString());
                    }
                }
                DebtInterestAsset debtInterestAsset = profile.DifficultyMode.DebtInterestAsset;
                CurrencyInstance currency = profile.CurrencyController.GetCurrency(debtInterestAsset.Data.Currency.ID);
                double num4 = profile.DifficultyMode.StartingDebtAmount - (double)currency.Amount;
                int num5;
                if (!profile.CertificationTiers.TryGetValue(debtInterestAsset.Data.CertTypeForCostReduction, out num5))
                    num5 = 0;
                num5 = Mathf.Min(debtInterestAsset.Data.InterestRatePercents.Count - 1, num5);
                if (num5 >= 0)
                {
                    PostMissionDeductionUIEntry component = UnityEngine.Object.Instantiate<GameObject>(___m_DeductionsEntryPrefab, ___m_DeductionsList).GetComponent<PostMissionDeductionUIEntry>();
                    if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                    {
                        double num6 = Math.Max(0.0, debtInterestAsset.Data.InterestRatePercents[num5] / 100.0 * num4);
                        component.Initialize(debtInterestAsset.Data.Name, num6.ToString("N"), debtInterestAsset.Data.InterestRatePercents[num5].ToString() + "%");
                        num1 += num6;
                    }
                }
                foreach (ShiftCycleCostAsset shiftCycleCost in profile.DifficultyMode.ShiftCycleRentalCostAssets.GetShiftCycleCosts())
                {
                    int num7;
                    if (!profile.CertificationTiers.TryGetValue(shiftCycleCost.Data.CertTypeForCostReduction, out num7))
                        num7 = 0;
                    num7 = Mathf.Min(shiftCycleCost.Data.Costs.Count - 1, num7);
                    if (num7 >= 0)
                    {
                        num1 += shiftCycleCost.Data.Costs[num7];
                        PostMissionDeductionUIEntry component = UnityEngine.Object.Instantiate<GameObject>(___m_DeductionsEntryPrefab, ___m_DeductionsList).GetComponent<PostMissionDeductionUIEntry>();
                        if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                            component.Initialize(shiftCycleCost.Data.Name, shiftCycleCost.Data.Costs[num7].ToString("N"), string.Empty);
                    }
                }
                ___m_TotalDeductionsText.text = num1.ToString("N");
                Main.EventSystem.Post((EventBase)CurrencyChangedEvent.Subtract(currency.CurrencyAssetID, (float)num1));
                double num8 = profile.DifficultyMode.StartingDebtAmount - (double)currency.Amount;
                ___m_NewDebtText.SetText(num8.ToString("N"));
                if ((double)profile.PreviousShiftEarnings != 0.0)
                {
                    ___m_SalvageEarnedText.alpha = 1f;
                    ___m_SalvageDollarSignText.alpha = 1f;
                    ___m_SalvageTitleText.TMProText.alpha = 1f;
                    ___m_SalvageEarnedText.text = profile.PreviousShiftEarnings.ToString("N");
                }
                else
                {
                    ___m_SalvageEarnedText.alpha = 0.0f;
                    ___m_SalvageDollarSignText.alpha = 0.0f;
                    ___m_SalvageTitleText.TMProText.alpha = 0.0f;
                }
                if (PlaystreamService.IsInitialized)
                    PlaystreamService.Instance.ReportTransactionEvent(currency.Name, -num1, -num8, "ShiftComplete", "ShiftRewards");
                ShiftCycleController_ReportOnlineStatistic.ReportOnlineStatistics(__instance, currency.Amount, (float)num1, profile.PreviousShiftEarnings);

                return false;
            }
        }

        [HarmonyPatch(typeof(JobBoardScreenController), "ClaimNewShipButtonClicked")]
        public class JobBoardScreenController_ClaimNewShipButtonClicked
        {
            [HarmonyPostfix]
            public static void postfix()
            {
                PayShips.Settings.settings.isFirstShiftWithCurrentShip = true;

                PayShips.Ships_fee.LoggerInstance.LogInfo($"buying new ship");
            }
        }

        [HarmonyPatch(typeof(JobBoardScreenController), "ClaimShipCancelButtonClicked")]
        public class JobBoardScreenController_ClaimShipCancelButtonClicked
        {
            [HarmonyPostfix]
            public static void postfix()
            {
                PayShips.Settings.settings.isFirstShiftWithCurrentShip = false;
                PayShips.Ships_fee.LoggerInstance.LogInfo($"cancelled buying new ship");
            }
        }
        [HarmonyPatch(typeof(SceneLoader), "TearDownAndLoadFrontEndAsync_Internal")]
        public class SceneLoader_TearDownAndLoadFrontEndAsync_Internal
        {

            [HarmonyPostfix]
            public static void postfix()
            {
                PayShips.Settings.settings.isFirstShiftWithCurrentShip = false;
                PayShips.Ships_fee.LoggerInstance.LogInfo($"cancelled buying new ship");
            }
        }

    }
}
