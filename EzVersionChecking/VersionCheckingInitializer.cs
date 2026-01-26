using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ZlothYNametag.Console;

namespace ZlothYNametag.EzVersionChecking;

public static class VersionCheckingInitializer
{
    public static Action VersionOutdatedDetected;
    public static Action VersionNotLatestDetected;

    public static bool VersionOutdated;
    public static bool VersionNotLatest;

    public static string OutdatedMessage;
    public static string NotLatestMessage;

    public static Version LatestVersion;

    public static void StartVersionChecking()
    {
        JObject             data         = DataHamburburOrg.Data;

        JToken modVersionInfo =
                ((JArray)data["Mod Version Info"])!.FirstOrDefault(token => (string)token["Mod Name"] ==
                                                                            Constants.PluginName);

        if (modVersionInfo != null)
        {
            LatestVersion = new Version(((string)modVersionInfo["Latest Version"])!);
            Version minimumVersion = new(((string)modVersionInfo["Minimum Version"])!);

            NotLatestMessage = (string)modVersionInfo["Not Latest Message"];
            OutdatedMessage  = (string)modVersionInfo["Outdated Message"];

            Version localVersion = new(Constants.PluginVersion);

            if (localVersion < minimumVersion)
                VersionOutdated = true;
            else if (localVersion < LatestVersion)
                VersionNotLatest = true;
        }

        if (VersionOutdated)
            return;

        new GameObject($"{Constants.PluginName} Version Checking").AddComponent<ContinousVersionChecking>();
    }

}
