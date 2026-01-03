using HarmonyLib;
using UnityEngine;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.RemoveRigFromGorillaParent))]
public class RigCachedPatch
{
    private static void Postfix(NetPlayer player, VRRig vrrig)
    {
        if (Plugin.Instance.OutdatedVersion)
            return;
        
        Object.Destroy(vrrig.GetComponent<FPSTag>());
        Object.Destroy(vrrig.GetComponent<PlatformTag>());
        Object.Destroy(vrrig.GetComponent<Nametag>());
        Object.Destroy(vrrig.GetComponent<CosmeticIconTag>());
    }
}