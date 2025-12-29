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

        Nametag nametag = GetComponent<Nametag>();
        if (nametag                != null &&
            nametag.firstPersonTag != null &&
            nametag.thirdPersonTag != null &&
            !string.IsNullOrEmpty(rig.concatStringOfCosmeticsAllowed))
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

        //Gooners check
        if (hasLoadedProfilePictures && profilePicturesFromID.TryGetValue(rig.OwningNetPlayer.UserId, out Texture2D profileTex))
        {
            if (!cosmeticTextures.TryGetValue("PROFILE", out Texture2D existing) || existing != profileTex)
                cosmeticTextures["PROFILE"] = profileTex;

            foundCosmetics.Add("PROFILE");
        }


        //Cheater Check
        if (rig.creator != null && rig.creator.GetPlayerRef().CustomProperties != null)
            foreach (DictionaryEntry prop in rig.creator.GetPlayerRef().CustomProperties)
            {
                string key   = prop.Key?.ToString();
                string value = prop.Value?.ToString();

                if ((key   == null || !cheaterProps.ContainsKey(key)) &&
                    (value == null || !cheaterProps.ContainsKey(value)))
                    continue;

                foundCosmetics.Add("CHEATER");

                break;
            }

        //Pirate/CosmetX check
        CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
        foreach (CosmeticsController.CosmeticItem cosmetic in cosmeticSet.items)
            if (!cosmetic.isNullItem && !rig.concatStringOfCosmeticsAllowed.Contains(cosmetic.itemName) &&
                !rig.inTryOnRoom)
            {
                foundCosmetics.Add("PIRATE");

                break;
            }

        //Rare Cosmetic Check
        foreach (KeyValuePair<string, string> kvp in specialCosmetics)
        {
            //Ignore the other stuff
            if (kvp.Key is "CHEATER" or "PIRATE")
                continue;

            if (rig.concatStringOfCosmeticsAllowed.Contains(kvp.Key))
                foundCosmetics.Add(kvp.Key);
        }

        GameObject fpTag = GetComponent<Nametag>().firstPersonTag;
        if (fpTag != null)
            CreateIconsForTag(fpTag, fpIcons, foundCosmetics);

        GameObject tpTag = GetComponent<Nametag>().thirdPersonTag;
        if (tpTag != null)
            CreateIconsForTag(tpTag, tpIcons, foundCosmetics);
    }

    private void CreateIconsForTag(GameObject parent, List<GameObject> iconList, List<string> cosmeticKeys)
    {
        float spacing     = 0.25f;
        float startOffset = -((cosmeticKeys.Count - 1) * spacing) / 2f;

        for (int i = 0; i < cosmeticKeys.Count; i++)
        {
            if (!cosmeticTextures.TryGetValue(cosmeticKeys[i], out Texture2D tex))
                continue;

            GameObject iconObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(iconObj.GetComponent<Collider>());

            iconObj.transform.SetParent(parent.transform);
            iconObj.transform.localPosition = new Vector3(startOffset + i * spacing, 0.31f, 0f);
            iconObj.transform.localScale    = new Vector3(0.25f,                     0.25f, 0.25f);
            iconObj.transform.localRotation = Quaternion.identity;
            iconObj.layer                   = parent.layer;

            Renderer renderer = iconObj.GetComponent<Renderer>();
            // ReSharper disable once UseObjectOrCollectionInitializer
            renderer.material             = new Material(UIShader);
            renderer.material.mainTexture = tex;

            iconList.Add(iconObj);
        }
    }
}