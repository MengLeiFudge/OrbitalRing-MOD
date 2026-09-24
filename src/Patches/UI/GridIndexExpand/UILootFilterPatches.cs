using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using static ProjectOrbitalRing.ProjectOrbitalRing;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectOrbitalRing.Patches.UI
{
    public static class UILootFilterPatches
    {
        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UILootFilter), nameof(UILootFilter.currentType));

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnCreate))]
        [HarmonyPostfix]
        public static void Create(UILootFilter __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();
            var index = 1;

            foreach (TabData tabData in allTabs)
            {
                if (tabData == null) continue;

                index = tabData.tabIndex - 1;
                GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.filterTrans, false);

                ((RectTransform)gameObject.transform).anchoredPosition = new Vector2(index * 70 - 54, -72f);
                UITabButton component = gameObject.GetComponent<UITabButton>();
                Sprite newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                if (index == 2) {
                    component.Init(newIcon, tabData.tabName, index + 5, __instance.OnTypeButtonClick);
                } else {
                    component.Init(newIcon, tabData.tabName, index, __instance.OnTypeButtonClick);
                }
                _tabs.Add(component);
            }
            __instance.typeButton2.transform.localPosition = new Vector3((index + 1) * 70 - 54, -40, 0);
            __instance.typeButton3.transform.localPosition = new Vector3((index + 2) * 70 - 54, -40, 0);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnTypeButtonClick))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void OnTypeClicked_Prefix(int type) => UILootFilter.showAll = type == 2;

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnTypeButtonClick))]
        [HarmonyPostfix]
        public static void OnTypeClicked_Postfix(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyPostfix]
        public static void RefreshIcons_Postfix(UILootFilter __instance)
        {
            if (__instance.currentType == 7) {
                __instance.currentType = 2;
                int num = 8;
                GameHistoryData history = __instance.gameData.history;
                ItemProto[] dataArray = LDB.items.dataArray;
                IconSet iconSet = GameMain.iconSet;
                for (int i = 0; i < dataArray.Length; i++) {
                    if (dataArray[i].GridIndex >= 1101) {
                        int num8 = dataArray[i].GridIndex / 1000;
                        if (num8 == __instance.currentType + 1) {
                            if (UILootFilter.showAll || history.enemyDropItemUnlocked.Contains(dataArray[i].ID) || history.ItemUnlocked(dataArray[i].ID)) {
                                int num9 = (dataArray[i].GridIndex - num8 * 1000) / 100 - 1;
                                int num10 = dataArray[i].GridIndex % 100 - 1;
                                if (num9 >= 0 && num10 >= 0 && num9 < num && num10 < 14) {
                                    int num11 = num9 * 14 + num10;
                                    if (num11 >= 0 && num11 < __instance.indexArray.Length) {
                                        uint num12 = (!__instance.pickFilters.ContainsKey(dataArray[i].ID)) ? 1U : ((__instance.pickFilters[dataArray[i].ID] == 0) ? 0U : 2U);
                                        __instance.indexArray[num11] = iconSet.itemIconIndex[dataArray[i].ID];
                                        __instance.stateArray[num11] = num12;
                                        __instance.protoArray[num11] = dataArray[i];
                                        if (num12 == 2U) {
                                            __instance.ActiveGridText(num11);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                __instance.currentType = 7;
            }
        }

        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnUpdate))]
        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RepositionGridText))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    return matcher.InstructionEnumeration();
        //}

        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> RefreshIcons_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
        //    matcher.SetOperandAndAdvance((sbyte)17);

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
        //    matcher.SetOperandAndAdvance((sbyte)17); 

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
        //    matcher.SetOperandAndAdvance((sbyte)17);

        //    return matcher.InstructionEnumeration();
        //}

        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.TestMouseIndex))]
        //[HarmonyTranspiler]
        //[HarmonyPriority(Priority.Last)]
        //public static IEnumerable<CodeInstruction> TestMouseIndex_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

        //    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //       .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

        //    return matcher.InstructionEnumeration();
        //}

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnBoxMouseDown))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_OnBoxMouseDown_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            //matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).SetOpcodeAndAdvance(OpCodes.Beq_S);
            matcher.Advance(2).SetOpcodeAndAdvance(OpCodes.Beq_S);

            //matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_7).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.TestMouseIndex))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetMaterialProps))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_SetMaterialProps_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(2).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);
            //matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_7).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshIcons_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(2).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);
            //matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_7).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(2);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));
            matcher.Advance(2).SetOpcodeAndAdvance(OpCodes.Beq_S);
            //matcher.Advance(1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_7).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> UILootFilter_RefreshWindow_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);

        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 692f), new CodeMatch(OpCodes.Ldc_R4, 536f));

        //    matcher.SetOperandAndAdvance(830f).SetOperandAndAdvance(500f);

        //    return matcher.InstructionEnumeration();
        //}

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyPostfix]
        public static void RefreshWindow_Postfix(UILootFilter __instance)
        {
            //__instance.contentTrans.sizeDelta = __instance.currentType == 1 ? new Vector2(644f, 414f) : new Vector2(782f, 322f);

            bool show = !__instance.showDropOnly;

            foreach (UITabButton uiTabButton in _tabs) uiTabButton.gameObject.SetActive(show);
        }

        //[HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetMaterialProps))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> UILootFilter_SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);
        //    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_8));
        //    matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_7);

        //    //matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 14f));

        //    //matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        //    //   .SetInstructionAndAdvance(
        //    //        Transpilers.EmitDelegate<Func<UILootFilter, float>>(filter => filter.currentType == 1 ? 14f : 17f));

        //    return matcher.InstructionEnumeration();
        //}
    }
}
