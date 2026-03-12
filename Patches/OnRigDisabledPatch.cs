using HarmonyLib;
using UnityEngine;
using ZlothYNametag.EzVersionChecking;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
public class OnRigDisabledPatch
{
    private static void Prefix(VRRig __instance)
    {
        if (__instance.isLocal || VersionCheckingInitializer.VersionOutdated)
            return;
        
        Plugin.Log(
                $"Rig disabled called, removing rig");

        if (__instance.TryGetComponent(out FPSTag fpsTag)) Object.Destroy(fpsTag);
        if (__instance.TryGetComponent(out PlatformTag platformTag)) Object.Destroy(platformTag);
        if (__instance.TryGetComponent(out Nametag nametag)) Object.Destroy(nametag);
        if (__instance.TryGetComponent(out CosmeticIconTag cosmeticIconTag))
            Object.Destroy(cosmeticIconTag);
    }
}