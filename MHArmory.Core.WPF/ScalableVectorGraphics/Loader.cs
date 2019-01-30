using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MHArmory.Core.WPF.ScalableVectorGraphics
{
    public class VectorGraphicsInfo
    {
        public IList<Path> Paths { get; }
        public Rect Viewbox { get; }

        public VectorGraphicsInfo(IList<Path> paths, Rect viewbox)
        {
            Paths = paths;
            Viewbox = viewbox;
        }
    }

    public class Loader
    {
        public VectorGraphicsInfo LoadFromFile(string svgFullFilename)
        {
            if (svgFullFilename == null)
                throw new ArgumentNullException(nameof(svgFullFilename));

            var document = XDocument.Load(svgFullFilename);
            return LoadFromXml(document);
        }

        public VectorGraphicsInfo LoadFromStream(System.IO.Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var document = XDocument.Load(stream);
            return LoadFromXml(document);
        }

        public VectorGraphicsInfo LoadFromXml(XDocument document)
        {
            XElement root = document.Root;

            XAttribute viewboxAttribute = root.Attribute("viewBox");
            if (viewboxAttribute == null)
                return null;

            var viewbox = Rect.Parse(viewboxAttribute.Value);

            var gName = XName.Get("g", "http://www.w3.org/2000/svg");
            var pathName = XName.Get("path", "http://www.w3.org/2000/svg");

            var paths = new List<Path>();

            foreach (XElement g in root.Elements(gName))
            {
                Brush fillBrush;

                XAttribute fillAttribute = g.Attribute("fill");
                if (fillAttribute != null)
                {
                    var color = (Color)ColorConverter.ConvertFromString(fillAttribute.Value);
                    fillBrush = new SolidColorBrush(color);
                    if (fillBrush.CanFreeze)
                        fillBrush.Freeze();
                }
                else
                    fillBrush = Brushes.Black;

                var currentPathGeometry = new PathGeometry();

                foreach (XElement path in g.Elements(pathName))
                {
                    XAttribute dAttribute = path.Attribute("d");
                    if (dAttribute == null)
                        continue;

                    string pathValue = dAttribute.Value;

                    currentPathGeometry.AddGeometry(Geometry.Parse(pathValue));
                }

                paths.Add(new Path
                {
                    Fill = fillBrush,
                    Data = currentPathGeometry
                });
            }

            return new VectorGraphicsInfo(paths, viewbox);
        }
    }
}
