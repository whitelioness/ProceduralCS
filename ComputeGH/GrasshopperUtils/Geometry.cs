﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Microsoft.Scripting.Utils;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Geometry.Intersect;

namespace ComputeCS.Grasshopper.Utils
{
    public class Geometry
    {
        public static void checkNames(List<ObjRef> objList)
        {
            List<string> names = new List<string>();
            string objName = "";

            for (int i = 0; i < objList.Count; i++)
            {
                objName = objList[i].Object().Attributes.GetUserString("ComputeName");

                // First define a new name (if it doesnt already exist)
                if (objName == "")
                {
                    objName = Guid.NewGuid().ToString();
                }

                // Now check for uniqueness
                if (names.Contains(objList[i].Object().Attributes.GetUserString("ComputeName")))
                {
                    int counter = 1;
                    while (names.Contains(objList[i].Object().Attributes.GetUserString("ComputeName")))
                    {
                        counter++;
                    }

                    ;
                    objName = objName + "." + counter.ToString("D3");
                }

                // Set the name of the object
                objList[i].Object().Attributes.SetUserString("ComputeName", objName);
            }
        }

        public static List<double[]> pntsToArrays(List<Point3d> points)
        {
            List<double[]> pntList = new List<double[]>(points.Count);
            foreach (Point3d p in points)
            {
                pntList.Add(new double[3] {p.X, p.Y, p.Z});
            }

            return pntList;
        }

        public static bool checkName(string name)
        {
            return (name == fixName(name));
        }

        public static string fixName(string name)
        {
            if ((name == "") | name == null)
            {
                name = Guid.NewGuid().ToString();
            }

            name = name.Replace(" ", "_");
            if (Char.IsDigit(name[0]))
            {
                name = "_" + name;
            }

            return name;
        }

        public static List<ObjRef> getVisibleObjects()
        {
            List<ObjRef> objRefList = new List<ObjRef>();
            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
            settings.VisibleFilter = true;
            settings.HiddenObjects = false;

            foreach (RhinoObject rhObj in doc.Objects.GetObjectList(settings))
            {
                objRefList.Add(new ObjRef(rhObj));
            }

            return objRefList;
        }

        // getObjRef Overloaded Methods (GH_Brep and GH_Mesh)
        public static ObjRef getObjRef<T>(T ghObj) where T : IGH_GeometricGoo
        {
            return new ObjRef(ghObj.ReferenceID);
        }

        public static List<ObjRef> getObjRef<T>(List<T> ghObjList) where T : IGH_GeometricGoo
        {
            List<ObjRef> objRefList = new List<ObjRef>();
            for (int i = 0; i < ghObjList.Count; i++)
            {
                objRefList.Add(getObjRef(ghObjList[i]));
            }

            return objRefList;
        }

        // Set Overloaded Methods
        public static void setDocObjectUserString(RhinoObject docObj, string key, string value)
        {
            docObj.Attributes.SetUserString(key, value);
        }

        public static void setDocObjectUserString(ObjRef docObjRef, string key, string value)
        {
            setDocObjectUserString(docObjRef.Object(), key, value);
        }

        public static void setDocObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            setDocObjectUserString(getObjRef(ghObj), key, value);
        }

        // Get Overloaded Methods
        public static string getDocObjectUserString(RhinoObject docObj, string key)
        {
            return docObj.Attributes.GetUserString(key);
        }

        public static string getDocObjectUserString(ObjRef docObjRef, string key)
        {
            return getDocObjectUserString(docObjRef.Object(), key);
        }

