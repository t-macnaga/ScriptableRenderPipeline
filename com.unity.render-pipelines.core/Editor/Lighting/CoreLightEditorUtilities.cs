using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering
{
    public static class CoreLightEditorUtilities
    {
        [Flags]
        enum HandleDirections
        {
            Left = 1 << 0,
            Up = 1 << 1,
            Right = 1 << 2,
            Down = 1 << 3,
            All = Left | Up | Right | Down
        }

        static readonly int k_RadiusHandleHash = "RadiusHandleHash".GetHashCode();

        public static void DrawPointLightWireFrameWithZTest(Light light)
        {
            // Default Color for outer cone will be Yellow if nothing has been provided.
            Color outerColor = GetLightAboveObjectWireframeColor(light.color);

            // Drawing the point light
            DrawPointLight(light, outerColor);

            // Draw the handles and labels
            DrawPointHandlesAndLabels(light, outerColor);
        }

        static void DrawPointHandlesAndLabels(Light light, Color color)
        {
            // Getting the first control on radius handle
            var firstControl = GUIUtility.GetControlID(k_RadiusHandleHash, FocusType.Passive) - 6;
            if (Event.current.type != EventType.Repaint)
                return;

            // Adding label /////////////////////////////////////
            Vector3 labelPosition = light.transform.position;

            if (GUIUtility.hotControl != 0)
            {
                var offsetFromHandle = 20f;
                var cam = Camera.current;
                float val = HandleUtility.GetProjectedSize(cam, labelPosition) * offsetFromHandle;

                labelPosition = Vector3.up * val;
                switch (GUIUtility.hotControl - firstControl)
                {
                    case 0:
                        labelPosition += light.transform.position + Vector3.right * light.range;
                        break;
                    case 1:
                        labelPosition += light.transform.position + Vector3.up * light.range;
                        break;
                    case 2:
                        labelPosition += light.transform.position + Vector3.forward * light.range;
                        break;
                    case 3:
                        labelPosition += light.transform.position + Vector3.left * light.range;
                        break;
                    case 4:
                        labelPosition += light.transform.position + Vector3.down * light.range;
                        break;
                    case 5:
                        labelPosition += light.transform.position + Vector3.back * light.range;
                        break;
                    default:
                        return;
                }

                string labelText = $"Range: {(light.range).ToString("0.00")}";
                var style = new GUIStyle{normal = {background = Texture2D.whiteTexture}};
                GUI.color = new Color(0.82f, 0.82f, 0.82f, 1);

                Handles.Label(labelPosition, labelText, style);
            }
            /////////////////////////////////////////////////////
        }

        static void DrawPointLight(Light light, Color outerColor)
        {
            float range = light.range;

            using (new Handles.DrawingScope(outerColor))
            {
                EditorGUI.BeginChangeCheck();
                range = Handles.RadiusHandle(Quaternion.identity, light.transform.position, range, false, true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(light, "Adjust Point Light");
                    light.range = range;
                }
            }
        }






        public static void DrawRectangleLightWireFrameWithZTest(Light light)
        {
            // Saving the default colors
            var defColor = Handles.color;
            var defZTest = Handles.zTest;

            // Default Color for outer cone will be Yellow if nothing has been provided.
            Color outerColor = GetLightAboveObjectWireframeColor(light.color);

            // The default z-test outer color will be 20% opacity of the outer color
            Color outerColorZTest = GetLightBehindObjectWireframeColor(outerColor);

            // Drawing before objects
            //Handles.zTest = CompareFunction.LessEqual;
            DrawRectangleLight(light, outerColor);

            // Drawing behind objects
            //Handles.zTest = CompareFunction.Greater;
            //DrawRectangleLight(light, outerColorZTest);

            // Resets the compare function to always
            //Handles.zTest = CompareFunction.Always;

            // Draw the handles and labels
            DrawRectangleHandlesAndLabels(light);

            // Resets the handle colors
            Handles.color = defColor;
            Handles.zTest = defZTest;
            /////////////////////////////////////////////////////
        }

        static void DrawRectangleLight(Light light, Color outerColor)
        {
            Vector2 size = light.areaSize;

            using (new Handles.DrawingScope(outerColor))
            {
                EditorGUI.BeginChangeCheck();
                size = DoRectHandles(size);
                Handles.DrawLine(Vector3.zero, Vector3.forward);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(light, "Adjust Area Rectangle Light");
                    light.areaSize = size;
                }
            }
        }

        static void DrawRectangleHandlesAndLabels(Light light)
        {
            // Getting the first control on radius handle
            var firstControl = GUIUtility.GetControlID(s_AreaLightHandle.GetHashCode(), FocusType.Passive) -6; // BoxBoundsHandle allocates 6 control IDs
            if (Event.current.type != EventType.Repaint)
                return;

            // Adding label /////////////////////////////////////
            Vector3 labelPosition = light.transform.position;

            if (GUIUtility.hotControl != 0)
            {
                var offsetFromHandle = 20f;
                var cam = Camera.current;
                float val = HandleUtility.GetProjectedSize(cam, labelPosition) * offsetFromHandle;

                labelPosition = Vector3.up * val;

                switch (GUIUtility.hotControl - firstControl)
                {
                    case 0: // PositiveX
                        labelPosition += Vector3.right * (light.areaSize.x / 2);
                        break;
                    case 1: // NegativeX
                        labelPosition += Vector3.left * (light.areaSize.x / 2);
                        break;
                    case 2: // PositiveY
                        labelPosition += Vector3.up * (light.areaSize.y / 2);
                        break;
                    case 3: // NegativeY
                        labelPosition += Vector3.down * (light.areaSize.y / 2);
                        break;
                    default:
                        return;
                }
                var style = new GUIStyle{normal = {background = Texture2D.whiteTexture}};
                GUI.color = new Color(0.82f, 0.82f, 0.82f, 1);
                string labelText = "";
                labelText = $"w:{light.areaSize.x:0.##} x h:{light.areaSize.y:0.##}";
                Handles.Label(labelPosition, labelText, style);
            }
        }








        public static void DrawDiscLightWireFrameWithZTest(Light light)
        {
            // Saving the default colors
            var defColor = Handles.color;
            var defZTest = Handles.zTest;

            // Default Color for outer cone will be Yellow if nothing has been provided.
            Color outerColor = GetLightAboveObjectWireframeColor(light.color);

            // The default z-test outer color will be 20% opacity of the outer color
            //Color outerColorZTest = GetLightBehindObjectWireframeColor(outerColor);

            // Drawing before objects
            //Handles.zTest = CompareFunction.LessEqual;
            DrawDiscLight(light, outerColor);

            // Drawing behind objects
            //Handles.zTest = CompareFunction.Greater;
            //DrawDiscLight(light, outerColorZTest);

            // Resets the compare function to always
            //Handles.zTest = CompareFunction.Always;

            // Draw handles
        //    DrawDiscHandlesAndLabel(light, outerColor);

            // Resets the handle colors
            Handles.color = defColor;
            Handles.zTest = defZTest;
            /////////////////////////////////////////////////////
        }



        static void DrawDiscLight(Light light, Color outerColor)
        {
            float radius = light.areaSize.x;

            using (new Handles.DrawingScope(outerColor))
            {
                EditorGUI.BeginChangeCheck();
                radius = DoDiscHandles(radius);
                Handles.DrawLine(Vector3.zero, Vector3.forward);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(light, "Adjust Area Disc Light");
                    light.areaSize = new Vector2(radius * 2, light.areaSize.y);
                }
            }



//            using (new Handles.DrawingScope(outerColor))
//            {
//                s_DiscLightHandle.center = Vector3.zero;
//                s_DiscLightHandle.radius = radius/2;
//                s_DiscLightHandle.DrawHandle();
//                //Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius/2);
//                //Handles.DrawLine(Vector3.zero, Vector3.forward);
//            }
        }

        static void DrawDiscHandlesAndLabel(Light light, Color color)
        {
            // Adding label /////////////////////////////////////
            Vector3 labelPosition = Vector3.zero;

            float radius = light.areaSize.x;
            // Draw the handles ///////////////////////////////
            Handles.color = color;

            EditorGUI.BeginChangeCheck();
            radius = SliderLineHandle(Vector3.zero, Vector3.left, radius/2);
            radius = SliderLineHandle(Vector3.zero, Vector3.right, radius);
            radius = SliderLineHandle(Vector3.zero, Vector3.up, radius);
            radius = SliderLineHandle(Vector3.zero, Vector3.down, radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(light, "Adjust Area Disc Light");
                m_HandleHotControl = GUIUtility.hotControl;
                light.areaSize = new Vector2(radius * 2, light.areaSize.y);
            }

            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl == m_HandleHotControl)
            {
                string labelText = (light.areaSize.x).ToString("0.00");
                var style = new GUIStyle(GUI.skin.label);
                var offsetFromHandle = 20;
                style.contentOffset = new Vector2(0, -(HandleUtility.GetHandleSize(labelPosition) * 0.03f - offsetFromHandle));
                Handles.Label(labelPosition, labelText, style);
            }
        }

        static readonly BoxBoundsHandle s_AreaLightHandle =
            new BoxBoundsHandle { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y };

        static Vector2 DoRectHandles(Vector2 size)
        {
            s_AreaLightHandle.center = Vector3.zero;
            s_AreaLightHandle.size = size;
            s_AreaLightHandle.drawWithZTest = true;
            s_AreaLightHandle.DrawHandle();

            return s_AreaLightHandle.size;
        }

        static readonly SphereBoundsHandle s_DiscLightHandle =
            new SphereBoundsHandle { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y } ;
        static float DoDiscHandles(float radius)
        {
            s_DiscLightHandle.center = Vector3.zero;
            s_DiscLightHandle.radius = radius/2;
            s_DiscLightHandle.drawWithZTest = true;
            s_DiscLightHandle.DrawHandle();

            return s_DiscLightHandle.radius;
        }

        static Color HandleColor()
        {
            // Give handles twice the alpha of the lines
            Color color = Handles.color;
            color.a = Mathf.Clamp01(Handles.color.a * 2);
            color = RemapLightColor(CoreUtils.ConvertSRGBToActiveColorSpace(color));
            return color;
        }

        static bool drawInnerConeAngle = true;
        public static void DrawSpotlightWireFrameWithZTest(Light spotlight, Color? drawColorOuter = null, Color? drawColorInner = null)
        {
            // Saving the default colors
            var defColor = Handles.color;
            var defZTest = Handles.zTest;

            // Default Color for outer cone will be Yellow if nothing has been provided.
            Color outerColor = GetLightAboveObjectWireframeColor(drawColorOuter ?? spotlight.color);

            // The default z-test outer color will be 20% opacity of the outer color
            Color outerColorZTest = GetLightBehindObjectWireframeColor(outerColor);

            // Default Color for inner cone will be Yellow-ish if nothing has been provided.
            Color innerColor = GetLightInnerConeColor(drawColorInner ?? spotlight.color);

            // The default z-test outer color will be 20% opacity of the inner color
            Color innerColorZTest = GetLightBehindObjectWireframeColor(innerColor);

            // Drawing before objects
            Handles.zTest = CompareFunction.LessEqual;
            DrawSpotlightWireframe(spotlight, outerColor, innerColor);

            // Drawing behind objects
            Handles.zTest = CompareFunction.Greater;
            DrawSpotlightWireframe(spotlight, outerColorZTest, innerColorZTest);

            // Resets the compare function to always
            Handles.zTest = CompareFunction.Always;

            // Draw handles
            DrawHandlesAndLabels(spotlight, outerColor);

            // Resets the handle colors
            Handles.color = defColor;
            Handles.zTest = defZTest;
        }

        // These are for the Labels, so we know which one to show
        static int m_HandleHotControl = 0;
        static bool m_ShowOuterLabel = true;
        static bool m_ShowRange = false;
        static bool m_ShowNearPlaneRange = false;

        static void DrawHandlesAndLabels(Light spotlight, Color color)
        {
            // Zero position vector3
            Vector3 zeroPos = Vector3.zero;

            // Variable for which direction to draw the handles
            HandleDirections DrawHandleDirections;

            // Draw the handles ///////////////////////////////
            Handles.color = color;

            // Draw Center Handle
            float range = spotlight.range;
            EditorGUI.BeginChangeCheck();
            range = SliderLineHandle(Vector3.zero, Vector3.forward, range);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(new[] { spotlight }, "Undo range change.");
                m_HandleHotControl = GUIUtility.hotControl;
                m_ShowRange = true;
            }

            // Draw outer handles
            DrawHandleDirections = HandleDirections.Down | HandleDirections.Up;

            EditorGUI.BeginChangeCheck();
            float outerAngle = DrawConeHandles(zeroPos, spotlight.spotAngle, range, DrawHandleDirections);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(new[] { spotlight }, "Undo outer angle change.");
                m_HandleHotControl = GUIUtility.hotControl;
                m_ShowOuterLabel = true;
            }

            // Draw inner handles
            // Commented until inner angle will bake
            float innerAngle = 0;
            if (spotlight.innerSpotAngle > 0f && drawInnerConeAngle)
            {
                DrawHandleDirections = HandleDirections.Left | HandleDirections.Right;
                EditorGUI.BeginChangeCheck();
                innerAngle = DrawConeHandles(zeroPos, spotlight.innerSpotAngle, range, DrawHandleDirections);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(new[] { spotlight }, "Undo inner angle change.");
                    m_HandleHotControl = GUIUtility.hotControl;
                    m_ShowOuterLabel = false;
                }
            }

            // Draw Near Plane Handle
            float nearPlaneRange = spotlight.shadowNearPlane;
            if(spotlight.shadows != LightShadows.None && spotlight.lightmapBakeType != LightmapBakeType.Baked)
            {
                EditorGUI.BeginChangeCheck();
                nearPlaneRange = SliderLineHandle(Vector3.zero, Vector3.forward, nearPlaneRange);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(new[] { spotlight }, "Undo shadow near plane change.");
                    m_HandleHotControl = GUIUtility.hotControl;
                    m_ShowNearPlaneRange = true;
                    nearPlaneRange = Mathf.Clamp(nearPlaneRange, 0.1f, spotlight.range);
                }
            }
            /////////////////////////////////////////////////////

            // Adding label /////////////////////////////////////
            Vector3 labelPosition = (Vector3.forward * spotlight.range);

            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl == m_HandleHotControl)
            {
                string labelText = "";
                if (m_ShowRange)
                    labelText = (spotlight.range).ToString("0.00");
                else if (m_ShowNearPlaneRange)
                    labelText = (spotlight.shadowNearPlane).ToString("0.00");
                else if (m_ShowOuterLabel)
                    labelText = (spotlight.spotAngle).ToString("0.00");
                else
                    labelText = (spotlight.innerSpotAngle).ToString("0.00");

                var style = new GUIStyle(GUI.skin.label);
                var offsetFromHandle = 10;
                style.contentOffset = new Vector2(0, -(12 + HandleUtility.GetHandleSize(labelPosition) * 0.03f + offsetFromHandle));
                Handles.Label(labelPosition, labelText, style);
            }
            /////////////////////////////////////////////////////

            // If changes has been made we update the corresponding property
            if (GUI.changed)
            {
                spotlight.spotAngle = outerAngle;
                // Commented until inner cone angle bakes
                spotlight.innerSpotAngle = innerAngle;
                spotlight.range = Math.Max(range, 0.01f);
                spotlight.shadowNearPlane = Mathf.Clamp(nearPlaneRange, 0.1f, spotlight.range);
            }

            // Resets the member variables
            if (EditorGUIUtility.hotControl == 0 && EditorGUIUtility.hotControl != m_HandleHotControl)
            {
                m_HandleHotControl = 0;
                m_ShowOuterLabel = true;
                m_ShowRange = false;
                m_ShowNearPlaneRange = false;
            }
        }

        static Color GetLightInnerConeColor(Color wireframeColor)
        {
            Color color = wireframeColor;
            color.a = 0.4f;
            return RemapLightColor(CoreUtils.ConvertSRGBToActiveColorSpace(color));
        }

        static Color GetLightAboveObjectWireframeColor(Color wireframeColor)
        {
            Color color = wireframeColor;
            color.a = 1f;
            return RemapLightColor(CoreUtils.ConvertSRGBToActiveColorSpace(color));
        }

        static Color GetLightBehindObjectWireframeColor(Color wireframeColor)
        {
            Color color = wireframeColor;
            color.a = 0.2f;
            return RemapLightColor(CoreUtils.ConvertSRGBToActiveColorSpace(color));
        }

        static Color RemapLightColor(Color src)
        {
            Color color = src;
            float max = Mathf.Max(color.r, color.g);
            if (max > 0f)
            {
                float mult = 1f / max;
                color.r *= mult;
                color.g *= mult;
                color.b *= mult;
            }
            else
            {
                color = Color.white;
            }

            return color;
        }

        static void DrawSpotlightWireframe(Light spotlight, Color outerColor, Color innerColor)
        {
            // Variable for which direction to draw the handles
            HandleDirections DrawHandleDirections;

            float outerAngle = spotlight.spotAngle;
            float innerAngle = spotlight.innerSpotAngle;
            float range = spotlight.range;

            var outerDiscRadius = range * Mathf.Sin(outerAngle * Mathf.Deg2Rad * 0.5f);
            var outerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * outerAngle * 0.5f) * range;
            var vectorLineUp = Vector3.Normalize(Vector3.forward * outerDiscDistance + Vector3.up * outerDiscRadius);
            var vectorLineLeft = Vector3.Normalize(Vector3.forward * outerDiscDistance + Vector3.left * outerDiscRadius);

            // Need to check if we need to draw inner angle
            // Need to disable this for now until we get all the inner angle baking working.
            if(innerAngle > 0f && drawInnerConeAngle)
            {
                DrawHandleDirections = HandleDirections.Up | HandleDirections.Down;
                var innerDiscRadius = range * Mathf.Sin(innerAngle * Mathf.Deg2Rad * 0.5f);
                var innerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * innerAngle * 0.5f) * range;

                // Drawing the inner Cone and also z-testing it to draw another color if behind
                Handles.color = innerColor;
                DrawConeWireframe(innerDiscRadius, innerDiscDistance, DrawHandleDirections);
            }

            // Draw range line
            Handles.color = innerColor;
            var rangeCenter = Vector3.forward * range;
            Handles.DrawLine(Vector3.zero, rangeCenter);

            // Drawing the outer Cone and also z-testing it to draw another color if behind
            Handles.color = outerColor;

            DrawHandleDirections = HandleDirections.Left | HandleDirections.Right;
            DrawConeWireframe(outerDiscRadius, outerDiscDistance, DrawHandleDirections);

            // Bottom arcs, making a nice rounded shape
            Handles.DrawWireArc(Vector3.zero, Vector3.right, vectorLineUp, outerAngle, range);
            Handles.DrawWireArc(Vector3.zero, Vector3.up, vectorLineLeft, outerAngle, range);

            // If we are using shadows we draw the near plane for shadows
            if(spotlight.shadows != LightShadows.None && spotlight.lightmapBakeType != LightmapBakeType.Baked)
            {
                DrawShadowNearPlane(spotlight, innerColor);
            }


        }

        static void DrawShadowNearPlane(Light spotlight, Color color)
        {
            Color previousColor = Handles.color;
            Handles.color = color;

            var shadowDiscRadius = Mathf.Tan(spotlight.spotAngle * Mathf.Deg2Rad * 0.5f) * spotlight.shadowNearPlane;
            var shadowDiscDistance = spotlight.shadowNearPlane ;
            Handles.DrawWireDisc(Vector3.forward * shadowDiscDistance, Vector3.forward, shadowDiscRadius);
            Handles.DrawLine(Vector3.forward * shadowDiscDistance, (Vector3.right * shadowDiscRadius) + (Vector3.forward * shadowDiscDistance));
            Handles.DrawLine(Vector3.forward * shadowDiscDistance, (-Vector3.right * shadowDiscRadius) + (Vector3.forward * shadowDiscDistance));

            Handles.color = previousColor;
        }

        static void DrawConeWireframe(float radius, float height, HandleDirections handleDirections)
        {
            var rangeCenter = Vector3.forward * height;
            if (handleDirections.HasFlag(HandleDirections.Up))
            {
                var rangeUp = rangeCenter + Vector3.up * radius;
                Handles.DrawLine(Vector3.zero, rangeUp);
            }

            if (handleDirections.HasFlag(HandleDirections.Down))
            {
                var rangeDown = rangeCenter - Vector3.up * radius;
                Handles.DrawLine(Vector3.zero, rangeDown);
            }

            if (handleDirections.HasFlag(HandleDirections.Right))
            {
                var rangeRight = rangeCenter + Vector3.right * radius;
                Handles.DrawLine(Vector3.zero, rangeRight);
            }

            if (handleDirections.HasFlag(HandleDirections.Left))
            {
                var rangeLeft = rangeCenter - Vector3.right * radius;
                Handles.DrawLine(Vector3.zero, rangeLeft);
            }

            //Draw Circle
            Handles.DrawWireDisc(rangeCenter, Vector3.forward, radius);
        }

        static float DrawConeHandles(Vector3 position, float angle, float range, HandleDirections handleDirections)
        {
            if(handleDirections.HasFlag(HandleDirections.Left))
            {
                angle = SizeSliderSpotAngle(position, Vector3.forward, -Vector3.right, range, angle);
            }
            if(handleDirections.HasFlag(HandleDirections.Up))
            {
                angle = SizeSliderSpotAngle(position, Vector3.forward, Vector3.up, range, angle);
            }
            if(handleDirections.HasFlag(HandleDirections.Right))
            {
                angle = SizeSliderSpotAngle(position, Vector3.forward, Vector3.right, range, angle);
            }
            if(handleDirections.HasFlag(HandleDirections.Down))
            {
                angle = SizeSliderSpotAngle(position, Vector3.forward, -Vector3.up, range, angle);
            }
            return angle;
        }

        static Vector2 SliderPlaneHandle(Vector3 origin, Vector3 axis1, Vector3 axis2, Vector2 position)
        {
            Vector3 pos = origin + position.x * axis1 + position.y * axis2;
            float sizeHandle = HandleUtility.GetHandleSize(pos);
            bool temp = GUI.changed;
            GUI.changed = false;
            pos = Handles.Slider2D(pos, Vector3.forward, axis1, axis2, sizeHandle * 0.03f, Handles.DotHandleCap, 0f);
            if (GUI.changed)
            {
                position = new Vector2(Vector3.Dot(pos, axis1), Vector3.Dot(pos, axis2));
            }
            GUI.changed |= temp;
            return position;
        }

        static float SliderLineHandle(Vector3 position, Vector3 direction, float value)
        {
            Vector3 pos = position + direction * value;
            float sizeHandle = HandleUtility.GetHandleSize(pos);
            bool temp = GUI.changed;
            GUI.changed = false;
            pos = Handles.Slider(pos, direction, sizeHandle * 0.03f, Handles.DotHandleCap, 0f);
            if (GUI.changed)
            {
                value = Vector3.Dot(pos - position, direction);
            }
            GUI.changed |= temp;
            return value;
        }

        static float SliderCircleHandle(Vector3 position, Vector3 normal, Vector3 zeroValueDirection, float angleValue, float radius)
        {
            zeroValueDirection.Normalize();
            normal.Normalize();
            Quaternion rot = Quaternion.AngleAxis(angleValue, normal);
            Vector3 pos = position + rot * zeroValueDirection * radius;
            float sizeHandle = HandleUtility.GetHandleSize(pos);
            bool temp = GUI.changed;
            GUI.changed = false;
            Vector3 tangeant = Vector3.Cross(normal, (pos - position).normalized);
            pos = Handles.Slider(pos, tangeant, sizeHandle * 0.03f, Handles.DotHandleCap, 0f);
            if (GUI.changed)
            {
                Vector3 dir = (pos - position).normalized;
                Vector3 cross = Vector3.Cross(zeroValueDirection, dir);
                int sign = ((cross - normal).sqrMagnitude < (-cross - normal).sqrMagnitude) ? 1 : -1;
                angleValue = Mathf.Acos(Vector3.Dot(zeroValueDirection, dir)) * Mathf.Rad2Deg * sign;
            }
            GUI.changed |= temp;
            return angleValue;
        }

        static int s_SliderSpotAngleId;


        static float SizeSliderSpotAngle(Vector3 position, Vector3 forward, Vector3 axis, float range, float spotAngle)
        {
            if (Math.Abs(spotAngle) <= 0.05f && GUIUtility.hotControl != s_SliderSpotAngleId)
                return spotAngle;
            var angledForward = Quaternion.AngleAxis(Mathf.Max(spotAngle, 0.05f) * 0.5f, axis) * forward;
            var centerToLeftOnSphere = (angledForward * range + position) - (position + forward * range);
            bool temp = GUI.changed;
            GUI.changed = false;
            var newMagnitude = Mathf.Max(0f, SliderLineHandle(position + forward * range, centerToLeftOnSphere.normalized, centerToLeftOnSphere.magnitude));
            if (GUI.changed)
            {
                s_SliderSpotAngleId = GUIUtility.hotControl;
                centerToLeftOnSphere = centerToLeftOnSphere.normalized * newMagnitude;
                angledForward = (centerToLeftOnSphere + (position + forward * range) - position).normalized;
                spotAngle = Mathf.Clamp(Mathf.Acos(Vector3.Dot(forward, angledForward)) * Mathf.Rad2Deg * 2, 0f, 179f);
                if (spotAngle <= 0.05f || float.IsNaN(spotAngle))
                    spotAngle = 0f;
            }
            GUI.changed |= temp;
            return spotAngle;
        }

        public static Color GetLightHandleColor(Color wireframeColor)
        {
            Color color = wireframeColor;
            color.a = Mathf.Clamp01(color.a * 2);
            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.linear : color;
        }

        // Don't use Handles.Disc as it break the highlight of the gizmo axis, use our own draw disc function instead for gizmo
        public static void DrawWireDisc(Quaternion q, Vector3 position, Vector3 axis, float radius)
        {
            Matrix4x4 rotation = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);

            Gizmos.color = Color.white;
            float theta = 0.0f;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = rotation * new Vector3(x, y, 0);
            pos += position;
            Vector3 newPos = pos;
            Vector3 lastPos = pos;
            for (theta = 0.1f; theta < 2.0f * Mathf.PI; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);

                newPos = rotation * new Vector3(x, y, 0);
                newPos += position;
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }

        public static void DrawSpotlightWireframe(Vector3 outerAngleInnerAngleRange, float shadowPlaneDistance = -1f)
        {
            float outerAngle = outerAngleInnerAngleRange.x;
            float innerAngle = outerAngleInnerAngleRange.y;
            float range = outerAngleInnerAngleRange.z;


            var outerDiscRadius = range * Mathf.Sin(outerAngle * Mathf.Deg2Rad * 0.5f);
            var outerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * outerAngle * 0.5f) * range;
            var vectorLineUp = Vector3.Normalize(Vector3.forward * outerDiscDistance + Vector3.up * outerDiscRadius);
            var vectorLineLeft = Vector3.Normalize(Vector3.forward * outerDiscDistance + Vector3.left * outerDiscRadius);

            if(innerAngle>0f)
            {
                var innerDiscRadius = range * Mathf.Sin(innerAngle * Mathf.Deg2Rad * 0.5f);
                var innerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * innerAngle * 0.5f) * range;
                DrawConeWireframe(innerDiscRadius, innerDiscDistance);
            }
            DrawConeWireframe(outerDiscRadius, outerDiscDistance);
            Handles.DrawWireArc(Vector3.zero, Vector3.right, vectorLineUp, outerAngle, range);
            Handles.DrawWireArc(Vector3.zero, Vector3.up, vectorLineLeft, outerAngle, range);

            if (shadowPlaneDistance > 0)
            {
                var shadowDiscRadius = shadowPlaneDistance * Mathf.Sin(outerAngle * Mathf.Deg2Rad * 0.5f);
                var shadowDiscDistance = Mathf.Cos(Mathf.Deg2Rad * outerAngle / 2) * shadowPlaneDistance;
                Handles.DrawWireDisc(Vector3.forward * shadowDiscDistance, Vector3.forward, shadowDiscRadius);
            }
        }

        static void DrawConeWireframe(float radius, float height)
        {
            var rangeCenter = Vector3.forward * height;
            var rangeUp = rangeCenter + Vector3.up * radius;
            var rangeDown = rangeCenter - Vector3.up * radius;
            var rangeRight = rangeCenter + Vector3.right * radius;
            var rangeLeft = rangeCenter - Vector3.right * radius;

            //Draw Lines
            Handles.DrawLine(Vector3.zero, rangeUp);
            Handles.DrawLine(Vector3.zero, rangeDown);
            Handles.DrawLine(Vector3.zero, rangeRight);
            Handles.DrawLine(Vector3.zero, rangeLeft);

            Handles.DrawWireDisc(Vector3.forward * height, Vector3.forward, radius);
        }

        public static Vector3 DrawSpotlightHandle(Vector3 outerAngleInnerAngleRange)
        {
            float outerAngle = outerAngleInnerAngleRange.x;
            float innerAngle = outerAngleInnerAngleRange.y;
            float range = outerAngleInnerAngleRange.z;

            if (innerAngle > 0f)
            {
                innerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.right, range, innerAngle);
                innerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.left, range, innerAngle);
                innerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.up, range, innerAngle);
                innerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.down, range, innerAngle);
            }

            outerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.right, range, outerAngle);
            outerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.left, range, outerAngle);
            outerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.up, range, outerAngle);
            outerAngle = SizeSliderSpotAngle(Vector3.zero, Vector3.forward, Vector3.down, range, outerAngle);

            range = SliderLineHandle(Vector3.zero, Vector3.forward, range);

            return new Vector3(outerAngle, innerAngle, range);
        }

        public static void DrawAreaLightWireframe(Vector2 rectangleSize)
        {
            Handles.DrawWireCube(Vector3.zero, rectangleSize);
        }

        public static Vector2 DrawAreaLightHandle(Vector2 rectangleSize, bool withYAxis)
        {
            float halfWidth = rectangleSize.x * 0.5f;
            float halfHeight = rectangleSize.y * 0.5f;

            EditorGUI.BeginChangeCheck();
            halfWidth = SliderLineHandle(Vector3.zero, Vector3.right, halfWidth);
            halfWidth = SliderLineHandle(Vector3.zero, Vector3.left, halfWidth);
            if (EditorGUI.EndChangeCheck())
            {
                halfWidth = Mathf.Max(0f, halfWidth);
            }

            if (withYAxis)
            {
                EditorGUI.BeginChangeCheck();
                halfHeight = SliderLineHandle(Vector3.zero, Vector3.up, halfHeight);
                halfHeight = SliderLineHandle(Vector3.zero, Vector3.down, halfHeight);
                if (EditorGUI.EndChangeCheck())
                {
                    halfHeight = Mathf.Max(0f, halfHeight);
                }
            }

            return new Vector2(halfWidth * 2f, halfHeight * 2f);
        }

        // Same as Gizmo.DrawFrustum except that when aspect is below one, fov represent fovX instead of fovY
        // Use to match our light frustum pyramid behavior
        public static void DrawPyramidFrustumWireframe(Vector4 aspectFovMaxRangeMinRange, float distanceTruncPlane = 0f)
        {
            float aspect = aspectFovMaxRangeMinRange.x;
            float fov = aspectFovMaxRangeMinRange.y;
            float maxRange = aspectFovMaxRangeMinRange.z;
            float minRange = aspectFovMaxRangeMinRange.w;
            float tanfov = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);

            var startAngles = new Vector3[4];
            if (minRange > 0.0f)
            {
                startAngles = GetFrustrumProjectedRectAngles(minRange, aspect, tanfov);
                Handles.DrawLine(startAngles[0], startAngles[1]);
                Handles.DrawLine(startAngles[1], startAngles[2]);
                Handles.DrawLine(startAngles[2], startAngles[3]);
                Handles.DrawLine(startAngles[3], startAngles[0]);
            }

            if (distanceTruncPlane > 0f)
            {
                var truncAngles = GetFrustrumProjectedRectAngles(distanceTruncPlane, aspect, tanfov);
                Handles.DrawLine(truncAngles[0], truncAngles[1]);
                Handles.DrawLine(truncAngles[1], truncAngles[2]);
                Handles.DrawLine(truncAngles[2], truncAngles[3]);
                Handles.DrawLine(truncAngles[3], truncAngles[0]);
            }

            var endAngles = GetFrustrumProjectedRectAngles(maxRange, aspect, tanfov);
            Handles.DrawLine(endAngles[0], endAngles[1]);
            Handles.DrawLine(endAngles[1], endAngles[2]);
            Handles.DrawLine(endAngles[2], endAngles[3]);
            Handles.DrawLine(endAngles[3], endAngles[0]);

            Handles.DrawLine(startAngles[0], endAngles[0]);
            Handles.DrawLine(startAngles[1], endAngles[1]);
            Handles.DrawLine(startAngles[2], endAngles[2]);
            Handles.DrawLine(startAngles[3], endAngles[3]);
        }

        // Same as Gizmo.DrawFrustum except that when aspect is below one, fov represent fovX instead of fovY
        // Use to match our light frustum pyramid behavior
        public static void DrawSpherePortionWireframe(Vector4 aspectFovMaxRangeMinRange, float distanceTruncPlane = 0f)
        {
            float aspect = aspectFovMaxRangeMinRange.x;
            float fov = aspectFovMaxRangeMinRange.y;
            float maxRange = aspectFovMaxRangeMinRange.z;
            float minRange = aspectFovMaxRangeMinRange.w;
            float tanfov = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);

            var startAngles = new Vector3[4];
            if (minRange > 0f)
            {
                startAngles = GetFrustrumProjectedRectAngles(minRange, aspect, tanfov);
                Handles.DrawLine(startAngles[0], startAngles[1]);
                Handles.DrawLine(startAngles[1], startAngles[2]);
                Handles.DrawLine(startAngles[2], startAngles[3]);
                Handles.DrawLine(startAngles[3], startAngles[0]);
            }

            if (distanceTruncPlane > 0f)
            {
                var truncAngles = GetFrustrumProjectedRectAngles(distanceTruncPlane, aspect, tanfov);
                Handles.DrawLine(truncAngles[0], truncAngles[1]);
                Handles.DrawLine(truncAngles[1], truncAngles[2]);
                Handles.DrawLine(truncAngles[2], truncAngles[3]);
                Handles.DrawLine(truncAngles[3], truncAngles[0]);
            }

            var endAngles = GetSphericalProjectedRectAngles(maxRange, aspect, tanfov);
            var planProjectedCrossNormal0 = new Vector3(endAngles[0].y, -endAngles[0].x, 0).normalized;
            var planProjectedCrossNormal1 = new Vector3(endAngles[1].y, -endAngles[1].x, 0).normalized;
            Vector3[] faceNormals = new[] {
                Vector3.right - Vector3.Dot((endAngles[3] + endAngles[0]).normalized, Vector3.right) * (endAngles[3] + endAngles[0]).normalized,
                Vector3.up    - Vector3.Dot((endAngles[0] + endAngles[1]).normalized, Vector3.up)    * (endAngles[0] + endAngles[1]).normalized,
                Vector3.left  - Vector3.Dot((endAngles[1] + endAngles[2]).normalized, Vector3.left)  * (endAngles[1] + endAngles[2]).normalized,
                Vector3.down  - Vector3.Dot((endAngles[2] + endAngles[3]).normalized, Vector3.down)  * (endAngles[2] + endAngles[3]).normalized,
                //cross
                planProjectedCrossNormal0 - Vector3.Dot((endAngles[1] + endAngles[3]).normalized, planProjectedCrossNormal0)  * (endAngles[1] + endAngles[3]).normalized,
                planProjectedCrossNormal1 - Vector3.Dot((endAngles[0] + endAngles[2]).normalized, planProjectedCrossNormal1)  * (endAngles[0] + endAngles[2]).normalized,
            };

            float[] faceAngles = new[] {
                Vector3.Angle(endAngles[3], endAngles[0]),
                Vector3.Angle(endAngles[0], endAngles[1]),
                Vector3.Angle(endAngles[1], endAngles[2]),
                Vector3.Angle(endAngles[2], endAngles[3]),
                Vector3.Angle(endAngles[1], endAngles[3]),
                Vector3.Angle(endAngles[0], endAngles[2]),
            };

            Handles.DrawWireArc(Vector3.zero, faceNormals[0], endAngles[0], faceAngles[0], maxRange);
            Handles.DrawWireArc(Vector3.zero, faceNormals[1], endAngles[1], faceAngles[1], maxRange);
            Handles.DrawWireArc(Vector3.zero, faceNormals[2], endAngles[2], faceAngles[2], maxRange);
            Handles.DrawWireArc(Vector3.zero, faceNormals[3], endAngles[3], faceAngles[3], maxRange);
            Handles.DrawWireArc(Vector3.zero, faceNormals[4], endAngles[0], faceAngles[4], maxRange);
            Handles.DrawWireArc(Vector3.zero, faceNormals[5], endAngles[1], faceAngles[5], maxRange);

            Handles.DrawLine(startAngles[0], endAngles[0]);
            Handles.DrawLine(startAngles[1], endAngles[1]);
            Handles.DrawLine(startAngles[2], endAngles[2]);
            Handles.DrawLine(startAngles[3], endAngles[3]);
        }

        static Vector3[] GetFrustrumProjectedRectAngles(float distance, float aspect, float tanFOV)
        {
            Vector3 sizeX;
            Vector3 sizeY;
            float minXYTruncSize = distance * tanFOV;
            if (aspect >= 1.0f)
            {
                sizeX = new Vector3(minXYTruncSize * aspect, 0, 0);
                sizeY = new Vector3(0, minXYTruncSize, 0);
            }
            else
            {
                sizeX = new Vector3(minXYTruncSize, 0, 0);
                sizeY = new Vector3(0, minXYTruncSize / aspect, 0);
            }
            Vector3 center = new Vector3(0, 0, distance);
            Vector3[] angles =
            {
                center + sizeX + sizeY,
                center - sizeX + sizeY,
                center - sizeX - sizeY,
                center + sizeX - sizeY
            };

            return angles;
        }

        static Vector3[] GetSphericalProjectedRectAngles(float distance, float aspect, float tanFOV)
        {
            var angles = GetFrustrumProjectedRectAngles(distance, aspect, tanFOV);
            for (int index = 0; index < 4; ++index)
                angles[index] = angles[index].normalized * distance;
            return angles;
        }

        public static Vector4 DrawPyramidFrustumHandle(Vector4 aspectFovMaxRangeMinRange, bool useNearPlane, float minAspect = 0.05f, float maxAspect = 20f, float minFov = 1f)
        {
            float aspect = aspectFovMaxRangeMinRange.x;
            float fov = aspectFovMaxRangeMinRange.y;
            float maxRange = aspectFovMaxRangeMinRange.z;
            float minRange = aspectFovMaxRangeMinRange.w;
            float tanfov = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);

            var e = GetFrustrumProjectedRectAngles(maxRange, aspect, tanfov);

            if (useNearPlane)
            {
                minRange = SliderLineHandle(Vector3.zero, Vector3.forward, minRange);
            }

            maxRange = SliderLineHandle(Vector3.zero, Vector3.forward, maxRange);

            float distanceRight = HandleUtility.DistanceToLine(e[0], e[3]);
            float distanceLeft = HandleUtility.DistanceToLine(e[1], e[2]);
            float distanceUp = HandleUtility.DistanceToLine(e[0], e[1]);
            float distanceDown = HandleUtility.DistanceToLine(e[2], e[3]);

            int pointIndex = 0;
            if (distanceRight < distanceLeft)
            {
                if (distanceUp < distanceDown)
                    pointIndex = 0;
                else
                    pointIndex = 3;
            }
            else
            {
                if (distanceUp < distanceDown)
                    pointIndex = 1;
                else
                    pointIndex = 2;
            }

            Vector2 send = e[pointIndex];
            Vector3 farEnd = new Vector3(0, 0, maxRange);
            EditorGUI.BeginChangeCheck();
            Vector2 received = SliderPlaneHandle(farEnd, Vector3.right, Vector3.up, send);
            if (EditorGUI.EndChangeCheck())
            {
                bool fixedFov = Event.current.control && !Event.current.shift;
                bool fixedAspect = Event.current.shift && !Event.current.control;

                //work on positive quadrant
                int xSign = send.x < 0f ? -1 : 1;
                int ySign = send.y < 0f ? -1 : 1;
                Vector2 corrected = new Vector2(received.x * xSign, received.y * ySign);

                //fixed aspect correction
                if (fixedAspect)
                {
                    corrected.x = corrected.y * aspect;
                }

                //remove aspect deadzone
                if (corrected.x > maxAspect * corrected.y)
                {
                    corrected.y = corrected.x * minAspect;
                }
                if (corrected.x < minAspect * corrected.y)
                {
                    corrected.x = corrected.y / maxAspect;
                }

                //remove fov deadzone
                float deadThresholdFoV = Mathf.Tan(Mathf.Deg2Rad * minFov * 0.5f) * maxRange;
                corrected.x = Mathf.Max(corrected.x, deadThresholdFoV);
                corrected.y = Mathf.Max(corrected.y, deadThresholdFoV, Mathf.Epsilon * 100); //prevent any division by zero

                if (!fixedAspect)
                {
                    aspect = corrected.x / corrected.y;
                }
                float min = Mathf.Min(corrected.x, corrected.y);
                if (!fixedFov && maxRange > Mathf.Epsilon * 100)
                {
                    fov = Mathf.Atan(min / maxRange) * 2f * Mathf.Rad2Deg;
                }
            }

            return new Vector4(aspect, fov, maxRange, minRange);
        }

        public static Vector4 DrawSpherePortionHandle(Vector4 aspectFovMaxRangeMinRange, bool useNearPlane, float minAspect = 0.05f, float maxAspect = 20f, float minFov = 1f)
        {
            float aspect = aspectFovMaxRangeMinRange.x;
            float fov = aspectFovMaxRangeMinRange.y;
            float maxRange = aspectFovMaxRangeMinRange.z;
            float minRange = aspectFovMaxRangeMinRange.w;
            float tanfov = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);

            var endAngles = GetSphericalProjectedRectAngles(maxRange, aspect, tanfov);

            if (useNearPlane)
            {
                minRange = SliderLineHandle(Vector3.zero, Vector3.forward, minRange);
            }

            maxRange = SliderLineHandle(Vector3.zero, Vector3.forward, maxRange);

            float distanceRight = HandleUtility.DistanceToLine(endAngles[0], endAngles[3]);
            float distanceLeft = HandleUtility.DistanceToLine(endAngles[1], endAngles[2]);
            float distanceUp = HandleUtility.DistanceToLine(endAngles[0], endAngles[1]);
            float distanceDown = HandleUtility.DistanceToLine(endAngles[2], endAngles[3]);

            int pointIndex = 0;
            if (distanceRight < distanceLeft)
            {
                if (distanceUp < distanceDown)
                    pointIndex = 0;
                else
                    pointIndex = 3;
            }
            else
            {
                if (distanceUp < distanceDown)
                    pointIndex = 1;
                else
                    pointIndex = 2;
            }

            Vector2 send = endAngles[pointIndex];
            Vector3 farEnd = new Vector3(0, 0, endAngles[0].z);
            EditorGUI.BeginChangeCheck();
            Vector2 received = SliderPlaneHandle(farEnd, Vector3.right, Vector3.up, send);
            if (EditorGUI.EndChangeCheck())
            {
                bool fixedFov = Event.current.control && !Event.current.shift;
                bool fixedAspect = Event.current.shift && !Event.current.control;

                //work on positive quadrant
                int xSign = send.x < 0f ? -1 : 1;
                int ySign = send.y < 0f ? -1 : 1;
                Vector2 corrected = new Vector2(received.x * xSign, received.y * ySign);

                //fixed aspect correction
                if (fixedAspect)
                {
                    corrected.x = corrected.y * aspect;
                }

                //remove aspect deadzone
                if (corrected.x > maxAspect * corrected.y)
                {
                    corrected.y = corrected.x * minAspect;
                }
                if (corrected.x < minAspect * corrected.y)
                {
                    corrected.x = corrected.y / maxAspect;
                }

                //remove fov deadzone
                float deadThresholdFoV = Mathf.Tan(Mathf.Deg2Rad * minFov * 0.5f) * maxRange;
                corrected.x = Mathf.Max(corrected.x, deadThresholdFoV);
                corrected.y = Mathf.Max(corrected.y, deadThresholdFoV, Mathf.Epsilon * 100); //prevent any division by zero

                if (!fixedAspect)
                {
                    aspect = corrected.x / corrected.y;
                }
                float min = Mathf.Min(corrected.x, corrected.y);
                if (!fixedFov && maxRange > Mathf.Epsilon * 100)
                {
                    fov = Mathf.Atan(min / maxRange) * 2f * Mathf.Rad2Deg;
                }
            }

            return new Vector4(aspect, fov, maxRange, minRange);
        }

        public static void DrawOrthoFrustumWireframe(Vector4 widthHeightMaxRangeMinRange, float distanceTruncPlane = 0f)
        {
            float halfWidth = widthHeightMaxRangeMinRange.x * 0.5f;
            float halfHeight = widthHeightMaxRangeMinRange.y * 0.5f;
            float maxRange = widthHeightMaxRangeMinRange.z;
            float minRange = widthHeightMaxRangeMinRange.w;

            Vector3 sizeX = new Vector3(halfWidth, 0, 0);
            Vector3 sizeY = new Vector3(0, halfHeight, 0);
            Vector3 nearEnd = new Vector3(0, 0, minRange);
            Vector3 farEnd = new Vector3(0, 0, maxRange);

            Vector3 s1 = nearEnd + sizeX + sizeY;
            Vector3 s2 = nearEnd - sizeX + sizeY;
            Vector3 s3 = nearEnd - sizeX - sizeY;
            Vector3 s4 = nearEnd + sizeX - sizeY;

            Vector3 e1 = farEnd + sizeX + sizeY;
            Vector3 e2 = farEnd - sizeX + sizeY;
            Vector3 e3 = farEnd - sizeX - sizeY;
            Vector3 e4 = farEnd + sizeX - sizeY;

            Handles.DrawLine(s1, s2);
            Handles.DrawLine(s2, s3);
            Handles.DrawLine(s3, s4);
            Handles.DrawLine(s4, s1);

            Handles.DrawLine(e1, e2);
            Handles.DrawLine(e2, e3);
            Handles.DrawLine(e3, e4);
            Handles.DrawLine(e4, e1);

            Handles.DrawLine(s1, e1);
            Handles.DrawLine(s2, e2);
            Handles.DrawLine(s3, e3);
            Handles.DrawLine(s4, e4);

            if (distanceTruncPlane> 0f)
            {
                Vector3 truncPoint = new Vector3(0, 0, distanceTruncPlane);
                Vector3 t1 = truncPoint + sizeX + sizeY;
                Vector3 t2 = truncPoint - sizeX + sizeY;
                Vector3 t3 = truncPoint - sizeX - sizeY;
                Vector3 t4 = truncPoint + sizeX - sizeY;

                Handles.DrawLine(t1, t2);
                Handles.DrawLine(t2, t3);
                Handles.DrawLine(t3, t4);
                Handles.DrawLine(t4, t1);
            }
        }
        public static Vector4 DrawOrthoFrustumHandle(Vector4 widthHeightMaxRangeMinRange, bool useNearHandle)
        {
            float halfWidth = widthHeightMaxRangeMinRange.x * 0.5f;
            float halfHeight = widthHeightMaxRangeMinRange.y * 0.5f;
            float maxRange = widthHeightMaxRangeMinRange.z;
            float minRange = widthHeightMaxRangeMinRange.w;
            Vector3 farEnd = new Vector3(0, 0, maxRange);

            if (useNearHandle)
            {
                minRange = SliderLineHandle(Vector3.zero, Vector3.forward, minRange);
            }

            maxRange = SliderLineHandle(Vector3.zero, Vector3.forward, maxRange);

            EditorGUI.BeginChangeCheck();
            halfWidth = SliderLineHandle(farEnd, Vector3.right, halfWidth);
            halfWidth = SliderLineHandle(farEnd, Vector3.left, halfWidth);
            if (EditorGUI.EndChangeCheck())
            {
                halfWidth = Mathf.Max(0f, halfWidth);
            }

            EditorGUI.BeginChangeCheck();
            halfHeight = SliderLineHandle(farEnd, Vector3.up, halfHeight);
            halfHeight = SliderLineHandle(farEnd, Vector3.down, halfHeight);
            if (EditorGUI.EndChangeCheck())
            {
                halfHeight = Mathf.Max(0f, halfHeight);
            }

            return new Vector4(halfWidth * 2f, halfHeight * 2f, maxRange, minRange);
        }
    }
}
