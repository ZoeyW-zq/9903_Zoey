using UnityEngine;

public class MemoryPlacementItem : MonoBehaviour
{
    [Tooltip("Optional name used in logs and the future report.")]
    public string memoryId;

    private Transform savedRoomParent;
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;
    private Vector3 savedLocalScale;
    private bool savedUseGravity;
    private bool savedIsKinematic;
    private bool hasSavedPlacement;

    public void SaveForRoomReturn(Transform roomParent)
    {
        Holdable holdable = GetComponent<Holdable>();
        if (holdable != null
            && (holdable.myRayManipulator != null
                || holdable.myMagnetSnapper != null
                || holdable.moving))
        {
            holdable.ForceDrop();
        }

        savedRoomParent = roomParent != null ? roomParent : transform.parent;
        if (savedRoomParent != null)
            transform.SetParent(savedRoomParent, true);

        savedLocalPosition = transform.localPosition;
        savedLocalRotation = transform.localRotation;
        savedLocalScale = transform.localScale;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            savedUseGravity = body.useGravity;
            savedIsKinematic = body.isKinematic;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
        }

        hasSavedPlacement = true;
    }

    public void RestoreAfterRoomReturn()
    {
        if (!hasSavedPlacement)
            return;

        if (savedRoomParent != null)
            transform.SetParent(savedRoomParent, false);

        transform.localPosition = savedLocalPosition;
        transform.localRotation = savedLocalRotation;
        transform.localScale = savedLocalScale;
        gameObject.SetActive(true);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = savedUseGravity;
            body.isKinematic = savedIsKinematic;
        }
    }
}
