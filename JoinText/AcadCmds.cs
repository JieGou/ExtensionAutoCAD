﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices;
using AcadGeo = Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using QuickGraph.Algorithms.ConnectedComponents;
using QuickGraph;

[assembly: CommandClass(typeof(RoutingSolid.AcadCmds))]

namespace RoutingSolid
{
    public class AcadCmds
    {
        //<image url="$(ProjectDir)\DocumentImages\originalLineRouter.png"/>
        //创建后
        //<image url="$(ProjectDir)\DocumentImages\SolidRouter.png"/>

        /// <summary>
        /// 创建管网三维实体
        /// </summary>
        [CommandMethod("RoutingSolid")]
        public void RoutingSolid()
        {
            try
            {
                AcadFuncs.GetEditor().WriteMessage("dev by [gdt.anv@gmail.com]");
                Model model = new Model();
                model.BuildModel();
                //检查图的 Component个数
                var dfs = new ConnectedComponentsAlgorithm<Node, Connection>(model.routers);
                dfs.Compute();
                if (dfs.ComponentCount <= 1)
                {
                    model.BuildProfile();
                    model.BuildRoutingSolid();
                    model.RoutingSolid();
                }
                else
                {
                    // 分别得到各Component的图
                    List<IUndirectedGraph<Node, Connection>> subGraphs = GraphComponentUtils.GetSubComponentGraphs(model.routers, dfs);
                    foreach (UndirectedGraph<Node, Connection> subGraph in subGraphs)
                    {
                        Model submodel = new Model
                        {
                            routers = subGraph
                        };
                        submodel.BuildProfile();
                        submodel.BuildRoutingSolid();
                        submodel.RoutingSolid();
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
            }
            finally
            {
            }
        }

        public void MySort(ref List<JoinEnt> jes)
        {
            if (jes.Count < 2)
                return;

            for (int i = 0; i < jes.Count; i++)
            {
                for (int j = i + 1; j < jes.Count; j++)
                {
                    if (1 == jes[i].CompareTo(jes[j]))
                    {
                        JoinEnt tmp = jes[i];

                        jes[i] = jes[j];
                        jes[j] = tmp;
                    }
                }
            }
        }

        public static double delta_y = 1.0;

        [CommandMethod("SetupDeltaY")]
        public void SetupDeltaY()
        {
            AcadFuncs.GetDouble(ref delta_y, "Nhap khoang cach:");
        }

        private AcadGeo.Point3d GetPosition(ObjectId id, out bool valid)
        {
            using (Transaction tr = AcadFuncs.GetActiveDB().TransactionManager.StartTransaction())
            {
                valid = true;
                var obj = tr.GetObject(id, OpenMode.ForWrite);
                DBText text = obj as DBText;
                if (null != text)
                {
                    text.Justify = AttachmentPoint.BottomLeft;
                    return text.Position;
                }

                MText mtext = obj as MText;
                if (null != mtext)
                {
                    mtext.SetAttachmentMovingLocation(AttachmentPoint.BottomLeft);
                    return mtext.Location;
                }

                valid = false;
                return new AcadGeo.Point3d();
            }
        }

        private string GetContent(ObjectId id)
        {
            using (Transaction tr = AcadFuncs.GetActiveDB().TransactionManager.StartTransaction())
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                DBText text = obj as DBText;
                if (null != text)
                {
                    return text.TextString;
                }

                MText mtext = obj as MText;
                if (null != mtext)
                    return mtext.Text;
            }

            return "";
        }

        [CommandMethod("JoinText")]
        public void JoinText()
        {
            var ent_ids = AcadFuncs.PickEnts();
            if (0 == ent_ids.Count)
                return;

            AcadGeo.Point3d ins_pnt = new AcadGeo.Point3d();
            if (!AcadFuncs.GetPoint(ref ins_pnt, "Chon vi tri text:"))//选择文本位置
                return;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<JoinEnt> data = new List<JoinEnt>();
            for (int i = 0; i < ent_ids.Count; i++)
            {
                bool valid = true;
                JoinEnt je = new JoinEnt(ent_ids[i], GetPosition(ent_ids[i], out valid));
                if (valid)
                    data.Add(je);
            }

            data.Sort((item1, item2) => item1.pos.Y.CompareTo(item2.pos.Y));
            data.Sort();
            //MySort(ref data);

            while (data.Count > 0)
            {
                JoinEnt je = data[0];
                data.RemoveAt(0);

                List<JoinEnt> ents = new List<JoinEnt>();
                ents.Add(je);
                while (data.Count > 0)
                {
                    bool export = false;
                    if (Math.Abs(data[0].pos.Y - je.pos.Y) < delta_y)
                    {
                        ents.Add(data[0]);
                        data.RemoveAt(0);
                    }
                    else
                        export = true;

                    if (0 == data.Count || export)
                    {
                        ents.Sort((item1, item2) => item1.pos.X.CompareTo(item2.pos.X));

                        DBText text = new DBText();
                        text.TextString += GetContent(ents[0].id);
                        text.TextString += "_";

                        for (int i = 1; i < ents.Count; i++)
                        {
                            text.TextString += GetContent(ents[i].id);
                            text.TextString += "x";
                        }

                        text.TextString = text.TextString.Substring(0, text.TextString.Count() - 1);
                        text.Position = ins_pnt;
                        text.Height = 1.0;
                        ins_pnt = new AcadGeo.Point3d(ins_pnt.X, ins_pnt.Y + text.Height * 2.0, ins_pnt.Z);
                        AcadFuncs.AddNewEnt(text);
                        break;
                    }
                }
            }

            AcadFuncs.GetEditor().WriteMessage("\nDone By gdt.anv@gmail.com\n");
            watch.Stop();
            AcadFuncs.GetEditor().WriteMessage(watch.ElapsedMilliseconds.ToString());
        }

        [CommandMethod("JoinText2")]
        public void JoinText2()
        {
            var ent_ids = AcadFuncs.PickEnts();
            if (0 == ent_ids.Count)
                return;

            AcadGeo.Point3d ins_pnt = new AcadGeo.Point3d();
            if (!AcadFuncs.GetPoint(ref ins_pnt, "Chon vi tri text:"))
                return;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            //C5.IntervalHeap<JoinEnt> data = new C5.IntervalHeap<JoinEnt>();
            List<JoinEnt> data = new List<JoinEnt>();
            for (int i = 0; i < ent_ids.Count; i++)
            {
                bool valid = true;
                JoinEnt je = new JoinEnt(ent_ids[i], GetPosition(ent_ids[i], out valid));
                if (valid)
                    data.Add(je);
            }

            //data.Sort((item1, item2) => item1.pos.Y.CompareTo(item2.pos.Y));
            data.Sort();

            while (data.Count > 0)
            {
                JoinEnt je = data.Min();
                //data.DeleteMin();
                data.RemoveAt(0);

                List<JoinEnt> ents = new List<JoinEnt>();
                ents.Add(je);
                while (data.Count > 0)
                {
                    bool export = false;
                    if (Math.Abs(data.Min().pos.Y - je.pos.Y) < delta_y)
                    {
                        ents.Add(data.Min());
                        //data.DeleteMin();
                    }
                    else
                        export = true;

                    if (0 == data.Count || export)
                    {
                        ents.Sort((item1, item2) => item1.pos.X.CompareTo(item2.pos.X));

                        DBText text = new DBText();
                        text.TextString += GetContent(ents[0].id);
                        text.TextString += "_";

                        for (int i = 1; i < ents.Count; i++)
                        {
                            text.TextString += GetContent(ents[i].id);
                            text.TextString += "x";
                        }

                        text.TextString = text.TextString.Substring(0, text.TextString.Count() - 1);
                        text.Position = ins_pnt;
                        text.Height = 1.0;
                        ins_pnt = new AcadGeo.Point3d(ins_pnt.X, ins_pnt.Y + text.Height * 2.0, ins_pnt.Z);
                        AcadFuncs.AddNewEnt(text);
                        break;
                    }
                }
            }

            AcadFuncs.GetEditor().WriteMessage("\nDone By gdt.anv@gmail.com\n");
            watch.Stop();
            AcadFuncs.GetEditor().WriteMessage(watch.ElapsedMilliseconds.ToString());
        }
    }

    public class JoinEnt : IComparable
    {
        public ObjectId id;
        public AcadGeo.Point3d pos;

        public JoinEnt(ObjectId _id, AcadGeo.Point3d _pos)
        {
            id = _id;
            pos = _pos;
        }

        //1 if obj is smaller than this item
        //0 if equal
        //-1 if obj is bigger

        public int CompareTo(object obj)
        {
            JoinEnt tmp = obj as JoinEnt;
            return tmp.pos.Y.CompareTo(pos.Y);
        }
    }
}
