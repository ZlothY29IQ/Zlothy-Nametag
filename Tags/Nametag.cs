using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZlothYNametag.Tags;

public class Nametag : MonoBehaviour
{
    [FormerlySerializedAs("firstPersonTag")] public GameObject FirstPersonTag;
    [FormerlySerializedAs("thirdPersonTag")] public GameObject ThirdPersonTag;

    private TextMeshPro firstPersonTagText;

    private NetPlayer   player;
    private TextMeshPro thirdPersonTagText;

    private void Start() => player = GetComponent<VRRig>().OwningNetPlayer;

    private void Update()
    {
        FirstPersonTag.transform.LookAt(Plugin.firstPersonCameraTransform);
        ThirdPersonTag.transform.LookAt(Plugin.thirdPersonCameraTransform);

        FirstPersonTag.transform.Rotate(0f, 180f, 0f);
        ThirdPersonTag.transform.Rotate(0f, 180f, 0f);

        firstPersonTagText.text = player.NickName;
        thirdPersonTagText.text = player.NickName;
    }

    private void OnDestroy()
    {
        Destroy(FirstPersonTag);
        Destroy(ThirdPersonTag);
    }

    public void UpdateColour(Color colour)
    {
        if (FirstPersonTag == null || ThirdPersonTag == null)
            CreateNametags();

        firstPersonTagText.color = colour;
        thirdPersonTagText.color = colour;
    }

    private void CreateNametags()
    {
        CreateNametag(ref FirstPersonTag, ref firstPersonTagText, "FirstPersonTag", "FirstPersonOnly");
        CreateNametag(ref ThirdPersonTag, ref thirdPersonTagText, "ThirdPersonTag", "MirrorOnly");
    }

    private void CreateNametag(ref GameObject tagObj, ref TextMeshPro tagText, string name, string layerName)
    {
        tagObj                                   = new GameObject(name, typeof(Canvas), typeof(RectTransform));
        tagObj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        tagObj.transform.SetParent(transform);
        tagObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        tagObj.layer = LayerMask.NameToLayer(layerName);

        tagText           = tagObj.AddComponent<TextMeshPro>();
        tagText.fontSize  = 1.5f;
        tagText.alignment = TextAlignmentOptions.Center;
        tagText.font      = Plugin.comicSans;
    }
}