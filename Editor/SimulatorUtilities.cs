using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.DeviceSimulator
{
    internal enum RenderedScreenOrientation
    {
        Portrait = 1,
        PortraitUpsideDown = 2,
        LandscapeLeft = 3,
        LandscapeRight = 4,
    }

    internal enum ResolutionScalingMode
    {
        Disabled = 0,
        FixedDpi = 1
    }

    internal enum SimulationState{ Enabled, Disabled }

    internal class SimulatorJsonSerialization
    {
        public int scale = 20;
        public bool fitToScreenEnabled = true;
        public int rotationDegree = 0;
        public bool highlightSafeAreaEnabled = false;
        [NonSerialized]
        public Quaternion rotation = Quaternion.identity;
        public string friendlyName = string.Empty;
    }

    internal static class Extensions
    {
        #region DeviceInfo
        public static bool IsAndroidDevice(this DeviceInfo deviceInfo)
        {
            return IsGivenDevice(deviceInfo, "android");
        }

        public static bool IsiOSDevice(this DeviceInfo deviceInfo)
        {
            return IsGivenDevice(deviceInfo, "ios");
        }

        public static bool IsGivenDevice(this DeviceInfo deviceInfo, string os)
        {
            return (deviceInfo?.SystemInfo != null) ? deviceInfo.SystemInfo.operatingSystem.ToLower().Contains(os) : false;
        }

        public static bool LoadOverlayImage(this DeviceInfo deviceInfo)
        {
            if (deviceInfo.Meta.overlayImage != null)
                return true;

            if (string.IsNullOrEmpty(deviceInfo.Meta.overlay))
                return false;

            var filePath = Path.Combine(deviceInfo.Directory, deviceInfo.Meta.overlay);
            if (!File.Exists(filePath))
                return false;

            var overlayBytes = File.ReadAllBytes(filePath);
            var texture = new Texture2D(2, 2, TextureFormat.Alpha8, false)
            {
                alphaIsTransparency = true
            };

            if (!texture.LoadImage(overlayBytes, false))
                return false;

            deviceInfo.Meta.overlayImage = texture;
            return true;
        }

        #endregion
    }

    internal static class SimulatorUtilities
    {
        public static void SetTextureCoordinates(ScreenOrientation orientation, Vector2[] vertices)
        {
            Assert.IsTrue(vertices.Length == 4);

            // Check the orientation to set the UVs correctly.
            var uvs = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

            int startPos = 0;
            switch (orientation)
            {
                case ScreenOrientation.Portrait:
                    startPos = 0;
                    break;
                case ScreenOrientation.LandscapeRight:
                    startPos = 1;
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    startPos = 2;
                    break;
                case ScreenOrientation.LandscapeLeft:
                    startPos = 3;
                    break;
            }

            for (int index = 0; index < 4; ++index)
            {
                var uvIndex = (index + startPos) % 4;
                vertices[index] = uvs[uvIndex];
            }
        }

        public static ScreenOrientation ToScreenOrientation(UIOrientation original)
        {
            switch (original)
            {
                case UIOrientation.Portrait:
                    return ScreenOrientation.Portrait;
                case UIOrientation.PortraitUpsideDown:
                    return ScreenOrientation.PortraitUpsideDown;
                case UIOrientation.LandscapeLeft:
                    return ScreenOrientation.LandscapeLeft;
                case UIOrientation.LandscapeRight:
                    return ScreenOrientation.LandscapeRight;
                case UIOrientation.AutoRotation:
                    return ScreenOrientation.AutoRotation;
            }
            throw new ArgumentException($"Unexpected value of UIOrientation {original}");
        }

        public static ScreenOrientation RotationToScreenOrientation(Quaternion rotation)
        {
            var angle = rotation.eulerAngles.z;
            ScreenOrientation orientation = ScreenOrientation.Portrait;
            if (angle > 315 || angle <= 45)
            {
                orientation = ScreenOrientation.Portrait;
            }
            else if (angle > 45 && angle <= 135)
            {
                orientation = ScreenOrientation.LandscapeRight;
            }
            else if (angle > 135 && angle <= 225)
            {
                orientation = ScreenOrientation.PortraitUpsideDown;
            }
            else if (angle > 225 && angle <= 315)
            {
                orientation = ScreenOrientation.LandscapeLeft;
            }
            return orientation;
        }

        public static bool IsLandscape(ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Landscape || orientation == ScreenOrientation.LandscapeLeft ||
                orientation == ScreenOrientation.LandscapeRight)
                return true;

            return false;
        }
    }
}
