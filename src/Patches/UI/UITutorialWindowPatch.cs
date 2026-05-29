using HarmonyLib;
using UnityEngine.UI;

namespace ProjectOrbitalRing.Patches.UI
{
    internal class UITutorialWindowPatch
    {
        /// 以下复制自万物分馏，感谢萌泪佬（@MoeLei）的帮助！
        /// <summary>
        /// 在指引窗口打开时，将左侧区域的垂直滚动条设为可见并添加事件监听器。
        /// 感谢海星佬（@starfi5h）的帮助！
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITutorialWindow), nameof(UITutorialWindow._OnOpen))]
        private static void UITutorialWindow_OnOpen_Postfix(UITutorialWindow __instance)
        {
            if (!__instance.entryList.VertScroll) {
                __instance.entryList.VertScroll = true;
                __instance.entryList.m_ScrollRect.vertical = true;
                __instance.entryList.m_ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                // Trigger ScrollRect.OnEnable() to add listeners
                __instance.entryList.m_ScrollRect.enabled = false;
                __instance.entryList.m_ScrollRect.enabled = true;
            }
        }
    }
}
