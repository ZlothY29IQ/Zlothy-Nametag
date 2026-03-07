using HarmonyLib;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRig))]
internal static class RigCosmeticsLoadedPatch
{
    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPostfix]
    private static void OnGetRigCosmetics(VRRig __instance)
    {
        if (!__instance.TryGetComponent(out CosmeticIconTag iconTag))
            iconTag = __instance.AddComponent<CosmeticIconTag>();

        iconTag.OnCosmeticsLoaded();
    }
}