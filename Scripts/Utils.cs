
namespace UnityExplorerPlusMod;

public static class Utils
{
    public static int CheckCount = 5;
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id)
    {
        var count = CheckCount;
        if(count % 2 == 0) count++;
        Dictionary<string, (Texture2D, int)> text = new();
        for (int i = 0; i < CheckCount; i++)
        {
            var tex = SpriteUtils.ExtractTk2dSprite(def, id);
            using(var md5 = System.Security.Cryptography.MD5.Create())
            {
                var m5 = BitConverter.ToString(md5.ComputeHash(tex.EncodeToPNG()));
                if(text.TryGetValue(m5, out var t))
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
                else
                {
                    t = (tex, 0);
                }
                t.Item2++;
                text[m5] = t;
            }
        }
        Texture2D maxTex = null;
        int maxCount = -1;
        foreach((string md5,(Texture2D tex, int c)) in text)
        {
            if(c > maxCount)
            {
                maxCount = c;
                maxTex = tex;
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
        return maxTex;
    }
}
