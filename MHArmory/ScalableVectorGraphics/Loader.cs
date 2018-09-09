using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MHArmory.ScalableVectorGraphics
{
    public class Loader
    {
        private double currentAbsolutePositionX = 0.0;
        private double currentAbsolutePositionY = 0.0;

        private PathFigure currentPathFigure;
        private PathGeometry currentPathGeometry;

        public (IList<Path> paths, Rect viewbox) LoadFile(string svgFullFilename)
        {
            if (svgFullFilename == null)
                throw new ArgumentNullException(nameof(svgFullFilename));

            var document = XDocument.Load(svgFullFilename);
            XElement root = document.Root;

            XAttribute viewboxAttribute = root.Attribute("viewBox");
            if (viewboxAttribute == null)
                return (null, Rect.Empty);

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

                currentPathGeometry = new PathGeometry();

                foreach (XElement path in g.Elements(pathName))
                {
                    XAttribute dAttribute = path.Attribute("d");
                    if (dAttribute == null)
                        continue;

                    string pathValue = dAttribute.Value;



                    //IList<PathElement> pathElements;

                    //try
                    //{
                    //    pathElements = PathLexer.Process(pathValue);
                    //}
                    //catch (FormatException fex)
                    //{
                    //    // TODO: log fex error.
                    //    return null;
                    //}
                    //catch (NotSupportedException nsex)
                    //{
                    //    // TODO: log nsex error.
                    //    return null;
                    //}

                    //int index = 0;

                    //while (index < pathElements.Count)
                    //{
                    //    int followingPathElementCount = CountFollowingPathElements(pathElements, index);

                    //    int indexDelta = ProcessPathElementAt(pathElements, index, followingPathElementCount);

                    //    if (indexDelta < 0)
                    //        return null;

                    //    index += indexDelta;
                    //}

                    currentPathGeometry.AddGeometry(Geometry.Parse(pathValue));
                }

                paths.Add(new Path
                {
                    Fill = fillBrush,
                    Data = currentPathGeometry
                });
            }

            return (paths, viewbox);
        }

        private int CountFollowingPathElements(IList<PathElement> pathElements, int index)
        {
            int count = 1;

            for (int i = index + 1; i < pathElements.Count; i++)
            {
                if (pathElements[i].Command != PathCommand.Unset)
                    break;

                count++;
            }

            return count;
        }

        private int ProcessPathElementAt(IList<PathElement> pathElements, int index, int groupCount)
        {
            PathElement pathElement = pathElements[index];

            switch (pathElement.Command)
            {
                case PathCommand.MoveTo:
                    return ProcessMoveTo(pathElements, index, groupCount);
                case PathCommand.LineTo:
                    return ProcessLineTo(pathElements, index, groupCount);
                case PathCommand.HorizontalLineTo:
                    return ProcessHorizontalLineTo(pathElements, index, groupCount);
                case PathCommand.VerticalLineTo:
                    return ProcessVerticalLineTo(pathElements, index, groupCount);
                case PathCommand.CubicBezier:
                    return ProcessCubicBezierTo(pathElements, index, groupCount);
                case PathCommand.Close:
                    return ProcessClose(pathElements, index, groupCount);
                default:
                    return ProcessLineTo(pathElements, index, groupCount);
            }

            return -1;
        }

        private int ProcessMoveTo(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure != null)
                throw new FormatException($"Path figure not properly closed.");

            Point position = CreatePoint(
                pathElements[index].IsAbsolute || index == 0,
                pathElements[index].Value,
                pathElements[index + 1].Value,
                true
            );

            currentPathFigure = new PathFigure
            {
                StartPoint = position,
                IsFilled = true,
                IsClosed = true
            };

            if (groupCount > 2)
            {
                int indexDelta = ProcessLineTo(pathElements, index + 2, groupCount - 2);

                if (indexDelta < 0)
                    return indexDelta;

                return 2 + indexDelta;
            }

            return groupCount;
        }

        private int ProcessClose(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure == null)
                throw new FormatException($"Path figure not properly opened.");

            currentPathFigure.Segments.Add(new LineSegment(currentPathFigure.StartPoint, false));

            currentPathGeometry.Figures.Add(currentPathFigure);

            currentPathFigure = null;

            return groupCount;
        }

        private int ProcessLineTo(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure == null)
                throw new FormatException($"Path figure not properly opened.");

            bool isAbsolute = pathElements[index].IsAbsolute;

            if (groupCount <= 2)
            {
                Point position = CreatePoint(
                    isAbsolute,
                    pathElements[index].Value,
                    pathElements[index + 1].Value,
                    true
                );

                currentPathFigure.Segments.Add(new LineSegment(position, false)); // TODO: false is hardcoded here, not good.
            }
            else
            {
                var points = new Point[groupCount / 2];

                for (int i = 0; i < groupCount; i += 2)
                {
                    points[i / 2] = CreatePoint(
                        isAbsolute,
                        pathElements[index + i].Value,
                        pathElements[index + i + 1].Value,
                        i == groupCount - 2
                    );
                }

                currentPathFigure.Segments.Add(new PolyLineSegment(points, false)); // TODO: false is hardcoded here, not good.
            }

            return groupCount;
        }

        private int ProcessHorizontalLineTo(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure == null)
                throw new FormatException($"Path figure not properly opened.");

            Point position = CreatePoint(
                pathElements[index].IsAbsolute,
                pathElements[index + groupCount - 1].Value,
                0.0,
                true
            );

            currentPathFigure.Segments.Add(new LineSegment(position, false)); // TODO: false is hardcoded here, not good.

            return groupCount;
        }

        private int ProcessVerticalLineTo(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure == null)
                throw new FormatException($"Path figure not properly opened.");

            Point position = CreatePoint(
                pathElements[index].IsAbsolute,
                0.0,
                pathElements[index + groupCount - 1].Value,
                true
            );

            currentPathFigure.Segments.Add(new LineSegment(position, false)); // TODO: false is hardcoded here, not good.

            return groupCount;
        }

        private int ProcessCubicBezierTo(IList<PathElement> pathElements, int index, int groupCount)
        {
            if (currentPathFigure == null)
                throw new FormatException($"Path figure not properly opened.");

            bool isAbsolute = pathElements[index].IsAbsolute;

            //if (groupCount == 6) // do single bezier
            //{
                Point controlPoint1 = CreatePoint(
                    isAbsolute,
                    pathElements[index].Value,
                    pathElements[index + 1].Value,
                    false
                );

                Point controlPoint2 = CreatePoint(
                    isAbsolute,
                    pathElements[index + 2].Value,
                    pathElements[index + 3].Value,
                    false
                );

                Point targetPoint = CreatePoint(
                    isAbsolute,
                    pathElements[index + 4].Value,
                    pathElements[index + 5].Value,
                    true
                );

                currentPathFigure.Segments.Add(new BezierSegment(controlPoint1, controlPoint2, targetPoint, false));

                return 6;
            //}
            //else if (groupCount > 6)
            //{
            //    var points = new Point[groupCount / 2];

            //    for (int i = 0; i < groupCount; i += 2)
            //    {
            //        points[i / 2] = CreatePoint(
            //            isAbsolute,
            //            pathElements[index + i].Value,
            //            pathElements[index + i + 1].Value,
            //            i == groupCount - 2
            //        );
            //    }

            //    currentPathFigure.Segments.Add(new PolyBezierSegment(points, false));
            //}
            //else
            //    throw new FormatException($"A cubic Bezier contains less than 6 elments ({pathElements[index]}).");

            //return groupCount;
        }

        private Point CreatePoint(bool isAbsolute, double x, double y, bool updateCurrent)
        {
            Point result;

            if (isAbsolute)
            {
                result = new Point(x, y);

                //if (updateCurrent)
                {
                    currentAbsolutePositionX = x;
                    currentAbsolutePositionY = y;
                }
            }
            else
            {
                result = new Point(currentAbsolutePositionX + x, currentAbsolutePositionY + y);

                //if (updateCurrent)
                {
                    currentAbsolutePositionX += x;
                    currentAbsolutePositionY += y;
                }
            }

            return result;
        }
    }
}
