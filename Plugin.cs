using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using BepInEx;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using ZlothYNametag.Console;
using ZlothYNametag.Patches;
using ZlothYNametag.Tags;

namespace ZlothYNametag;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    // ReSharper disable InconsistentNaming
    public static Transform firstPersonCameraTransform;
    public static Transform thirdPersonCameraTransform;

    public static TMP_FontAsset comicSans;

    private void Start()
    {
        HarmonyPatches.ApplyHarmonyPatches();

        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
    }

    private void OnGameInitialized()
    {
        using HttpClient httpClient = new();
        HttpResponseMessage gorillaInfoEndPointResponse =
                httpClient.GetAsync(
                        "https://raw.githubusercontent.com/HanSolo1000Falcon/GorillaInfo/main/KnownCheats.txt").Result;

        using (Stream gorillaInfoStream = gorillaInfoEndPointResponse.Content
                                                                     .ReadAsStreamAsync().Result)
        {
            using (StreamReader reader = new(gorillaInfoStream))
            {
                CosmeticIconTag.cheaterProps =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            }
        }

        firstPersonCameraTransform = GorillaTagger.Instance.mainCamera.transform;
        thirdPersonCameraTransform = GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0);

        Stream stream = Assembly.GetExecutingAssembly()
                                .GetManifestResourceStream("ZlothYNametag.Resources.fpsnametagsforzlothy");

        AssetBundle bundle = AssetBundle.LoadFromStream(stream);
        // ReSharper disable once PossibleNullReferenceException
        stream.Close();

        comicSans = Instantiate(bundle.LoadAsset<TMP_FontAsset>("COMICBD SDF"));
        // ReSharper disable once ShaderLabShaderReferenceNotResolved
        comicSans.material.shader = Shader.Find("TextMeshPro/Mobile/Distance Field");

        Hashtable properties = new()
        {
                {
                        "FPS-Nametags for Zlothy",
                        $"Made by HanSolo1000Falcon & ZlothY - Version {Constants.PluginVersion}"
                },
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        string     ConsoleGUID   = "goldentrophy_Console";
        GameObject ConsoleObject = GameObject.Find(ConsoleGUID);

        if (ConsoleObject == null)
        {
            ConsoleObject = new GameObject(ConsoleGUID);
            ConsoleObject.AddComponent<Console.Console>();
        }
        else
        {
            if (ConsoleObject.GetComponents<Component>()
                             .Select(c => c.GetType().GetField("ConsoleVersion",
                                             BindingFlags.Public |
                                             BindingFlags.Static |
                                             BindingFlags.FlattenHierarchy))
                             .Where(f => f != null && f.IsLiteral && !f.IsInitOnly)
                             .Select(f => f.GetValue(null))
                             .FirstOrDefault() is string consoleVersion)
                if (ServerData.VersionToNumber(consoleVersion) <
                    ServerData.VersionToNumber(Console.Console.ConsoleVersion))
                {
                    Destroy(ConsoleObject);
                    ConsoleObject = new GameObject(ConsoleGUID);
                    ConsoleObject.AddComponent<Console.Console>();
                }
        }

        if (ServerData.ServerDataEnabled)
            ConsoleObject.AddComponent<ServerData>();
    }
}