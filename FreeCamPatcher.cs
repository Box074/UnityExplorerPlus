using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniverseLib.Input;
using FreeCamPanel = UnityExplorer.UI.Panels.FreeCamPanelR;

namespace UnityExplorerPlus
{
    internal class FreeCamPatcher
    {
        private static Toggle toggle_moveAsGC;
        private static Toggle toggle_lockCam;
        private static FreeCamPanelR panel;
        public static void Init()
        {
            On.UnityExplorer.UI.Panels.FreeCamBehaviour.Update += FreeCamBehaviour_Update; ;
            On.UnityExplorer.UI.Panels.FreeCamPanel.ConstructPanelContent += FreeCamPanel_ConstructPanelContent;
        }

       

        private static void FreeCamPanel_ConstructPanelContent(On.UnityExplorer.UI.Panels.FreeCamPanel.orig_ConstructPanelContent orig, 
            UEPanel self)
        {
            orig(self);

            var s = panel = (FreeCamPanel)self.Reflect();

            ButtonRef resetPosButton = UIFactory.CreateButton(s.ContentRoot, "SynchronizeWithMainCamera",
                "Synchronize with MainCamera");
            UIFactory.SetLayoutElement(resetPosButton.GameObject, flexibleWidth: 9999, minHeight: 25);
            resetPosButton.OnClick += () =>
            {
                if(FreeCamPanel.ourCamera && FreeCamPanel.lastMainCamera &&
                    FreeCamPanel.ourCamera != FreeCamPanel.lastMainCamera)
                {
                    FreeCamPanel.ourCamera.CopyFrom(FreeCamPanel.lastMainCamera);
                }
            };

            var moveAsGame = UIFactory.CreateToggle(s.ContentRoot, "MoveAsGameCamera",
                out toggle_moveAsGC, out var t_gc);
            UIFactory.SetLayoutElement(moveAsGame, minHeight: 25, flexibleWidth: 9999);
            t_gc.text = "Moves like an orthographic camera";
            toggle_moveAsGC.isOn = false;
            toggle_moveAsGC.onValueChanged.AddListener(_ =>
            {
                RefreshControlTips();
            });
            RefreshControlTips();

            var lockCam = UIFactory.CreateToggle(s.ContentRoot, "Lock Camera",
                out toggle_lockCam, out var t_lockcam);
            UIFactory.SetLayoutElement(moveAsGame, minHeight: 25, flexibleWidth: 9999);
            t_lockcam.text = "Lock Camera";
        }

        private static void RefreshControlTips()
        {
            var text = panel.ContentRoot.FindChild("Instructions").GetComponent<Text>();
            if(toggle_moveAsGC.isOn)
            {
                text.text = @"Controls:
- WASD / Arrows / Right Mouse Button: Movement
- + / - / : FOV + / -
- Shift: Super speed";
            }
            else
            {
                text.text = @"Controls:
- WASD / Arrows: Movement
- Space / PgUp: Move up
- LeftCtrl / PgDown: Move down
- Right Mouse Button: Free look
- Shift: Super speed";
            }
        }

        private static void FreeCamBehaviour_Update(On.UnityExplorer.UI.Panels.FreeCamBehaviour.orig_Update orig, MonoBehaviour self)
        {
            if (!FreeCamPanel.inFreeCamMode || toggle_lockCam.isOn) return;
            if(!FreeCamPanel.ourCamera)
            {
                FreeCamPanel.EndFreecam();
                return;
            }
            if(toggle_moveAsGC.isOn)
            {
                FreeCamBehaviour_UpdateMoveAsGC((FreeCamBehaviourR)self);
            }
            else
            {
                FreeCamBehaviour_UpdateNormal((FreeCamBehaviourR)self);
            }
        }

