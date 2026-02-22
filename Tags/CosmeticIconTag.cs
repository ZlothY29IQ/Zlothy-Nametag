using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GorillaNetworking;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ZlothYNametag.Tags;

public class CosmeticIconTag : MonoBehaviour
{
    private const string ProfilePicturesRepo =
            "https://raw.githubusercontent.com/ZlothY29IQ/GorillaInfo/refs/heads/main/profilepictures.txt";

    public static Dictionary<string, string> cheaterProps = [];

    private readonly Dictionary<string, Texture2D> cosmeticTextures      = new();
    private readonly Dictionary<Texture2D, Material> cachedMaterials     = new();
    private readonly List<GameObject>              fpIcons               = [];
    private readonly Dictionary<string, Texture2D> profilePicturesFromID = [];

    private readonly Dictionary<string, string> specialCosmetics = new()
    {
            { "LBAAD.", "ZlothYNametag.Resources.Admin.png" },
            { "LBAAK.", "ZlothYNametag.Resources.Stick.png" },
            { "LBADE.", "ZlothYNametag.Resources.Fingerpainter.png" },
            { "LBANI.", "ZlothYNametag.Resources.AACreator.png" },
            { "LBAGS.", "ZlothYNametag.Resources.Illustrator.png" },
            { "LMAPY.", "ZlothYNametag.Resources.Forestguide.png" },

            // Cheater icon (only detects cheats that set custom props like ShibaGT Genesis)
            { "CHEATER", "ZlothYNametag.Resources.cheater.png" },

            //Pirate/CosmetX user icon
            { "PIRATE", "ZlothYNametag.Resources.pirate.png" },
    };

    private readonly List<GameObject> tpIcons = [];
    private          bool             hasLoadedProfilePictures;

    private VRRig  rig;
    private Shader UIShader;
    private string lastCosmeticString;

    private void Awake()
    {
        UIShader = Shader.Find("UI/Default");
        LoadCosmeticTextures();
        StartCoroutine(LoadProfilePictures());
    }

    private void Update()
    {
        if (rig == null)
            rig = GetComponent<VRRig>();

        if (rig == null)
            return;

        Nametag nametag = GetComponent<Nametag>();
        if (nametag == null ||
            nametag.FirstPersonTag == null ||
            nametag.ThirdPersonTag == null ||
            string.IsNullOrEmpty(rig.rawCosmeticString))
            return;

        if (rig.rawCosmeticString == lastCosmeticString)
            return;

        lastCosmeticString = rig.rawCosmeticString;
        CreateCosmeticIcons();
    }

    private IEnumerator LoadProfilePictures()
    {
        using WWW www = new(ProfilePicturesRepo);

        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            yield break;

        StringReader reader = new(www.text);

        while (true)
        {
            string line = reader.ReadLine();

            if (line == null)
                break;

            string[] split = line.Split(';');

            if (split.Length != 3)
                continue;

            string playerId = split[1];
            string imageUrl = split[2];

            if (profilePicturesFromID.ContainsKey(playerId))
                continue;

            using WWW img = new(imageUrl);

            yield return img;

            if (!string.IsNullOrEmpty(img.error))
                continue;

            Texture2D tex = new(2, 2);
            tex.LoadImage(img.bytes);

            profilePicturesFromID[playerId] = tex;
        }

        hasLoadedProfilePictures = true;
    }

    private void LoadCosmeticTextures()
    {
        foreach (KeyValuePair<string, string> kvp in specialCosmetics)
        {
            Texture2D tex = LoadEmbeddedImage(kvp.Value);
            if (tex != null)
                cosmeticTextures[kvp.Key] = tex;
        }
    }

    private Texture2D LoadEmbeddedImage(string resourcePath)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

        if (stream == null)
            return null;

        byte[] imageData = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        stream.Read(imageData, 0, imageData.Length);

        Texture2D texture = new(2, 2);
        texture.LoadImage(imageData);

