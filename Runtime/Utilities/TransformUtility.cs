using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Extensions;

namespace Runtime.Utilities
{
    public static class TransformUtility
    {
        #region Members

        private static readonly int s_numberOfSourcePoints = 8;
        private static Vector3[] s_sourcePoints = new Vector3[s_numberOfSourcePoints];
        private static List<Vector3> s_boundPoints;
        private static CalculatePointResult[] s_pointResults;
        private static List<CalculatePointResult> pointResults = new List<CalculatePointResult>();
        private static CalculateBoundsResult s_boundResult = new CalculateBoundsResult();

        #endregion Members

        #region Class Methods

        static TransformUtility()
        {
            //Initialize the pointResults array.
            s_pointResults = new CalculatePointResult[s_numberOfSourcePoints];
            for (int i = 0; i < s_pointResults.Length; i++)
            {
                s_pointResults[i] = new CalculatePointResult();
                s_pointResults[i].edgePoints = new CalculatePointResult[3];
            }

            InitializedEdgePoint(s_pointResults[0], s_pointResults[1], s_pointResults[2], s_pointResults[4]);
            InitializedEdgePoint(s_pointResults[1], s_pointResults[0], s_pointResults[3], s_pointResults[5]);
            InitializedEdgePoint(s_pointResults[2], s_pointResults[0], s_pointResults[3], s_pointResults[6]);
            InitializedEdgePoint(s_pointResults[3], s_pointResults[1], s_pointResults[2], s_pointResults[7]);
            InitializedEdgePoint(s_pointResults[4], s_pointResults[0], s_pointResults[5], s_pointResults[6]);
            InitializedEdgePoint(s_pointResults[5], s_pointResults[1], s_pointResults[4], s_pointResults[7]);
            InitializedEdgePoint(s_pointResults[6], s_pointResults[2], s_pointResults[4], s_pointResults[7]);
            InitializedEdgePoint(s_pointResults[7], s_pointResults[3], s_pointResults[6], s_pointResults[5]);
            s_boundPoints = new List<Vector3>();
        }

        /// <summary>
        /// Initialize edge point array.
        /// </summary>
        private static void InitializedEdgePoint(CalculatePointResult source, CalculatePointResult edge1, CalculatePointResult edge2, CalculatePointResult edge3)
        {
            source.edgePoints[0] = edge1;
            source.edgePoints[1] = edge2;
            source.edgePoints[2] = edge3;
            source.check = false;
        }

