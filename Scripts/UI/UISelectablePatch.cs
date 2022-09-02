
namespace UnityExplorerPlusMod;

static class UISelectablePatch
{
    public static Stack<Selectable> selectables = new();
    public static void TryBack(GameObject self)
    {
        while(selectables.TryPop(out var select))
        {
            if(select == null || !select.interactable || select.gameObject == null || !select.gameObject.activeInHierarchy) continue;
            select.Select();
            return;
        }
    }
    public static void Init()
    {
        On.UnityEngine.EventSystems.EventSystem.SetSelectedGameObject_GameObject_BaseEventData += (orig, self, selected, pointer) =>
        {
            var target = (GameObject)selected;
            var prev = self.private_m_CurrentSelected();
            orig(self, target, pointer);
            Utils.NoThrow(() =>
            {
                if(prev == target) return;
                if(!prev.TryGetComponent<Selectable>(out var prevSelectable)) return;
                selectables.Push(prevSelectable);
            });
        };
        On.UnityEngine.UI.Selectable.OnDisable += (orig, self) =>
        {
            orig(self);
            Utils.NoThrow(() =>
            {
                TryBack(self.gameObject);
            });
        };
        On.UnityEngine.UI.Selectable.set_interactable += (orig, self, val)=>
        {
            orig(self, val);
            Utils.NoThrow(() =>
            {
                if(!val) TryBack(self.gameObject);
            });
        };
    }
}
