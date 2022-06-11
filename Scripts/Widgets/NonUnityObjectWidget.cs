
namespace UnityExplorerPlusMod;

public class NonUnityObjectWidget : UnityObjectWidget
{
    public override GameObject CreateContent(GameObject uiRoot)
    {
        UIRoot = UIFactory.CreateUIObject("UnityObjectRow", uiRoot, default(Vector2));
        UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(UIRoot, false, false, true, true, 5);
        UIFactory.SetLayoutElement(UIRoot, null, 25, 9999, 0);
        return UIRoot;
    }
    public override void Update()
    {
        
    }
    public override void OnBorrowed(object target, Type targetType, ReflectionInspector inspector)
    {
        owner = inspector;
        if(UIRoot is null)
        {
            CreateContent(inspector.UIRoot);
        }
        else
        {
            UIRoot.transform.SetParent(inspector.UIRoot.transform);
        }
        UIRoot.transform.SetSiblingIndex(inspector.UIRoot.transform.childCount - 2);
    }
}
