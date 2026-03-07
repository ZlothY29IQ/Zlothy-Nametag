using GorillaLocomotion;
using UnityEngine;

namespace ZlothYNametag;

public class Utils
{
    public static readonly int TransparentFX    = LayerMask.NameToLayer("TransparentFX");
    public static readonly int IgnoreRaycast    = LayerMask.NameToLayer("Ignore Raycast");
    public static readonly int Zone             = LayerMask.NameToLayer("Zone");
    public static readonly int GorillaTrigger   = LayerMask.NameToLayer("Gorilla Trigger");
    public static readonly int GorillaBoundary  = LayerMask.NameToLayer("Gorilla Boundary");
    public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
    public static readonly int GorillaParticle  = LayerMask.NameToLayer("GorillaParticle");
    
    public static int NoInvisLayerMask() =>
            ~(1 << TransparentFX    | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary |
              1 << GorillaCosmetics | 1 << GorillaParticle);
    
    public static void TeleportPlayer(Vector3 destinationPosition)
    {
        GTPlayer.Instance.TeleportTo(FormatTeleportPosition(destinationPosition), GTPlayer.Instance.transform.rotation);
        VRRig.LocalRig.transform.position = destinationPosition;
    }

    public static Vector3 FormatTeleportPosition(Vector3 teleportPosition) =>
            teleportPosition - GorillaTagger.Instance.bodyCollider.transform.position +
            GorillaTagger.Instance.transform.position;
}