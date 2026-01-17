using System.Reflection;
using HarmonyLib;

namespace ZlothYNametag.Patches;

public class HarmonyPatches
{
    private const  string  InstanceId = Constants.PluginGuid;
    private static Harmony harmonyInstance;

    private static bool isPatched;

    internal static void ApplyHarmonyPatches()
    {
        if (isPatched)
            return;

        harmonyInstance ??= new Harmony(InstanceId);

        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        isPatched = true;
    }

    internal static void RemoveHarmonyPatches()
    {
        if (harmonyInstance == null || !isPatched)
            return;

        harmonyInstance.UnpatchSelf();
        isPatched = false;
    }
}