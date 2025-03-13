using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarFollowSelect : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    // Update is called once per frame
    void Update()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected)
        {
            bool isAncestor = false;
            for (
                var check = selected;
                check != null;
                check = check.transform.parent?.gameObject
            )
            {
                if (check == scrollRect.gameObject)
                {
                    isAncestor = true;
                    break;
                }
            }
            if (isAncestor)
            {
                RectTransform child = selected.GetComponent<RectTransform>();
                Vector2 margin = new Vector2(16.0f, 16.0f);
                // Taken from NibbleByte's answer to
                // https://stackoverflow.com/questions/30766020
                Canvas.ForceUpdateCanvases();
                Vector2 viewPosMin = scrollRect.viewport.rect.min;
                Vector2 viewPosMax = scrollRect.viewport.rect.max;
                Vector2 childPosMin = scrollRect.viewport
                    .InverseTransformPoint(child.TransformPoint(child.rect.min));
                Vector2 childPosMax = scrollRect.viewport
                    .InverseTransformPoint(child.TransformPoint(child.rect.max));
                childPosMin -= margin;
                childPosMax += margin;
                Vector2 move = Vector2.zero;
                if (childPosMax.y > viewPosMax.y) {
                    move.y = childPosMax.y - viewPosMax.y;
                }
                if (childPosMin.y < viewPosMin.y) {
                    move.y = childPosMin.y - viewPosMin.y;
                }
                Vector3 worldMove = scrollRect.viewport.TransformDirection(move);
                scrollRect.content.localPosition -=
                    scrollRect.content.InverseTransformDirection(worldMove);
            }
        }
    }
}
