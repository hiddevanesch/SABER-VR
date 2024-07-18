using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Initial;
using Microsoft.Msagl.Layout.Layered;
using UnityEngine;

namespace Util
{
    static class GraphUtil
    {
        public static Point Vector2ToPoint(Vector2 vector2)
        {
            return new Point(vector2.x, vector2.y);
        }

        public static Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        public static Vector2 SizeToVector2(Size size)
        {
            return new Vector2((float)size.Width, (float)size.Height);
        }

        public static void CenterGraph(GeometryGraph graph)
        {
            Rectangle boundingBox = graph.BoundingBox;

            Point offset = new Point(-boundingBox.Left - boundingBox.Width / 2, -boundingBox.Bottom - boundingBox.Height / 2);

            foreach (Node node in graph.Nodes)
            {
                node.BoundaryCurve.Translate(offset);
            }
        }

        public static void ApplyLayeredLayout(GeometryGraph graph)
        {
            PlaneTransformation verticalReflection = new PlaneTransformation(1, 0, 0, 0, -1, 0);

            SugiyamaLayoutSettings layeredSettings = new SugiyamaLayoutSettings()
            {
                Transformation = verticalReflection,
            };

            LayeredLayout layeredLayout = new LayeredLayout(graph, layeredSettings);

            layeredLayout.Run();
        }

        public static void ApplyInitialLayout(GeometryGraph graph)
        {
            FastIncrementalLayoutSettings initialSettings = new FastIncrementalLayoutSettings()
            {
                AvoidOverlaps = true,
                ClusterGravity = 0.1,
            };

            InitialLayout initialLayout = new InitialLayout(graph, initialSettings);

            initialLayout.Run();
        }
    }
}
