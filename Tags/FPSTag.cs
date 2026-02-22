using System.Collections;
using TMPro;
using UnityEngine;

namespace ZlothYNametag.Tags;

public class FPSTag : MonoBehaviour
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

        int fps = rig.fps;

        Color tagColour = fps switch
                          {
                                  < 49              => Color.red,
                                  < 59              => new Color(1f, 0.5f, 0f),
                                  < 89              => Color.yellow,
                                  >= 120 and <= 255 => Color.blue,
                                  var _             => Color.green,
                          };

        firstPersonTagText.text = fps.ToString();
        thirdPersonTagText.text = fps.ToString();

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
        Plugin.Log($"Creating nametag for {name}");
        
        tagObj = new GameObject(name);
        tagObj.transform.SetParent(isThirdPerson
                                           ? GetComponent<Nametag>().ThirdPersonTag.transform
                                           : GetComponent<Nametag>().FirstPersonTag.transform);

        tagObj.transform.localPosition = new Vector3(0f, 0.125f, 0f);

        tagObj.layer = LayerMask.NameToLayer(layerName);

        tagText           = tagObj.AddComponent<TextMeshPro>();
        tagText.fontSize  = 1f;
        tagText.alignment = TextAlignmentOptions.Center;
        tagText.font      = Plugin.comicSans;
    }
}