using UnityEngine;

namespace Core
{
    public static class WallTextureGenerator
    {
        private const int SUPERSAMPLE = 8;

        public static Texture2D Generate(WallLevelDesignData data, WallMapVisualSettings settings)
        {
            var ppc = Mathf.Max(1, settings.PixelsPerCell);
            var ssPpc = ppc * SUPERSAMPLE;

            var ssWidth = data.GridWidth * ssPpc;
            var ssHeight = data.GridHeight * ssPpc;

            var ssPixels = new Color[ssWidth * ssHeight];
            for (var i = 0; i < ssPixels.Length; i++) ssPixels[i] = settings.BackgroundColor;

            var th = Mathf.Clamp(settings.WallThickness * SUPERSAMPLE, 1, ssPpc / 2);
 
            var r = settings.CornerRadius > 0
                ? Mathf.Min(settings.CornerRadius * SUPERSAMPLE, th, ssPpc / 2)
                : th;

       
            for (var y = 0; y < data.GridHeight; y++)
                for (var x = 0; x < data.GridWidth; x++)
                    if (data.IsWall(x, y))
                        DrawStraightEdges(ssPixels, ssWidth, ssHeight, data, x, y, ssPpc, th, settings.WallColor);

      
            for (var vy = 0; vy <= data.GridHeight; vy++)
                for (var vx = 0; vx <= data.GridWidth; vx++)
                    ProcessVertex(ssPixels, ssWidth, ssHeight, data, vx, vy, ssPpc, r, settings);

  
            var finalWidth = data.GridWidth * ppc;
            var finalHeight = data.GridHeight * ppc;
            var finalPixels = Downsample(ssPixels, ssWidth, ssHeight, finalWidth, finalHeight, SUPERSAMPLE);

            var tex = new Texture2D(finalWidth, finalHeight, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            tex.SetPixels(finalPixels);
            tex.Apply();
            return tex;
        }

        private static bool IsWall(WallLevelDesignData data, int x, int y)
        {
            if (x < 0 || y < 0 || x >= data.GridWidth || y >= data.GridHeight) return false;
            return data.IsWall(x, y);
        }

        private static void DrawStraightEdges(Color[] pixels, int texW, int texH, WallLevelDesignData data,
            int cx, int cy, int ppc, int th, Color c)
        {
            var baseX = cx * ppc;
            var baseY = cy * ppc;

            var up = IsWall(data, cx, cy + 1);
            var down = IsWall(data, cx, cy - 1);
            var left = IsWall(data, cx - 1, cy);
            var right = IsWall(data, cx + 1, cy);

            if (!up) DrawH(pixels, texW, texH, baseX, baseY + ppc - th, ppc, th, c);
            if (!down) DrawH(pixels, texW, texH, baseX, baseY, ppc, th, c);
            if (!left) DrawV(pixels, texW, texH, baseX, baseY, th, ppc, c);
            if (!right) DrawV(pixels, texW, texH, baseX + ppc - th, baseY, th, ppc, c);
        }
        
        private static void ProcessVertex(Color[] pixels, int texW, int texH, WallLevelDesignData data,
            int vx, int vy, int ppc, int r, WallMapVisualSettings settings)
        {
            var bl = IsWall(data, vx - 1, vy - 1);
            var br = IsWall(data, vx, vy - 1);
            var tl = IsWall(data, vx - 1, vy);
            var tr = IsWall(data, vx, vy);

            var n = (bl ? 1 : 0) + (br ? 1 : 0) + (tl ? 1 : 0) + (tr ? 1 : 0);

            var vxPix = vx * ppc;
            var vyPix = vy * ppc;

            if (n == 1)
            {
                if (bl) CutQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.BackgroundColor, -1, -1);
                else if (br) CutQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.BackgroundColor, +1, -1);
                else if (tl) CutQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.BackgroundColor, -1, +1);
                else if (tr) CutQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.BackgroundColor, +1, +1);
            }
            else if (n == 3)
            {
                if (!bl) FillQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.WallColor, -1, -1);
                else if (!br) FillQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.WallColor, +1, -1);
                else if (!tl) FillQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.WallColor, -1, +1);
                else if (!tr) FillQuarterDisk(pixels, texW, texH, vxPix, vyPix, r, settings.WallColor, +1, +1);
            }
        }
        
        private static void CutQuarterDisk(Color[] pixels, int texW, int texH, int vxPix, int vyPix, int r,
            Color bg, int signX, int signY)
        {
            var centerX = vxPix + signX * r;
            var centerY = vyPix + signY * r;

            for (var j = 0; j < r; j++)
            {
                for (var i = 0; i < r; i++)
                {
                    var px = signX > 0 ? vxPix + i : vxPix - 1 - i;
                    var py = signY > 0 ? vyPix + j : vyPix - 1 - j;

                    if (px < 0 || py < 0 || px >= texW || py >= texH) continue;

                    var dx = px - centerX;
                    var dy = py - centerY;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r - 1f)
                        pixels[py * texW + px] = bg;
                }
            }
        }
        
        private static void FillQuarterDisk(Color[] pixels, int texW, int texH, int vxPix, int vyPix, int r,
            Color wallColor, int signX, int signY)
        {
            for (var j = 0; j < r; j++)
            {
                for (var i = 0; i < r; i++)
                {
                    var px = signX > 0 ? vxPix + i : vxPix - 1 - i;
                    var py = signY > 0 ? vyPix + j : vyPix - 1 - j;

                    if (px < 0 || py < 0 || px >= texW || py >= texH) continue;

                    var dx = px - vxPix;
                    var dy = py - vyPix;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r - 1f)
                        pixels[py * texW + px] = wallColor;
                }
            }
        }

        private static Color[] Downsample(Color[] src, int srcW, int srcH, int dstW, int dstH, int factor)
        {
            var dst = new Color[dstW * dstH];
            for (var y = 0; y < dstH; y++)
            {
                for (var x = 0; x < dstW; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (var sy = 0; sy < factor; sy++)
                    {
                        for (var sx = 0; sx < factor; sx++)
                        {
                            var c = src[(y * factor + sy) * srcW + (x * factor + sx)];
                            r += c.r; g += c.g; b += c.b; a += c.a;
                        }
                    }
                    var n = factor * factor;
                    dst[y * dstW + x] = new Color(r / n, g / n, b / n, a / n);
                }
            }
            return dst;
        }

        private static void DrawH(Color[] pixels, int texW, int texH, int sx, int sy, int len, int th, Color c)
        {
            for (var y = 0; y < th; y++)
            {
                var py = sy + y;
                if (py < 0 || py >= texH) continue;
                for (var x = 0; x < len; x++)
                {
                    var px = sx + x;
                    if (px < 0 || px >= texW) continue;
                    pixels[py * texW + px] = c;
                }
            }
        }

        private static void DrawV(Color[] pixels, int texW, int texH, int sx, int sy, int th, int len, Color c)
        {
            for (var x = 0; x < th; x++)
            {
                var px = sx + x;
                if (px < 0 || px >= texW) continue;
                for (var y = 0; y < len; y++)
                {
                    var py = sy + y;
                    if (py < 0 || py >= texH) continue;
                    pixels[py * texW + px] = c;
                }
            }
        }
    }
}