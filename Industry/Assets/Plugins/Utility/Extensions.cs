using UnityEngine;

public static class RectTransformExtensions
{
    public static Rect GetWorldRect
    (this RectTransform RectTransform)
    {
        Vector3[] Vector3 
        = new Vector3[4];

        RectTransform
        .GetWorldCorners
        (Vector3);
        return new Rect(
            Vector3[0].x,
            Vector3[0].y,
            Vector3[2].x - Vector3[0].x,
            Vector3[2].y - Vector3[0].y
        );
    }
}