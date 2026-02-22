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
        
        Object.Destroy(vrrig.GetComponent<FPSTag>());
        Object.Destroy(vrrig.GetComponent<PlatformTag>());
        Object.Destroy(vrrig.GetComponent<Nametag>());
        Object.Destroy(vrrig.GetComponent<CosmeticIconTag>());
    }
}