        return texture;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void CreateCosmeticIcons()
    {
        foreach (GameObject icon in fpIcons.Where(icon => icon != null))
            Destroy(icon);

        foreach (GameObject icon in tpIcons.Where(icon => icon != null))
            Destroy(icon);

        fpIcons.Clear();
        tpIcons.Clear();

        List<string> foundCosmetics = [];

        if (hasLoadedProfilePictures && rig.creator != null &&
            profilePicturesFromID.TryGetValue(rig.creator.UserId, out Texture2D profileTex))
        {
            cosmeticTextures["PROFILE"] = profileTex;
            foundCosmetics.Add("PROFILE");
        }

        if (rig.creator != null && rig.creator.GetPlayerRef().CustomProperties != null)
            foreach (DictionaryEntry prop in rig.creator.GetPlayerRef().CustomProperties)
            {
                string key   = prop.Key?.ToString();
                string value = prop.Value?.ToString();

                if ((key == null || !cheaterProps.ContainsKey(key)) &&
                    (value == null || !cheaterProps.ContainsKey(value)))
                    continue;

                foundCosmetics.Add("CHEATER");
                break;
            }

        CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
        if (cosmeticSet.items.Any(cosmetic => !cosmetic.isNullItem &&
                                              !rig.rawCosmeticString.Contains(cosmetic.itemName) &&
                                              !rig.inTryOnRoom))
        {
            foundCosmetics.Add("PIRATE");
        }

        foundCosmetics.AddRange(from kvp in specialCosmetics
                                where kvp.Key is not ("CHEATER" or "PIRATE")
                                where rig.rawCosmeticString.Contains(kvp.Key)
                                select kvp.Key);

        Nametag nametag = GetComponent<Nametag>();

        if (nametag.FirstPersonTag != null)
            CreateIconsForTag(nametag.FirstPersonTag, fpIcons, foundCosmetics);

        if (nametag.ThirdPersonTag != null)
            CreateIconsForTag(nametag.ThirdPersonTag, tpIcons, foundCosmetics);
    }

    private void CreateIconsForTag(GameObject parent, List<GameObject> iconList, List<string> cosmeticKeys)
    {
        const float spacing = 0.25f;
        float startOffset = -((cosmeticKeys.Count - 1) * spacing) / 2f;

        for (int i = 0; i < cosmeticKeys.Count; i++)
        {
            if (!cosmeticTextures.TryGetValue(cosmeticKeys[i], out Texture2D tex))
                continue;

            GameObject iconObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(iconObj.GetComponent<Collider>());

            iconObj.transform.SetParent(parent.transform);
            iconObj.transform.localPosition = new Vector3(startOffset + i * spacing, 0.31f, 0f);
            iconObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            iconObj.transform.localRotation = Quaternion.identity;
            iconObj.layer = parent.layer;

            Renderer renderer = iconObj.GetComponent<Renderer>();

            if (!cachedMaterials.TryGetValue(tex, out Material mat))
            {
                mat                  = new Material(UIShader)
                {
                        mainTexture = tex,
                };

                cachedMaterials[tex] = mat;
            }

            renderer.material = mat;

            iconList.Add(iconObj);
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject icon in fpIcons.Where(icon => icon != null))
            Destroy(icon);

        foreach (GameObject icon in tpIcons.Where(icon => icon != null))
            Destroy(icon);

        foreach (Material mat in cachedMaterials.Values.Where(mat => mat != null))
            Destroy(mat);

        foreach (Texture2D tex in profilePicturesFromID.Values.Where(tex => tex != null))
            Destroy(tex);

        foreach (Texture2D tex in cosmeticTextures.Values.Where(tex => tex != null))
            Destroy(tex);

        cachedMaterials.Clear();
        profilePicturesFromID.Clear();
        cosmeticTextures.Clear();
        fpIcons.Clear();
        tpIcons.Clear();
    }
}