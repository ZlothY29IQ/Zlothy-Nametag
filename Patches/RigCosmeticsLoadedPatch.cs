using HarmonyLib;
using ZlothYNametag.Tags;

namespace ZlothYNametag.Patches;

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics", MethodType.Normal)]
public static class RigCosmeticsLoadedPatch
{
    private static void Postfix(VRRig __instance)
    {
        if (!__instance.TryGetComponent(out CosmeticIconTag iconTag))
            iconTag = __instance.AddComponent<CosmeticIconTag>();

        iconTag.OnCosmeticsLoaded();
    }
}