using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using BepInEx;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZlothYNametag.Console;
using ZlothYNametag.Patches;
using ZlothYNametag.Tags;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ZlothYNametag;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;

    private string detectedVersionFromGorillaInfo;

    // ReSharper disable InconsistentNaming
    public static Transform firstPersonCameraTransform;
    public static Transform thirdPersonCameraTransform;

    public static TMP_FontAsset comicSans;

    public bool OutdatedVersion;

    private void Awake()
    {
        Instance = this;

        Debug.Log(Constants.LicenseUsage);
    }

    private void Start()
    {
        HarmonyPatches.ApplyHarmonyPatches();
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
    }

    private void OnGameInitialized()
    {
        using HttpClient httpClient1 = new();
        HttpResponseMessage gorillaInfoVersionResponse =
                httpClient1.GetAsync(
                                    "https://raw.githubusercontent.com/HanSolo1000Falcon/GorillaInfo/main/ZlothYNametagVersion.txt")
                           .Result;

        using (Stream gorillaInfoStream = gorillaInfoVersionResponse.Content
                                                                    .ReadAsStreamAsync().Result)
        {
            using (StreamReader reader = new(gorillaInfoStream))
            {
                detectedVersionFromGorillaInfo = reader.ReadToEnd().Trim();

                if (detectedVersionFromGorillaInfo != Constants.PluginVersion)
                {
                    OutdatedVersion = true;
                    StartCoroutine(CreateOutdatedCountdown());

                    return;
                }
            }
        }

        using HttpClient httpClient2 = new();
        HttpResponseMessage gorillaInfoKnownCheatsResponse =
                httpClient2.GetAsync(
                        "https://raw.githubusercontent.com/HanSolo1000Falcon/GorillaInfo/main/KnownCheats.txt").Result;

        using (Stream gorillaInfoStream = gorillaInfoKnownCheatsResponse.Content
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

        string ConsoleGUID = "goldentrophy_Console"; // Do not change this, it's used to get other instances of Console
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

    private IEnumerator CreateOutdatedCountdown()
    {
        Process.Start(new ProcessStartInfo
        {
                FileName        = "https://github.com/ZlothY29IQ/Zlothy-Nametag/releases/latest",
                UseShellExecute = true,
        });

        GameObject stumpObj = new("ZlothYNametagOutdatedCountdownObject");
        Canvas     canvas   = stumpObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = stumpObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        stumpObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = stumpObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta          = new Vector2(9f, 9f);
        stumpObj.transform.position   = new Vector3(-66.9419f, 12.35f, -82.6273f);
        stumpObj.transform.localScale = Vector3.one * 0.003f;
        stumpObj.transform.Rotate(0f, 180f, 0f);

        float timer      = 20f;
        int   lastSecond = Mathf.CeilToInt(timer);

        TextMeshProUGUI textObj = new GameObject("FinText").AddComponent<TextMeshProUGUI>();
        textObj.transform.SetParent(stumpObj.transform, false);
        textObj.fontSize  = 30f;
        textObj.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0f,   -50f);
        textRect.sizeDelta        = new Vector2(900f, 700f);

        textObj.text =
                "<color=red>You are not on the latest release of ZlothYNametag!</color>\n"               +
                "A new tab should have open to get the latest version.\n"                                +
                "If it did not open please manually download the latest at:\n"                           +
                " <size=80%><b>https://github.com/ZlothY29IQ/Zlothy-Nametag/releases/latest<b></size>\n" +
                $"<color=yellow>Game will close in</color> {lastSecond} <color=yellow>seconds</color>";

        Texture2D tex = LoadEmbeddedImage("ZlothYNametag.Resources.cheater.png");
        if (tex != null)
        {
            GameObject imageObj = new("WarningIcon");
            imageObj.transform.SetParent(stumpObj.transform, false);
            Image uiImage = imageObj.AddComponent<Image>();

            RectTransform imgRect      = imageObj.GetComponent<RectTransform>();
            float         targetHeight = 115f;
            float         aspect       = (float)tex.width / tex.height;
            float         targetWidth  = targetHeight     * aspect;

            imgRect.sizeDelta        = new Vector2(targetWidth, targetHeight);
            imgRect.anchoredPosition = new Vector2(0f,          100f);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            uiImage.sprite = sprite;
        }

        while (timer > 0f)
        {
            if (stumpObj != null && Camera.main != null)
            {
                stumpObj.transform.LookAt(Camera.main.transform.position);
                stumpObj.transform.Rotate(0f, 180f, 0f);
            }

            timer -= Time.deltaTime;
            int currentSecond = Mathf.CeilToInt(timer);

            if (currentSecond != lastSecond)
            {
                lastSecond = currentSecond;
                textObj.text =
                        "<color=red>You are not on the latest release of ZlothYNametag!</color>\n"               +
                        "A new tab should have open to get the latest version.\n"                                +
                        "If it did not open please manually download the latest at:\n"                           +
                        " <size=80%><b>https://github.com/ZlothY29IQ/Zlothy-Nametag/releases/latest<b></size>\n" +
                        $"<color=yellow>Game will close in</color> {lastSecond} <color=yellow>seconds</color>";
            }

            yield return null;
        }

        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    private Texture2D LoadEmbeddedImage(string resourcePath)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

        if (stream == null) return null;
        byte[] imageData = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        stream.Read(imageData, 0, imageData.Length);
        Texture2D texture = new(2, 2);
        texture.LoadImage(imageData);

        return texture;
    }
}