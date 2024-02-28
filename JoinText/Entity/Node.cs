using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcadGeo = Autodesk.AutoCAD.Geometry;

namespace RoutingSolid
{
    /// <summary>
    /// 节点
    /// </summary>
    internal class Node
    {
        private AcadGeo.Point3d position;
        private List<Connection> connections;

        public Node(AcadGeo.Point3d ver)
        {
            position = ver;
            connections = new List<Connection>();
        }

        /// <summary>
        /// 节点位置
        /// </summary>
        public AcadGeo.Point3d Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// 当前节点的边集合
        /// </summary>
        public List<Connection> Connections
        {
            get
            {
                return connections;
            }
            set
            {
                connections = value;
            }
        }

        /// <summary>
        /// 获取指定边(与当前节点相连)的另一端点
        /// </summary>
        /// <param name="conn">边</param>
        /// <returns>另一端点</returns>
        public Node GetConnection(Connection conn)
        {
            if (conn.Source == this)
                return conn.Target;
            else
                return conn.Source;
        }
    }
}
