using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mono.Security.X509.X520;

namespace UnityExplorerPlus.Inspectors
{
    internal static class CameraSwitcher
    {
        private static List<Camera> cameras = new();
        private static Camera curCamera;
        private static float lastUpdate;
        private static bool disableCameraSwitch = false;
        public static void Init()
        {
            On.UnityExplorer.Inspectors.MouseInspector.UpdateInspect += MouseInspector_UpdateInspect;
            On.UnityExplorer.Inspectors.MouseInspector.ConstructPanelContent += MouseInspector_ConstructPanelContent;
        }

        private static void MouseInspector_UpdateInspect(On.UnityExplorer.Inspectors.MouseInspector.orig_UpdateInspect orig, 
            MouseInspector self)
        {
            orig(self);
            if(Input.GetMouseButtonDown(1))
            {
                SwitchCamera();
            }
            disableCameraSwitch = MouseInspector.Mode == MouseInspectMode.World ||
                MouseInspector.Mode == MouseInspectMode.UI;

            switchText.gameObject.SetActive(!disableCameraSwitch);

            if(!disableCameraSwitch)
            {
                MouseInspectorR.Instance.mousePosLabel.text = "<color=grey>Mouse Position:</color> " + GetCurrentMousePosition();
            }
        }

        private static Text switchText;

        private static void MouseInspector_ConstructPanelContent(On.UnityExplorer.Inspectors.MouseInspector.orig_ConstructPanelContent orig,
            MouseInspector self)
        {
            orig(self);

            var inspect = self.ContentRoot.FindChild("InspectContent");
            inspect.transform.GetChild(0).gameObject.SetActive(false);

            switchText = UIFactory.CreateLabel(inspect, "CameraSwitcherText", 
                "Press the right mouse button to switch the camera"
                , TextAnchor.MiddleCenter);
            switchText.horizontalOverflow = HorizontalWrapMode.Overflow;

            switchText.transform.SetSiblingIndex(0);
        }
        private static void RefreshCameras()
        {
            lastUpdate = Time.realtimeSinceStartup;
            cameras.RemoveAll(x => x == null);
            foreach (var cam in Camera.allCameras)
            {
                if (!cameras.Contains(cam))
                {
                    cameras.Add(cam);
                }
            }
        }
        private static void RefreshText()
        {
            string t;
            if(curCamera == null)
            {
                t = "No camera";
            }
            else
            {
                t = curCamera.name;
                if(curCamera == Camera.main)
                {
                    t += "<color=green>(Main Camera)</color>";
                }
            }
            switchText.text = "<b>Press the right mouse button to switch the camera.</b> Current: " + t;
        }
        private static void SwitchCamera()
        {
            RefreshCameras();

            var index = cameras.IndexOf(GetCurrentCamera()) + 1;
            if(index == 0)
            {
                curCamera = Camera.main;
            }
            else
            {
                if(cameras.Count <= index)
                {
                    index = 0;
                }
                curCamera = cameras[index];
            }

        }
        public static Camera GetCurrentCamera()
        {
            if (Time.realtimeSinceStartup > (lastUpdate + 0.5f))
            {
                RefreshCameras();
            }
            if(curCamera == null)
            {
                curCamera = Camera.main;
            }
            RefreshText();
            return curCamera;
        }
        public static Vector2 GetCurrentMousePosition()
        {
            var cam = GetCurrentCamera();
            var mousePos = Input.mousePosition;
            mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
            return cam.ScreenToWorldPoint(mousePos);
        }
    }
}
