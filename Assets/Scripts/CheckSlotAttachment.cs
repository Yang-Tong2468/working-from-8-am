using UnityEngine;
using Spine.Unity;
using Spine;

public class CheckSlotAttachment : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;
    public string slotName;

    void Update()
    {
        var slot = skeletonAnimation.Skeleton.FindSlot(slotName);
        if (slot != null)
        {
            var attachment = slot.Attachment;
            if (attachment is BoundingBoxAttachment)
            {
                Debug.Log($"{slotName} 当前是 BoundingBoxAttachment: {attachment.Name}");
            }
            else
            {
                Debug.Log($"{slotName} 当前不是 BoundingBoxAttachment，而是: {attachment.GetType().Name}");
            }
        }
    }
}