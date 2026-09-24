using HarmonyLib;
using ProjectOrbitalRing.Utils;

namespace ProjectOrbitalRing.Patches.Logic.BattleRelated
{
    internal class LocalPlasmaBanAntimatter
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemProto), "GetPropValue")]
        public static void GetPropValuePatch(ItemProto __instance, int index, ref string __result)
        {
            if (__instance.DescFields[index] == 52)
            {
                var type = __instance.prefabDesc.isTurret ? __instance.prefabDesc.turretAmmoType : __instance.AmmoType;
                if (type == EAmmoType.Plasma || type == EAmmoType.LocalPlasma)
                {
                    __result = "轨道弹".TranslateFromJson();
                }
            }
        }
    }
}
