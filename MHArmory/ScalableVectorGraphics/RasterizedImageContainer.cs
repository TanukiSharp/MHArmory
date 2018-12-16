using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Resources;

namespace MHArmory.ScalableVectorGraphics
{
    public static class RasterizedImageContainer
    {
        private static readonly Dictionary<string, ImageSource> renders = new Dictionary<string, ImageSource>();

        public static ImageSource GetRasterizedImage(int size, string imageResourceLocation)
        {
            size *= 2;

            string key = $"{size}|{imageResourceLocation}";

            if (renders.TryGetValue(key, out ImageSource result) == false)
            {
                try
                {
                    Assembly asm = typeof(App).GetTypeInfo().Assembly;

                    Stream stream = asm.GetManifestResourceStream($"MHArmory.{imageResourceLocation}");
                    if (stream != null)
                    {
                        var svgLoader = new Loader();
                        VectorGraphicsInfo vectorGraphicsInfo = svgLoader.LoadFromStream(stream);

                        result = Rasterizer.Render(size, size, vectorGraphicsInfo);
                    }

                    renders.Add(key, result);
                }
                catch
                {
                    renders.Add(key, null);
                }
            }

            return result;
        }
    }
}
