using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityExplorerPlus.STW
{
    internal static class STWNavbarButton
    {
        public static ButtonRef stwBtn;
        public static void Init()
        {
            On.UnityExplorer.UI.Widgets.TimeScaleWidget.ConstructUI += TimeScaleWidget_ConstructUI;
        }

        private static void TimeScaleWidget_ConstructUI(On.UnityExplorer.UI.Widgets.TimeScaleWidget.orig_ConstructUI orig, 
            object self, GameObject parent)
        {
            orig(self, parent);

            stwBtn = UIFactory.CreateButton(parent, "STWButton", "Freeze", new Color(0.2f, 0.2f, 0.2f));
            UIFactory.SetLayoutElement(stwBtn.Component.gameObject, minHeight: 25, minWidth: 50);
            stwBtn.OnClick = () =>
            {
                var s = STWCore.isFrozen;
                if(s)
                {
                    STWCore.Undo();
                }
                else
                {
                    STWCore.Apply();
                }
                s = !s;
                Color color = s ? new Color(0.3f, 0.3f, 0.2f) : new Color(0.2f, 0.2f, 0.2f);
                RuntimeHelper.SetColorBlock(stwBtn.Component, color, color * 1.2f, color * 0.7f);
                stwBtn.ButtonText.text = s ? "Unfreeze" : "Freeze";
            };
        }
    }
}
