using HarmonyLib;
using static ProjectOrbitalRing.ProjectOrbitalRing;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using UnityEngine;
using ProjectOrbitalRing.Patches.Logic.AddVein;


namespace ProjectOrbitalRing.Patches.Logic
{
    internal class GammaPowerSavingPatch
    {
        private static readonly long GAMMA_GenEnergyPerTick = 80000000; // 4800MW
        private static void GammaPowerSavingCalculate(PowerNetwork powerNetwork, ref PowerGeneratorComponent PowerGenerator)
        {
            if (PowerGenerator.gamma) {
                if (PowerGenerator.productId == 0) {
                    float num = (float)Cargo.accTableMilli[PowerGenerator.catalystIncLevel];
                    //PowerGenerator.genEnergyPerTick = (long)(generaterRatio * ((double)GAMMA_GenEnergyPerTick / (PowerGenerator.currentStrength * (1f + PowerGenerator.warmup * 1.5f) * ((PowerGenerator.catalystPoint > 0) ? (2f * (1f + num)) : 1f))) + 0.99999);
                    PowerGenerator.genEnergyPerTick = (long)(powerNetwork.generaterRatio * ((double)GAMMA_GenEnergyPerTick) + 0.99999);
                    //LogError($"PowerGenerator.genEnergyPerTick {PowerGenerator.genEnergyPerTick} generaterRatio {powerNetwork.generaterRatio}");
                } else {
                    PowerGenerator.genEnergyPerTick = GAMMA_GenEnergyPerTick;
                }
            }
        }

        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNetwork), nameof(PowerNetwork.generaterRatio)))
                );

            object powerNetwork = matcher.Operand;

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.wind))),
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldc_R4, 0.7f)
                );

            object gamma = matcher.Operand;

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc, powerNetwork),
                new CodeInstruction(OpCodes.Ldloc, gamma),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GammaPowerSavingPatch), nameof(GammaPowerSavingCalculate)))
                );


            //matcher.LogInstructionEnumeration();
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Gamma))]
        [HarmonyPostfix]
        public static void PowerGeneratorComponent_EnergyCap_Gamma_Patch(ref PowerGeneratorComponent __instance, float response, ref long __result)
        {
            float num2 = (float)Cargo.accTableMilli[__instance.catalystIncLevel];
            long temp = (long)(__instance.currentStrength * (1f + __instance.warmup * 1.5f) * ((__instance.catalystPoint > 0) ? (2f * (1f + num2)) : 1f) * (float)GAMMA_GenEnergyPerTick);
            temp = (long)((double)temp * (double)response);
            if (__instance.productId == 0) {
                __result = temp;
            }
        }

        // 数学率引擎的锅不生效，防止功率显示为负数
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), "RequestDysonSpherePower")]
        public static bool RequestDysonSpherePowerPrePatch(ref PowerSystem __instance)
        {
            PlanetFactory factory = __instance.factory;
            return BanPowerGenGammaReq(factory);
        }

        [HarmonyPatch(typeof(GameLogic), nameof(GameLogic._power_gen_gamma_parallel))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GameLogic_power_gen_gamma_parallel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameLogic), nameof(GameLogic.factories))));

            object planetFactory = matcher.Advance(3).Operand; // 变量索引

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.gamma))));

            object IL_01E9 = matcher.Advance(1).Operand; // 变量索引

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, planetFactory),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GammaPowerSavingPatch), nameof(BanPowerGenGammaReq))),
                new CodeInstruction(OpCodes.Brfalse_S, IL_01E9)
            );

            return matcher.InstructionEnumeration();
        }

        public static bool BanPowerGenGammaReq(PlanetFactory factory)
        {
            int starIndex = factory.planetId / 100 - 1;
            if (GameMain.galaxy.stars[starIndex].type != EStarType.BlackHole) {
                return true;
            } else {
                if (ProjectOrbitalRing.MoreMegaStructureCompatibility) {
                    try {
                        // 使用反射动态获取类型
                        var mmType = Type.GetType("MoreMegaStructure.MoreMegaStructure, MoreMegaStructure");
                        var starMegaType = mmType?.GetField("StarMegaStructureType")?.GetValue(null) as int[];

                        if (starMegaType?[starIndex] != 0) {
                            return true;
                        }
                    } catch (Exception ex) {
                        // ignored
                    }
                }
                return false;
            }
        }
    }
}