        public static string getDocObjectUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            return getDocObjectUserString(getObjRef(ghObj), key);
        }

        // Get or Set Overloaded Methods
        public static string getOrSetDocObjectUserString(RhinoObject docObj, string key, string value)
        {
            string v = getDocObjectUserString(docObj, key);
            if (((v == null) | (v == "")) & !(value == null))
            {
                setDocObjectUserString(docObj, key, value);
                v = value;
            }

            return v;
        }

        public static string getOrSetDocObjectUserString(ObjRef docObjRef, string key, string value)
        {
            return getOrSetDocObjectUserString(docObjRef.Object(), key, value);
        }

        public static string getOrSetDocObjectUserString<T>(T ghObj, string key, string value)
            where T : IGH_GeometricGoo
        {
            return getOrSetDocObjectUserString(getObjRef(ghObj), key, value);
        }

        // Get Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static string getGHObjectUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            if (ghObj.TypeName == "Brep")
            {
                Brep b;
                bool success = ghObj.CastTo(out b);
                return b.GetUserString(key);
            }
            else if (ghObj.TypeName == "Mesh")
            {
                Mesh m;
                bool success = ghObj.CastTo(out m);
                return m.GetUserString(key);
            }
            else if (ghObj.TypeName == "Curve")
            {
                Curve c;
                bool success = ghObj.CastTo(out c);
                return c.GetUserString(key);
            }
            else
            {
                throw new Exception("Expected Brep, Mesh or Curve object");
            }
        }

        // Set Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static void setGHObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            bool success;
            if (ghObj.TypeName == "Brep")
            {
                Brep b;
                success = ghObj.CastTo(out b);
                b.SetUserString(key, value);
            }
            else if (ghObj.TypeName == "Mesh")
            {
                Mesh m;
                success = ghObj.CastTo(out m);
                m.SetUserString(key, value);
            }
            else
            {
                throw new Exception("Expected GH_Brep or GH_Mesh object");
            }
        }

        // Get or Set Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static string getOrSetGHObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            string v = getGHObjectUserString(ghObj, key);
            if (((v == null) | (v == "")) & !(value == null))
            {
                setGHObjectUserString(ghObj, key, value);
                v = value;
            }

            return v;
        }

        // Generic Set User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static void setUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                setDocObjectUserString(ghObj, key, value);
            }
            else
            {
                setGHObjectUserString(ghObj, key, value);
            }
        }

        // Generic Get User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static string getUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                return getDocObjectUserString(ghObj, key);
            }
            else
            {
                return getGHObjectUserString(ghObj, key);
            }
        }

        // Generic Get or Set User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static string getOrSetUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                return getOrSetDocObjectUserString(ghObj, key, value);
            }
            else
            {
                return getOrSetGHObjectUserString(ghObj, key, value);
            }
        }

        public static Dictionary<string, DataTree<object>> CreateAnalysisMesh(
            List<Surface> baseSurfaces,
            double gridSize,
            List<Brep> excludeGeometry,
            double offset,
            string offsetDirection)
        {
            var analysisMesh = new DataTree<object>();
            var faceCenters = new DataTree<object>();
            var faceNormals = new DataTree<object>();
            var index = 0;
            foreach (var surface in baseSurfaces)
            {
                var _surface = SubtractBrep(surface, excludeGeometry);
                _surface = MoveSurface(_surface, offset, offsetDirection);
                var mesh = CreateMeshFromSurface(_surface, gridSize);

                var path = new GH_Path(index);
                analysisMesh.Add(mesh, path);
                mesh.RebuildNormals();
                foreach (var normal in mesh.FaceNormals)
                {
                    faceNormals.Add(normal, path);
                }
                for (var i = 0; i < mesh.Faces.Count(); i++)
                {
                    faceCenters.Add(mesh.Faces.GetFaceCenter(i), path);
                }
                
                index++;
            }

            return new Dictionary<string, DataTree<object>>
            {
                {"analysisMesh", analysisMesh},
                {"faceCenters", faceCenters},
                {"faceNormals", faceNormals},
            };
        }

        private static Mesh CreateMeshFromSurface(Brep surface, double gridSize)
        {
            var meshParams = MeshingParameters.DefaultAnalysisMesh;
            meshParams.MaximumEdgeLength = gridSize * 1.2;
            meshParams.MinimumEdgeLength = gridSize * 0.8;

            try
            {
                return Mesh.CreateFromBrep(surface, meshParams).First();
            }
            catch
            {
                throw new Exception("Error in converting Brep to Mesh");
            }
        }

        private static Brep SubtractBrep(Surface surface, List<Brep> excludeGeometry)
        {
            var brepSurface = Brep.CreateFromSurface(surface);
            const double tolerance = 0.1;
            foreach (var brep in excludeGeometry)
            {
                var intersectionCurves = new Curve[]{};
                var intersectionPoints = new Point3d[]{};
                Intersection.BrepBrep(brep, brepSurface, tolerance, out intersectionCurves, out intersectionPoints);
                var splitFaces = brepSurface.Split(intersectionCurves, tolerance);

                brepSurface = splitFaces.First(face => !brep.IsPointInside(AreaMassProperties.Compute((Brep) face).Centroid, tolerance, false));
            }

            return brepSurface;
        }

        private static Brep MoveSurface(Brep surface, double offset, string offsetDirection)
        {
            var vector = new Vector3d();
            if (offsetDirection == "x")
            {
                vector.X = offset;
                vector.Y = 0;
                vector.Z = 0;
            } else if (offsetDirection == "y")
            {
                vector.Y = offset;
                vector.X = 0;
                vector.Z = 0;
            } else if (offsetDirection == "z")
            {
                vector.Z = offset;
                vector.Y = 0;
                vector.X = 0;
            } else if (offsetDirection == "normal")
            {
                vector = surface.Faces.FirstOrDefault().NormalAt(0.5, 0.5);
            }
             
            surface.Translate(vector);
            return surface;
        }
    }
}