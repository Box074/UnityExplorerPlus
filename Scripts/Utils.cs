
using HKGraphics;

namespace UnityExplorerPlusMod;

public static class Utils
{
    public static void NoThrow(Action action)
    {
        try
        {
            action();
        }
        catch(Exception e)
        {
            Modding.Logger.LogError(e);
        }
    }
    public static void NextFrame(Action action)
    {
        IEnumerator Runner()
        {
            yield return null;
            action();
        }
        Runner().StartCoroutine().Start();
    }
    public static int CheckCount = 5;
    public static IEnumerator SetIgnoreWait(this IEnumerator coroutine)
    {
        var flat = new FlatEnumerator(coroutine);
        while(flat.MoveNext())
        {
            if (flat.Current is WaitForSeconds || flat.Current is WaitForSecondsRealtime) continue;
            yield return flat.Current;
        }
    }
    public static Texture2D ExtractTk2dSprite(tk2dSpriteCollectionData def, int id)
    {
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

        var sdef = def.spriteDefinitions[id];

        var trimedB = sdef.GetBounds();
        var untrimedB = sdef.GetUntrimmedBounds();

        var pixelPerUnitX = maxTex.width / trimedB.size.x;
        var pixelPerUnitY = maxTex.height / trimedB.size.y;

        var otex = Texture2D.redTexture.Clone(new((int)(untrimedB.size.x * pixelPerUnitX),
           (int)(untrimedB.size.y * pixelPerUnitY)), TextureFormat.RGBA32);
        //var otex = new Texture2D((int)(untrimedB.size.x * pixelPerUnitX),
        //   (int)(untrimedB.size.y * pixelPerUnitY), TextureFormat.RGBA32, false);

        var offsetX = (int)(Mathf.Abs(trimedB.min.x - untrimedB.min.x) * pixelPerUnitX);
        var offsetY = (int)(Mathf.Abs(trimedB.min.y - untrimedB.min.y) * pixelPerUnitY);

        maxTex.CopyTo(otex, new RectInt(0, 0, maxTex.width, maxTex.height),
                            new RectInt(offsetX, offsetY, maxTex.width, maxTex.height));
        UnityEngine.Object.DestroyImmediate(maxTex);
        return otex;
    }
}
