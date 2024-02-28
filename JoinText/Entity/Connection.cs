using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuickGraph;

using AcadGeo = Autodesk.AutoCAD.Geometry;
using AcadApp = Autodesk.AutoCAD.DatabaseServices;

namespace RoutingSolid
{
    /// <summary>
    /// 边
    /// </summary>
    internal class Connection : Edge<Node>
    {
        private AcadApp.Curve curve;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="n1">起始节点</param>
        /// <param name="n2">目标节点</param>
        public Connection(Node n1, Node n2) : base(n1, n2)
        {
        }

        /// <summary>
        /// 获取(当前边)与给定坐标点对应的另一端点坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public AcadGeo.Point3d ConnectedPos(AcadGeo.Point3d pos)
        {
            if (Source.Position.IsEqualTo(pos))
                return Target.Position;
            else
                return Source.Position;
        }
    }
}
