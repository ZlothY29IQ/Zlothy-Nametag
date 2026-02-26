using HarmonyLib;
using UnityEngine;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.RemoveRigFromGorillaParent))]
public class RigCachedPatch
{
    private static void Prefix(NetPlayer player, VRRig vrrig)
    {
        if (Plugin.Instance.OutdatedVersion)
            return;

        Plugin.Log($"Rig cached called, removing rig for {player.SanitizedNickName}");

        if (vrrig.TryGetComponent(out FPSTag fpsTag)) Object.Destroy(fpsTag);
        if (vrrig.TryGetComponent(out PlatformTag platformTag)) Object.Destroy(platformTag);
        if (vrrig.TryGetComponent(out Nametag nametag)) Object.Destroy(nametag);
        if (vrrig.TryGetComponent(out CosmeticIconTag cosmeticIconTag)) Object.Destroy(cosmeticIconTag);
    }
}