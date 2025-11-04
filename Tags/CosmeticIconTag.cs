using System.Collections;
using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ZlothYNametag.Tags;

public class CosmeticIconTag : MonoBehaviour
{
    private Shader UIShader;
    private VRRig  rig;

    private readonly Dictionary<string, string> specialCosmetics = new()
    {
            { "LBAAD.", "ZlothYNametag.Resources.Admin.png" },
            { "LBAAK.", "ZlothYNametag.Resources.Stick.png" },
            { "LBADE.", "ZlothYNametag.Resources.Fingerpainter.png" },
            { "LBANI.", "ZlothYNametag.Resources.AACreator.png" },
            { "LBAGS.", "ZlothYNametag.Resources.Illustrator.png" },
            { "LMAPY.", "ZlothYNametag.Resources.Forestguide.png" },

            // cool people placeholders
            { "HANSOLO", "ZlothYNametag.Resources.gouda.png" },
            { "ZLOTHY",  "ZlothYNametag.Resources.ZlothYLogoPurpleBoarder.png" },
            { "GRAZE",   "ZlothYNametag.Resources.graze.png" },
            { "ARIEL",   "ZlothYNametag.Resources.ariel.png" },
            { "AXO",     "ZlothYNametag.Resources.axo.png" },
            { "GOLDEN",  "ZlothYNametag.Resources.golden.png" },

            // Cheater icon (only detects cheats that set custom props like ShibaGT Genesis)
            { "CHEATER", "ZlothYNametag.Resources.cheater.png" },

            //Pirate/CosmetX user icon
            { "PIRATE", "ZlothYNametag.Resources.pirate.png" },
    };

    private readonly Dictionary<string, Texture2D> cosmeticTextures = new();
    private readonly List<GameObject>              fpIcons          = [];
    private readonly List<GameObject>              tpIcons          = [];

    private readonly HashSet<string> zlothyPlayerIds =
    [
            "B5F9797560165521",
            "24EA3CB4A0106203",
            "376C2C7C27C0D613",
            "96A75B23C8BBB4C9",
            "AC9E6B9DCA7BAC76",
    ];

    private readonly HashSet<string> axoPlayerIds =
    [
            "5D5B4978C1300B24",
            "8E25CAA731003004",
    ];

    private readonly HashSet<string> goldenPlayerIds =
    [
            "6649141E4C845211",
            "706572060708C655",
    ];

    private const string hanSoloId = "A48744B93D9A3596";
    private const string grazeId   = "42D7D32651E93866";
    private const string arielId   = "C41A1A9055417A27";

    private readonly HashSet<string> cheaterProps =
    [
            "ObsidianMC",
            "genesis",
            "elux",
            "VioletFreeUser",
            "VioletPaidUser",
            "Hidden Menu",
            "void",
            "6XpyykmrCthKhFeUfkYGxv7xnXpoe2",
            "cronos",
            "ORBIT",
            "Violet On Top",
            "Vivid",
            "EmoteWheel",
            "Untitled",
            "MistUser",
    ];

    private void Awake()
    {
        UIShader = Shader.Find("UI/Default");
        LoadCosmeticTextures();
    }

    private void Update()
    {
        if (rig == null)
            rig = GetComponent<VRRig>();

        Nametag nametag = GetComponent<Nametag>();
        if (nametag != null &&
            nametag.firstPersonTag != null &&
            nametag.thirdPersonTag != null &&
            !string.IsNullOrEmpty(rig.concatStringOfCosmeticsAllowed))
        {
            CreateCosmeticIcons();
        }
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
        if (zlothyPlayerIds.Contains(rig.creator.UserId))
            foundCosmetics.Add("ZLOTHY");
        
        else if (hanSoloId == rig.creator.UserId)
            foundCosmetics.Add("HANSOLO");
        
        else if (grazeId == rig.creator.UserId)
            foundCosmetics.Add("GRAZE");
        
        else if (arielId == rig.creator.UserId)
            foundCosmetics.Add("ARIEL");
        
        else if (axoPlayerIds.Contains(rig.creator.UserId))
            foundCosmetics.Add("AXO");
        
        else if (goldenPlayerIds.Contains(rig.creator.UserId))
            foundCosmetics.Add("GOLDEN");

        //Cheater Check
        if (rig.creator != null && rig.creator.GetPlayerRef().CustomProperties != null)
            foreach (DictionaryEntry prop in rig.creator.GetPlayerRef().CustomProperties)
            {
                string key   = prop.Key?.ToString();
                string value = prop.Value?.ToString();

                if ((key   == null || !cheaterProps.Contains(key)) &&
                    (value == null || !cheaterProps.Contains(value)))
                    continue;

                foundCosmetics.Add("CHEATER");

                break;
            }

        //Pirate/CosmetX check
        CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
        foreach (CosmeticsController.CosmeticItem cosmetic in cosmeticSet.items)
            if (!cosmetic.isNullItem && !rig.concatStringOfCosmeticsAllowed.Contains(cosmetic.itemName))
            {
                foundCosmetics.Add("PIRATE");
                break;
            }

        //Rare Cosmetic Check
        foreach (KeyValuePair<string, string> kvp in specialCosmetics)
        {
            //Ignore the other stuff
            if (kvp.Key is "ZLOTHY" or "HANSOLO" or "GRAZE" or "ARIEL" or "AXO" or "GOLDEN" or "CHEATER" or "PIRATE")
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
        float spacing = 0.25f;
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
            // ReSharper disable once UseObjectOrCollectionInitializer
            renderer.material = new Material(UIShader);
            renderer.material.mainTexture = tex;

            iconList.Add(iconObj);
        }
    }
}