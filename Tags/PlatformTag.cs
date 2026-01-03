using System.Collections;
using TMPro;
using UnityEngine;

namespace ZlothYNametag.Tags;

public class PlatformTag : MonoBehaviour
{
    private GameObject firstPersonTag;

    private TextMeshPro firstPersonTagText;

    private VRRig       rig;
    private GameObject  thirdPersonTag;
    private TextMeshPro thirdPersonTagText;

    private void Start() => StartCoroutine(DelayedStart());

    private void Update()
    {
        if (rig == null)
            rig = GetComponent<VRRig>();

        string platform = GetPlatform(rig);

        Color tagColour = platform switch
                          {
                                  "STEAM"      => Color.magenta,
                                  "PC"         => Color.yellow,
                                  "Standalone" => Color.cyan,
                                  var _        => Color.white,
                          };

        firstPersonTagText.text = platform;
        thirdPersonTagText.text = platform;

        firstPersonTagText.color = tagColour;
        thirdPersonTagText.color = tagColour;
    }

    private void OnDestroy()
    {
        Destroy(firstPersonTag);
        Destroy(thirdPersonTag);
    }

    private IEnumerator DelayedStart()
    {
        while (GetComponent<Nametag>() == null)
            yield return null;

        CreateNametags();
    }

    private void CreateNametags()
    {
        CreateNametag(ref firstPersonTag, ref firstPersonTagText, "FirstPersonTag", "FirstPersonOnly", false);
        CreateNametag(ref thirdPersonTag, ref thirdPersonTagText, "ThirdPersonTag", "MirrorOnly",      true);
    }

    private void CreateNametag(ref GameObject tagObj, ref TextMeshPro tagText, string name, string layerName,
                               bool           isThirdPerson)
    {
        tagObj = new GameObject(name);
        tagObj.transform.SetParent(isThirdPerson
                                           ? GetComponent<Nametag>().thirdPersonTag.transform
                                           : GetComponent<Nametag>().firstPersonTag.transform);

        tagObj.transform.localPosition = new Vector3(0f, -0.1f, 0f);

        tagObj.layer = LayerMask.NameToLayer(layerName);

        tagText           = tagObj.AddComponent<TextMeshPro>();
        tagText.fontSize  = 0.8f;
        tagText.alignment = TextAlignmentOptions.Center;
        tagText.font      = Plugin.comicSans;
    }

    private static string GetPlatform(VRRig rig)
    {
        string concatStringOfCosmeticsAllowed = rig.concatStringOfCosmeticsAllowed;

        if (concatStringOfCosmeticsAllowed.Contains("S. FIRST LOGIN"))
            return "STEAM";

        if (concatStringOfCosmeticsAllowed.Contains("FIRST LOGIN") ||
            rig.Creator.GetPlayerRef().CustomProperties.Count >= 2)
            return "PC";

        return "Standalone";
    }
}