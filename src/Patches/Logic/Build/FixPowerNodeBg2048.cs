using HarmonyLib;
using PowerNetworkStructures;

namespace ProjectOrbitalRing.Patches.Logic.Build
{
    internal class FixPowerNodeBg2048
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.line_arragement_for_add_node))]
        public static void PowerSystem_line_arragement_for_add_node_Patch(PowerSystem __instance, Node node)
        {
            if (node.conns.Count >= 2048) {
                __instance.tmp_state = new int[4096];
            }
        }
    }
}
