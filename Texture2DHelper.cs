using Unity.Collections.LowLevel.Unsafe;

namespace UnityExplorerPlus;

public static class Texture2DHelper
{
    public static Texture2D Cut(this Texture2D src, RectInt rect, TextureFormat? format = TextureFormat.RGBA32)
    {
        var tex = new Texture2D(rect.width, rect.height, format ?? src.format,
            false);
        src.CopyTo(tex, new(rect.xMin, rect.yMin), Vector2Int.zero, rect.size);
        return tex;
    }
    public static int GetPixelLength(TextureFormat format) => format switch
    {
        TextureFormat.Alpha8 => 1,
        TextureFormat.ARGB4444 => 2,
        TextureFormat.RGB24 => 3,
        TextureFormat.RGBA32 => 4,
        TextureFormat.ARGB32 => 4,
        TextureFormat.RGB565 => 3,
        TextureFormat.R16 => 2,
        TextureFormat.RGBA4444 => 2,
        TextureFormat.BGRA32 => 4,
        TextureFormat.RGBA64 => 8,
        TextureFormat.RHalf => 2,
        TextureFormat.RGHalf => 4,
        TextureFormat.RGBAHalf => 8,
        TextureFormat.RFloat => 4,
        TextureFormat.RGFloat => 8,
        TextureFormat.RGBAFloat => 16,
        TextureFormat.RG16 => 2,
        TextureFormat.R8 => 1,
        _ => throw new NotSupportedException($"Not support texture format: {format}")
    };
    public static void CopyTo(this Texture2D src, Texture2D dst, RectInt srcRect, RectInt dstRect)
    {
        if (srcRect.width == dstRect.width && srcRect.height == dstRect.height)
        {
            src.CopyTo(dst, srcRect.min, dstRect.min, srcRect.size);
            return;
        }
        var temp = src.Cut(srcRect, dst.format);
        var temp1 = temp.Clone(dstRect.size, temp.format);
        temp1.CopyTo(dst, Vector2Int.zero, dstRect.min, dstRect.size);
        UnityEngine.Object.DestroyImmediate(temp1);
        UnityEngine.Object.DestroyImmediate(temp);
    }
    public static void CopyTo(this Texture2D src, Texture2D dst, Vector2Int srcPosition, Vector2Int dstPosition,
        Vector2Int size, bool flipHorizontally = false, bool flipVertically = false)
    {
        if (!dst.isReadable) throw new InvalidOperationException();
        bool destroyTex = false;
        int len = GetPixelLength(dst.format);
        try
        {
            if (src.format != dst.format || !src.isReadable)
            {
                src = src.CreateReadable(dst.format);
                destroyTex = true;
            }
            var srcRaw = src.GetRawTextureData<byte>();
            var dstRaw = dst.GetRawTextureData<byte>();
            unsafe
            {
                byte* srcP = (byte*)srcRaw.GetUnsafePtr();
                byte* dstP = (byte*)dstRaw.GetUnsafePtr();
                for (int y = 0; y < size.y; y++)
                {
                    var srcStart = ((srcPosition.y + y) * src.width + srcPosition.x) * len;
                    var dstStart = ((dstPosition.y + (flipVertically ? size.y - y - 1 : y)) * dst.width + dstPosition.x) * len;
                    if (flipHorizontally)
                    {
                        for (int x = size.x - 1; x >= 0; x--) dstP[dstStart + x] = srcP[srcStart + x];
                    }
                    else
                    {
                        UnsafeUtility.MemCpy(dstP + dstStart, srcP + srcStart, size.x * len);
                    }
                }
            }
            dst.Apply(false, false);
        }
        finally
        {
            if (destroyTex)
            {
                UnityEngine.Object.DestroyImmediate(src);
            }
        }
    }
    public static Texture2D Clone(this Texture src, Vector2Int newSize, TextureFormat newFormat, Material material = null)
    {
        bool destroyTex = false;
        if (src is RenderTexture rt && material == null)
        {
            destroyTex = false;
        }
        else
        {
            rt = new(newSize.x, newSize.y, 0);
            if (material == null)
            {
                Graphics.Blit(src, rt);
            }
            else
            {
                Graphics.Blit(src, rt, material, 0);
            }
            destroyTex = true;
        }
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(newSize.x, newSize.y, newFormat, false);
        tex.ReadPixels(new(0, 0, tex.width, tex.height), 0, 0, false);
        tex.Apply();
        if (prev == rt)
        {
            prev = null;
        }
        RenderTexture.active = prev;
        if (destroyTex) rt.Release();
        return tex;
    }
    public static Texture2D CreateReadable(this Texture2D src, TextureFormat? outputFormat = null, Material material = null)
    {
        return src.Clone(new(src.width, src.height), outputFormat ?? src.format, material);
    }
}
