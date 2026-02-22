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
                ((JArray)data["modVersionInfo"])!.FirstOrDefault(token => (string)token["modName"] ==
                                                                            Constants.PluginName);

        if (modVersionInfo != null)
        {
            LatestVersion = new Version(((string)modVersionInfo["latestVersion"])!);
            Version minimumVersion = new(((string)modVersionInfo["minimumVersion"])!);

            NotLatestMessage = (string)modVersionInfo["notLatestMessage"];
            OutdatedMessage  = (string)modVersionInfo["outdatedMessage"];

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
