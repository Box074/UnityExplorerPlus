
namespace UnityExplorerPlusMod;
class AssetsInfo
{
    public Dictionary<string, string[]> resources = new();
    public Dictionary<string, PrefabInfo> sharedAssets = new();
}
class PrefabInfo
{
    public string assetFile = "";
    public List<string> compoents = new();
}