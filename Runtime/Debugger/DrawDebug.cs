#if DEBUGGING

using Drawing;
using UnityEngine;

public static class DrawDebug
{
    #region Class Methods

    public static void DrawBox(Vector3 centerPoint, Vector3 size, float angle, float timeDisplay = 0.1f)
    {
        using (Draw.ingame.WithDuration(timeDisplay))
        using (Draw.ingame.WithLineWidth(1))
            Draw.ingame.WireBox(centerPoint, Quaternion.AngleAxis(angle, Vector3.forward), size, Color.green);
    }

    public static void DrawCircle(Vector3 centerPoint, float radius, float timeDisplay = 0.1f)
    {
        using (Draw.ingame.WithDuration(timeDisplay))
        using (Draw.ingame.WithLineWidth(1))
            Draw.ingame.CircleXY(centerPoint, radius, Color.green);
    }

    public static void DrawSemiCircle(Vector3 centerPoint, Vector2 direction, float degree, float radius, float timeDisplay = 0.1f)
    {
        using (Draw.ingame.WithDuration(timeDisplay))
        {
            var startDirection = Quaternion.AngleAxis(-degree / 2, Vector3.forward) * direction;
            var endDirection = Quaternion.AngleAxis(degree / 2, Vector3.forward) * direction;
            var startPosition = centerPoint + startDirection.normalized * radius;
            var endPosition = centerPoint + endDirection.normalized * radius;
            using (Draw.ingame.WithLineWidth(1))
                Draw.ingame.SolidArc(centerPoint, startPosition, endPosition, new Color(0, 1, 0, 0.5f));
        }
    }

    #endregion Class Methods
}

#endif