        /// <summary>
        /// Convert screen rect to local rect.
        /// </summary>
        public static bool ScreenRectToLocalRectInRectangle(RectTransform rectTransform, Rect screenRect, Camera camera, out Rect localRect)
        {
            Vector2 min, max;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.min, camera, out min) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenRect.max, camera, out max))
            {
                localRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
                return true;
            }
            localRect = Rect.zero;
            return false;
        }

        /// <summary>
        /// Convert world space bound to screen rect.
        /// </summary>
        public static Rect BoundsToScreenRect(Camera camera, Bounds[] bounds, bool isInFrustumCheck)
        {
            if (!isInFrustumCheck)
                return BoundsToScreenRect(camera, bounds);

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;

            int i;
            for (i = 0; i < bounds.Length; ++i)
            {
                if (GeometryUtility.TestPlanesAABB(planes, bounds[i]))
                {
                    Rect rect = BoundsToScreenRect(camera, bounds[i]);
                    min = rect.min;
                    max = rect.max;
                    break;
                }
            }

            for (i = 0; i < bounds.Length; ++i)
            {
                if (!GeometryUtility.TestPlanesAABB(planes, bounds[i]))
                    continue;

                Rect rect = BoundsToScreenRect(camera, bounds[i]);
                min.x = Mathf.Min(rect.min.x, min.x);
                min.y = Mathf.Min(rect.min.y, min.y);
                max.x = Mathf.Max(rect.max.x, max.x);
                max.y = Mathf.Max(rect.max.y, max.y);
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// Convert world space bound to screen rect.
        /// </summary>
        public static Rect BoundsToScreenRect(Camera camera, Bounds[] bounds)
        {
            if (bounds.Length == 0)
                return Rect.zero;

            Rect rect = BoundsToScreenRect(camera, bounds[0]);
            Vector2 min = rect.min;
            Vector2 max = rect.max;

            for (int i = 1; i < bounds.Length; ++i)
            {
                rect = BoundsToScreenRect(camera, bounds[i]);
                min.x = Mathf.Min(rect.min.x, min.x);
                min.y = Mathf.Min(rect.min.y, min.y);
                max.x = Mathf.Max(rect.max.x, max.x);
                max.y = Mathf.Max(rect.max.y, max.y);
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// Convert world space bound to screen rect.
        /// </summary>
        public static Rect BoundsToScreenRect(Camera camera, Bounds bounds)
        {
            Vector3 centerPoint = bounds.center;
            Vector3 extendPoint = bounds.extents;
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x - extendPoint.x, centerPoint.y - extendPoint.y, centerPoint.z - extendPoint.z));
            Vector2 max = min;
            Vector2 point = min;
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x + extendPoint.x, centerPoint.y - extendPoint.y, centerPoint.z - extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x - extendPoint.x, centerPoint.y - extendPoint.y, centerPoint.z + extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x + extendPoint.x, centerPoint.y - extendPoint.y, centerPoint.z + extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x - extendPoint.x, centerPoint.y + extendPoint.y, centerPoint.z - extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x + extendPoint.x, centerPoint.y + extendPoint.y, centerPoint.z - extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x - extendPoint.x, centerPoint.y + extendPoint.y, centerPoint.z + extendPoint.z));
            GetMinMax(point, ref min, ref max);

            point = RectTransformUtility.WorldToScreenPoint(camera, new Vector3(centerPoint.x + extendPoint.x, centerPoint.y + extendPoint.y, centerPoint.z + extendPoint.z));
            GetMinMax(point, ref min, ref max);

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// Convert world space bound to screen rect.
        /// </summary>
        public static Rect BoundsToScreenRectMinMax(Camera cam, Bounds bounds)
        {
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, bounds.min);
            Vector2 max = min;
            Vector2 point = min;
            GetMinMax(point, ref min, ref max);
            point = RectTransformUtility.WorldToScreenPoint(cam, bounds.max);
            GetMinMax(point, ref min, ref max);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// Get min max for the point.
        /// </summary>
        private static void GetMinMax(Vector2 point, ref Vector2 min, ref Vector2 max)
        {
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
        }

        /// <summary>
        /// Get center of the target.
        /// </summary>
        public static Vector3 GetCenter(this Transform target)
        {
            MeshFilter filter = target.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
                return target.TransformPoint(filter.sharedMesh.bounds.center);

            SkinnedMeshRenderer smr = target.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
                return target.TransformPoint(smr.sharedMesh.bounds.center);

            return target.position;
        }

        /// <summary>
        /// Gets common center for all transform.
        /// </summary>
        public static Vector3 GetCommonCenter(IList<Transform> transforms)
        {
            Vector3 centerPosition = GetCenter(transforms[0]);
            for (int i = 1; i < transforms.Count; ++i)
            {
                Transform target = transforms[i];
                centerPosition += GetCenter(target);
            }
            centerPosition = centerPosition / transforms.Count;
            return centerPosition;
        }

        /// <summary>
        /// Get center point for all the vector.
        /// </summary>
        public static Vector3 CenterPoint(Vector3[] vectors)
        {
            Vector3 sum = Vector3.zero;
            if (vectors == null || vectors.Length == 0)
                return sum;

            foreach (Vector3 vec in vectors)
                sum += vec;

            return sum / vectors.Length;
        }

        /// <summary>
        /// Calculate regular bounds for all the transforms.
        /// </summary>
        public static Bounds CalculateBounds(Transform[] transforms)
        {
            CalculateBoundsResult result = new CalculateBoundsResult();
            for (int i = 0; i < transforms.Length; ++i)
            {
                Transform t = transforms[i];
                CalculateBounds(t, result);
            }

            if (result.Initialized)
                return result.Bounds;

            Vector3 center = CenterPoint(transforms.Select(t => t.position).ToArray());
            return new Bounds(center, Vector3.zero);
        }

        /// <summary>
        /// Calculate regular bounds for the transform.
        /// </summary>
        public static Bounds CalculateBounds(Transform transform)
        {
            s_boundResult.Initialized = false;
            CalculateBounds(transform, s_boundResult);

            if (s_boundResult.Initialized)
                return s_boundResult.Bounds;

            return new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        /// Calculate regular bounds for the transform.
        /// </summary>
        private static void CalculateBounds(Transform transform, CalculateBoundsResult result)
        {
            if (transform is RectTransform)
            {
                CalculateBounds((RectTransform)transform, result);
            }
            else
            {
                Renderer renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                    CalculateBounds(renderer, result);
            }

            foreach (Transform child in transform)
                CalculateBounds(child, result);
        }

        /// <summary>
        /// Calculate regular bounds for rect transform.
        /// </summary>
        private static void CalculateBounds(RectTransform rt, CalculateBoundsResult result)
        {
            var relativeBounds = rt.CalculateRelativeLocalBounds();
            var localToWorldMatrix = rt.localToWorldMatrix;
            var bounds = TranslateBounds(ref localToWorldMatrix, ref relativeBounds);
            if (!result.Initialized)
            {
                result.Bounds = bounds;
                result.Initialized = true;
            }
            else
            {
                result.Bounds.Encapsulate(bounds.min);
                result.Bounds.Encapsulate(bounds.max);
            }
        }

        /// <summary>
        /// Calculate regular bounds for the renderer.
        /// </summary>
        private static void CalculateBounds(Renderer renderer, CalculateBoundsResult result)
        {
            if (renderer is ParticleSystemRenderer)
                return;

            var bounds = renderer.bounds;
            if (bounds.size == Vector3.zero && bounds.center != renderer.transform.position)
            {
                var localToWorldMatrix = renderer.transform.localToWorldMatrix;
                bounds = TranslateBounds(ref localToWorldMatrix, ref bounds);
            }

            if (!result.Initialized)
            {
                result.Bounds = bounds;
                result.Initialized = true;
            }
            else
            {
                result.Bounds.Encapsulate(bounds.min);
                result.Bounds.Encapsulate(bounds.max);
            }
        }

        /// <summary>
        /// Translate bounds in different space.
        /// </summary>
        public static Bounds TranslateBounds(ref Matrix4x4 matrix, ref Bounds bounds)
        {
            var center = matrix.MultiplyPoint(bounds.center);

            // Transform the local extents' axes.
            var extents = bounds.extents;
            var axisX = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
            var axisY = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
            var axisZ = matrix.MultiplyVector(new Vector3(0, 0, extents.z));

            // Sum their absolute value to get the world extents.
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }

        /// <summary>
        /// Expensive way of realtime calculation of the renderer's bound.
        /// </summary>
        public static Rect CalculateScreenRect(Transform trans, Camera camera)
        {
            var rect = new Rect();
            var renderer = trans.GetComponent<Renderer>();
            if (renderer != null)
            {
                Quaternion originalRotation = trans.rotation;

                // Reset rotation.
                trans.rotation = Quaternion.identity;
                // Get object bounds from unrotated object.
                Bounds bounds = renderer.bounds;

                // Bottom left near.
                s_sourcePoints[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - trans.position;
                // Bottom right near.
                s_sourcePoints[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - trans.position;
                // Top left near.
                s_sourcePoints[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - trans.position;
                // Top right near.
                s_sourcePoints[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - trans.position;
                // Bottom left far.
                s_sourcePoints[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - trans.position;
                // Bottom right far.
                s_sourcePoints[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - trans.position;
                // Top left far.
                s_sourcePoints[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - trans.position;
                // Top right far.
                s_sourcePoints[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - trans.position;

                // Restore rotation.
                trans.rotation = originalRotation;

                // Apply scaling.
                for (int s = 0; s < s_sourcePoints.Length; s++)
                {
                    s_sourcePoints[s] = new Vector3(s_sourcePoints[s].x / trans.localScale.x,
                                                    s_sourcePoints[s].y / trans.localScale.y,
                                                    s_sourcePoints[s].z / trans.localScale.z);
                }

                Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, trans.TransformPoint(s_sourcePoints[0]));
                Vector2 max = min;
                Vector2 point = min;
                GetMinMax(point, ref min, ref max);
                // Transform points from local to world space.
                for (int t = 1; t < s_sourcePoints.Length; t++)
                {
                    point = RectTransformUtility.WorldToScreenPoint(camera, trans.TransformPoint(s_sourcePoints[t]));
                    GetMinMax(point, ref min, ref max);
                }

                return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            }

            return rect;
        }

        /// <summary>
        /// Calculate screen rect from unscaled bounds.
        /// </summary>
        public static Rect CalculateScreenRectFromBound(Transform transform, Camera camera, Bounds bounds, out bool outOfScreen)
        {
            outOfScreen = false;
            if (camera == null)
                return new Rect();
            // Bottom left near.
            s_sourcePoints[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            // Bottom right near.
            s_sourcePoints[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            // Top left near.
            s_sourcePoints[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            // Top right near.
            s_sourcePoints[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            // Bottom left far.
            s_sourcePoints[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            // Bottom right far.
            s_sourcePoints[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            // Top left far.
            s_sourcePoints[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            // Top right far.
            s_sourcePoints[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

            // Apply scaling.
            bool behindScreen = true;
            for (int s = 0; s < s_sourcePoints.Length; s++)
            {
                s_sourcePoints[s] = new Vector3(s_sourcePoints[s].x / transform.localScale.x,
                                              s_sourcePoints[s].y / transform.localScale.y,
                                              s_sourcePoints[s].z / transform.localScale.z);

                s_pointResults[s].point = camera.WorldToScreenPoint(transform.TransformPoint(s_sourcePoints[s]));
                s_pointResults[s].check = false;
                if (s_pointResults[s].point.z >= 0)
                {
                    // Point is out of camera.
                    behindScreen = false;
                }
                else
                {
                    s_pointResults[s].point.x = camera.pixelWidth - s_pointResults[s].point.x;
                    s_pointResults[s].point.y = camera.pixelHeight - s_pointResults[s].point.y;
                }
            }

            if (behindScreen)
            {
                outOfScreen = true;
                return new Rect();
            }
            else
            {
                pointResults.Clear();
                s_boundPoints.Clear();
                int counter = 0;
                for (int t = 0; t < s_pointResults.Length; t++)
                {
                    if (CheckPointOutOfScreen(s_pointResults[t], camera))
                    {
                        counter++;
                        if (counter >= 4)
                        {
                            outOfScreen = true;
                            return new Rect();
                        }
                        for (int m = 0; m < s_pointResults[t].edgePoints.Length; m++)
                        {
                            // Check the interception points on screen for the edges that hasn't been flagged check.
                            if (!s_pointResults[t].edgePoints[m].check)
                                GetInterceptionPoint(s_pointResults[t], s_pointResults[t].edgePoints[m], ref s_boundPoints, camera);
                        }
                        s_pointResults[t].check = true;
                    }
                    else s_boundPoints.Add(s_pointResults[t].point);
                }

                Rect screenRect = new Rect();
                if (s_boundPoints.Count > 0)
                {
                    Vector2 min = s_boundPoints[0];
                    Vector2 max = min;
                    Vector2 refPoint;
                    for (int i = 0; i < s_boundPoints.Count; i++)
                    {
                        refPoint = s_boundPoints[i];
                        GetMinMax(refPoint, ref min, ref max);
                    }

                    screenRect.min = min;
                    screenRect.max = max;
                }
                return screenRect;
            }
        }

        /// <summary>
        /// Check if point is out of screen.
        /// </summary>
        private static bool CheckPointOutOfScreen(CalculatePointResult point, Camera camera)
        {
            return point.point.x < 0 ||
                   point.point.x > camera.pixelWidth ||
                   point.point.y < 0 ||
                   point.point.y > camera.pixelHeight;
        }

        /// <summary>
        /// Get the interception point on the edge of the screen.
        /// </summary>
        private static void GetInterceptionPoint(CalculatePointResult pt1, CalculatePointResult pt2, ref List<Vector3> points, Camera camera)
        {
            if ((pt1.point.z < 0 || pt2.point.z < 0) ||
                (pt1.point.x < 0 && pt2.point.x < 0) ||
                (pt1.point.x > camera.pixelWidth && pt2.point.x > camera.pixelWidth) ||
                (pt1.point.y < 0 && pt2.point.y < 0) ||
                (pt1.point.y > camera.pixelHeight && pt2.point.y > camera.pixelHeight))
                return;

            var slope = (pt2.point.y - pt1.point.y) / (pt2.point.x - pt1.point.x);
            var offset = pt1.point.y - pt1.point.x * slope;
            var refPt = offset;
            if (((pt1.point.x <= 0 && pt2.point.x >= 0) || (pt1.point.x >= 0 && pt2.point.x <= 0)) && (refPt >= 0 && refPt <= camera.pixelHeight))
            {
                // Intercept left edge of the screen.
                points.Add(new Vector3(0, refPt, 0));
            }

            if ((pt1.point.x <= camera.pixelWidth && pt2.point.x >= camera.pixelWidth) || (pt1.point.x >= camera.pixelWidth && pt2.point.x <= camera.pixelWidth))
            {
                refPt = offset + camera.pixelWidth * slope;
                if (refPt >= 0 && refPt <= camera.pixelHeight)
                {
                    // Intercept right edge of the screen.
                    points.Add(new Vector3(camera.pixelWidth, refPt, 0));
                }
            }

            if ((pt1.point.y <= 0 && pt2.point.y >= 0) || (pt1.point.y >= 0 && pt2.point.y <= 0))
            {
                refPt = -offset / slope;
                if (refPt >= 0 && refPt <= camera.pixelWidth)
                {
                    // Intercept bottom edge of the screen
                    points.Add(new Vector3(refPt, 0, 0));
                }
            }

            if ((pt1.point.y <= camera.pixelHeight && pt2.point.y >= camera.pixelHeight) || (pt1.point.y >= camera.pixelHeight && pt2.point.y <= camera.pixelHeight))
            {
                refPt = (camera.pixelHeight - offset) / slope;
                if (refPt >= 0 && refPt <= camera.pixelWidth)
                {
                    // Intercept top edge of the screen.
                    points.Add(new Vector3(refPt, camera.pixelHeight, 0));
                }
            }
        }

        /// <summary>
        /// Calculate unscale bounds of the transform.
        /// </summary>
        public static Bounds CalculateUnscaledBounds(Transform transform, bool includeChildrens = false)
        {
            s_boundResult.Initialized = false;
            CalculateUnscaledBounds(transform, s_boundResult, includeChildrens);
            if (s_boundResult.Initialized)
                return s_boundResult.Bounds;
            else
                return new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        /// Calculate unscale bounds of the transform.
        /// </summary>
        private static void CalculateUnscaledBounds(Transform transform, CalculateBoundsResult result, bool includeChildrens = false)
        {
            if (transform is RectTransform)
            {
                CalculateBounds((RectTransform)transform, result);
            }
            else
            {
                Renderer renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                    CalculateUnscaledBounds(renderer, result);
            }

            if (includeChildrens)
            {
                foreach (Transform child in transform)
                    CalculateUnscaledBounds(child, result);
            }
        }

        /// <summary>
        /// Calculate unscale bounds of the renderer.
        /// </summary>
        private static void CalculateUnscaledBounds(Renderer renderer, CalculateBoundsResult result)
        {
            if (renderer is ParticleSystemRenderer)
                return;

            Bounds bounds;
            if (renderer is SkinnedMeshRenderer)
            {
                bounds = ((SkinnedMeshRenderer)renderer).sharedMesh.bounds;
            }
            else
            {
                var originalPosition = renderer.transform.position;
                var originalRotation = renderer.transform.rotation;
                renderer.transform.position = Vector3.zero;
                renderer.transform.rotation = Quaternion.identity;
                bounds = renderer.bounds;
                renderer.transform.position = originalPosition;
                renderer.transform.rotation = originalRotation;
            }

            if (!result.Initialized)
            {
                result.Bounds = bounds;
                result.Initialized = true;
            }
            else
            {
                result.Bounds.Encapsulate(bounds.min);
                result.Bounds.Encapsulate(bounds.max);
            }
        }

        /// <summary>
        /// Calculate all unscale bounds of the transform.
        /// </summary>
        public static Dictionary<Transform, Bounds> CalculateAllTransBound(Transform transform, bool includeInactive = false)
        {
            var dict = new Dictionary<Transform, Bounds>();
            var allTranforms = transform.GetComponentsInChildren<Transform>(includeInactive);
            for (int i = 0; i < allTranforms.Length; i++)
            {
                var bounds = CalculateUnscaledBounds(allTranforms[i], false);
                if (bounds.size != Vector3.zero)
                    dict.Add(allTranforms[i], bounds);
            }
            return dict;
        }

        /// <summary>
        /// Calculate combined screen rect of all the unscaled bounds.
        /// </summary>
        public static Rect CalculateCombineScreenRect(Dictionary<Transform, Bounds> unscaledBounds, Camera camera, out bool outOfScreen)
        {
            var rect = new Rect();
            outOfScreen = false;
            bool outScreen;
            foreach (var pair in unscaledBounds)
            {
                CalculateCombineRect(ref rect, CalculateScreenRectFromBound(pair.Key, camera, pair.Value, out outScreen));
                if (outScreen)
                    outOfScreen = true;
            }
            return rect;
        }

        public static Rect CalculateCombineScreenRect(Transform transform, Bounds unscaledBound, Camera camera, out bool outOfScreen)
        {
            var rect = new Rect();
            outOfScreen = false;
            bool outScreen;
            CalculateCombineRect(ref rect, CalculateScreenRectFromBound(transform, camera, unscaledBound, out outScreen));
            if (outScreen)
                outOfScreen = true;
            return rect;
        }

        /// <summary>
        /// Calculate combining rect.
        /// </summary>
        private static void CalculateCombineRect(ref Rect combineRect, Rect rect)
        {
            if (rect.size.magnitude != 0)
            {
                if (combineRect.size.magnitude != 0)
                {
                    combineRect.min = new Vector2(Mathf.Min(combineRect.min.x, rect.min.x), Mathf.Min(combineRect.min.y, rect.min.y));
                    combineRect.max = new Vector2(Mathf.Max(combineRect.max.x, rect.max.x), Mathf.Max(combineRect.max.y, rect.max.y));
                }
                else combineRect = rect;
            }
        }

        /// <summary>
        /// Convert rect transform from local to screen space.
        /// </summary>
        public static Rect RectTransformToScreenSpace(List<RectTransform> rectTransforms, Camera camera)
        {
            var rect = new Rect();
            for (int i = 0; i < rectTransforms.Count; i++)
                CalculateCombineRect(ref rect, CalculateScreenRect(rectTransforms[i], camera));
            return rect;
        }

        /// <summary>
        /// Calculate screen rect for the rect transform.
        /// </summary>
        public static Rect CalculateScreenRect(RectTransform transform, Camera camera, bool cutDecimals = false)
        {
            var worldCorners = new Vector3[4];
            var screenCorners = new Vector3[4];
            transform.GetWorldCorners(worldCorners);
            for (int i = 0; i < 4; i++)
            {
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, worldCorners[i]);
                if (cutDecimals)
                {
                    screenCorners[i].x = (int)screenCorners[i].x;
                    screenCorners[i].y = (int)screenCorners[i].y;
                }
            }
            return new Rect(screenCorners[0].x,
                            screenCorners[0].y,
                            screenCorners[2].x - screenCorners[0].x,
                            screenCorners[2].y - screenCorners[0].y);
        }

        #endregion Class Methods

        #region Owner Classes

        private class CalculateBoundsResult
        {
            #region Members

            public Bounds Bounds;
            public bool Initialized;

            #endregion Members
        }

        private class CalculatePointResult
        {
            #region Members

            public Vector3 point;
            public CalculatePointResult[] edgePoints;
            public bool check;

            #endregion Members
        }

        #endregion Owner Class
    }
}