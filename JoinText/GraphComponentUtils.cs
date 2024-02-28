using System.Collections.Generic;
using QuickGraph.Algorithms.ConnectedComponents;
using QuickGraph;
using QuickGraph.Algorithms.Search;

namespace RoutingSolid
{
    public static class GraphComponentUtils
    {
        /// <summary>
        /// 得到每部分Component的"子图"
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g"></param>
        /// <param name="dfs"></param>
        /// <returns></returns>
        public static List<IUndirectedGraph<TVertex, TEdge>> GetSubComponentGraphs<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, ConnectedComponentsAlgorithm<TVertex, TEdge> dfs) where TEdge : IEdge<TVertex>
        {
            //先得到顶点，找到相邻边，再重新构造图
            var subGraphs = new List<IUndirectedGraph<TVertex, TEdge>>();
            if (dfs.ComponentCount == 1)
            {
                subGraphs.Add(g);
                return subGraphs;
            }

            for (var i = 0; i < dfs.ComponentCount; i++)
            {
                var subGraphVertexs = new List<TVertex>();
                foreach (var kv in dfs.Components)
                {
                    var vertex = kv.Key;
                    var subIndex = kv.Value;
                    if (subIndex == i)
                    {
                        subGraphVertexs.Add(vertex);
                    }
                }
                if (subGraphVertexs.Count == 1)
                {
                    var subGraph = new UndirectedGraph<TVertex, TEdge>(false, g.EdgeEqualityComparer);
                    //添加节点
                    subGraph.AddVertex(subGraphVertexs[0]);
                    subGraphs.Add(subGraph);
                }
                else if (subGraphVertexs.Count > 1)
                {
                    var subEdges = GetEdges(g, subGraphVertexs);
                    if (subEdges.Count > 0)
                    {
                        var subGraph = subEdges.ToUndirectedGraph<TVertex, TEdge>();
                        subGraphs.Add(subGraph);
                    }
                }
            }
            return subGraphs;
        }

        /// <summary>
        /// 获取连接的边列表
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g"></param>
        /// <param name="subGraphVertexs"></param>
        /// <returns></returns>
        private static List<TEdge> GetEdges<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, List<TVertex> subGraphVertexs) where TEdge : IEdge<TVertex>
        {
            var edges = new List<TEdge>();
            var algo = new UndirectedBreadthFirstSearchAlgorithm<TVertex, TEdge>(g);
            algo.ExamineEdge += args =>
            {
                if (args != null && !edges.Contains(args))
                    edges.Add(args);
            };
            foreach (var v in subGraphVertexs)
            {
                if (v == null) continue;
                algo.Compute(v);
                break;
            }
            return edges;
        }
    }
}
