using HarmonyLib;
using ProjectOrbitalRing.Utils;
using System;

namespace ProjectOrbitalRing.Patches.Logic.OrbitalRing
{
    internal class UI
    {
        [HarmonyPatch(typeof(BuildPreview), nameof(BuildPreview.GetConditionText))]
        [HarmonyPostfix]
        public static void GetConditionTextPatch(BuildPreview __instance, EBuildCondition _condition, ref String __result)
        {
            if (_condition == (EBuildCondition)99) {
                __result = "同步轨道设施只能建设在特定位置".TranslateFromJson();
            } else if (_condition == (EBuildCondition)98) {
                __result = "同步轨道核心设施只能建设在对应基座上".TranslateFromJson();
            } else if (_condition == (EBuildCondition)97) {
                __result = "一个星环只能建造一座星环对撞机总控站".TranslateFromJson();
            } else if (_condition == (EBuildCondition)96) {
                __result = "电磁轨道弹射器只能建造在无大气星球".TranslateFromJson();
            } else if (_condition == (EBuildCondition)95) {
                __result = "不能低于16层".TranslateFromJson();
            } else if (_condition == (EBuildCondition)94) {
                __result = "低温工厂只能建造在极寒星球".TranslateFromJson();
            }
        }
    }
}
