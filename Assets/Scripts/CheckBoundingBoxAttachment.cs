using UnityEngine;
using Spine.Unity;
using Spine;

public class CheckBoundingBoxAttachment : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;

    void Start()
    {
        var skeleton = skeletonAnimation.Skeleton;
        foreach (var slot in skeleton.Slots)
        {
            var attachment = slot.Attachment;
            if (attachment is BoundingBoxAttachment)
            {
                //Debug.Log($"Slot: {slot.Data.Name} has BoundingBoxAttachment: {attachment.Name}");
            }
        }
    }
}