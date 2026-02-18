using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ZlothYNametag.EzVersionChecking;

public class ContinousVersionChecking : MonoBehaviour
{
    private const float CheckIntervalSeconds = 60f;

    private IEnumerator Start()
    {
        float lastCheckTime = 0f;

        while (!VersionCheckingInitializer.VersionOutdated)
        {
            yield return null;

            if (Time.time - lastCheckTime < CheckIntervalSeconds)
                continue;

            lastCheckTime = Time.time;

            using UnityWebRequest request = UnityWebRequest.Get("https://data.hamburbur.org/");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                continue;

            try
            {
                JObject data = JObject.Parse(request.downloadHandler.text);

                JToken modVersionInfo = ((JArray)data["modVersionInfo"])!
                       .FirstOrDefault(token => (string)token["modName"] == Constants.PluginName);

                if (modVersionInfo == null)
                    continue;

                Version latestVersion  = new((string)modVersionInfo["latestVersion"]!);
                Version minimumVersion = new((string)modVersionInfo["minimumVersion"]!);
                Version localVersion   = new(Constants.PluginVersion);

                VersionCheckingInitializer.LatestVersion    = latestVersion;
                VersionCheckingInitializer.NotLatestMessage = (string)modVersionInfo["notLatestMessage"];
                VersionCheckingInitializer.OutdatedMessage  = (string)modVersionInfo["outdatedMessage"];

                if (localVersion < minimumVersion)
                {
                    VersionCheckingInitializer.VersionOutdated = true;
                    VersionCheckingInitializer.VersionOutdatedDetected?.Invoke();
                    Destroy(gameObject);

                    yield break;
                }

                if (localVersion < latestVersion &&
                    !VersionCheckingInitializer.VersionNotLatest)
                {
                    VersionCheckingInitializer.VersionNotLatest = true;
                    VersionCheckingInitializer.VersionNotLatestDetected?.Invoke();
                }
            }
            catch
            {
                // ignored
            }
        }
    }

}