        private static void FreeCamBehaviour_UpdateMoveAsGC(FreeCamBehaviourR self)
        {
            Transform transform = FreeCamPanel.ourCamera.transform;

            FreeCamPanel.currentUserCameraPosition = transform.position;
            FreeCamPanel.currentUserCameraRotation = transform.rotation;

            float moveSpeed = FreeCamPanel.desiredMoveSpeed * Time.unscaledDeltaTime;

            if (InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetKey(KeyCode.RightShift))
                moveSpeed *= 10f;

            if (InputManager.GetKey(KeyCode.LeftArrow) || InputManager.GetKey(KeyCode.A))
                transform.position += Vector3.left * moveSpeed;

            if (InputManager.GetKey(KeyCode.RightArrow) || InputManager.GetKey(KeyCode.D))
                transform.position += Vector3.right * moveSpeed;

            if (InputManager.GetKey(KeyCode.UpArrow) || InputManager.GetKey(KeyCode.W))
                transform.position += Vector3.up * moveSpeed;

            if (InputManager.GetKey(KeyCode.DownArrow) || InputManager.GetKey(KeyCode.S))
                transform.position += Vector3.down * moveSpeed;

            if (InputManager.GetKey(KeyCode.KeypadPlus))
                FreeCamPanel.ourCamera.fieldOfView += moveSpeed;

            if (InputManager.GetKey(KeyCode.KeypadMinus))
                FreeCamPanel.ourCamera.fieldOfView -= moveSpeed;

            FreeCamPanel.ourCamera.fieldOfView += InputManager.MouseScrollDelta.y * moveSpeed * -1;

            if (InputManager.GetMouseButton(1))
            {
                Vector3 mouseDelta = InputManager.MousePosition - FreeCamPanel.previousMousePosition;

                transform.position += new Vector3(-mouseDelta.x * 0.3f, -mouseDelta.y * 0.3f);
            }

            FreeCamPanel.UpdatePositionInput();

            FreeCamPanel.previousMousePosition = InputManager.MousePosition;
        }

        private static void FreeCamBehaviour_UpdateNormal(FreeCamBehaviourR _)
        {
            Transform transform = FreeCamPanel.ourCamera.transform;

            FreeCamPanel.currentUserCameraPosition = transform.position;
            FreeCamPanel.currentUserCameraRotation = transform.rotation;

            float moveSpeed = FreeCamPanel.desiredMoveSpeed * Time.unscaledDeltaTime;

            if (InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetKey(KeyCode.RightShift))
                moveSpeed *= 10f;

            if (InputManager.GetKey(KeyCode.LeftArrow) || InputManager.GetKey(KeyCode.A))
                transform.position += transform.right * -1 * moveSpeed;

            if (InputManager.GetKey(KeyCode.RightArrow) || InputManager.GetKey(KeyCode.D))
                transform.position += transform.right * moveSpeed;

            if (InputManager.GetKey(KeyCode.UpArrow) || InputManager.GetKey(KeyCode.W))
                transform.position += transform.forward * moveSpeed;

            if (InputManager.GetKey(KeyCode.DownArrow) || InputManager.GetKey(KeyCode.S))
                transform.position += transform.forward * -1 * moveSpeed;

            if (InputManager.GetKey(KeyCode.Space) || InputManager.GetKey(KeyCode.PageUp))
                transform.position += transform.up * moveSpeed;

            if (InputManager.GetKey(KeyCode.LeftControl) || InputManager.GetKey(KeyCode.PageDown))
                transform.position += transform.up * -1 * moveSpeed;

            

            if (InputManager.GetMouseButton(1))
            {
                Vector3 mouseDelta = InputManager.MousePosition - FreeCamPanel.previousMousePosition;

                float newRotationX = transform.localEulerAngles.y + mouseDelta.x * 0.3f;
                float newRotationY = transform.localEulerAngles.x - mouseDelta.y * 0.3f;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            FreeCamPanel.UpdatePositionInput();

            FreeCamPanel.previousMousePosition = InputManager.MousePosition;
        }
    }
}
