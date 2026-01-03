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

        __instance.GetOrAddComponent(out Nametag nametag);
        nametag.UpdateColour(color);

        __instance.GetOrAddComponent<FPSTag>(out FPSTag _);
        __instance.GetOrAddComponent<PlatformTag>(out PlatformTag _);
        __instance.GetOrAddComponent<CosmeticIconTag>(out CosmeticIconTag _);
    }
}