using HarmonyLib;
using UnityEngine;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetColor))]
public class SetColourPatch
{
    private static void Postfix(VRRig __instance, Color color)
    {
        if (__instance.isLocal || Plugin.Instance.OutdatedVersion)
            return;

        Plugin.Log($"Rig update colour, removing rig for {__instance.creator.SanitizedNickName}");

        __instance.GetOrAddComponent(out Nametag nametag);
        nametag.UpdateColour(color);

        if (!__instance.TryGetComponent(out FPSTag _)) __instance.AddComponent<FPSTag>();
        if (!__instance.TryGetComponent(out PlatformTag _)) __instance.AddComponent<PlatformTag>();
    }
}