﻿using System;
using System.Collections.Generic;
using System.Drawing;
using ComputeCS.Components;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace ComputeCS.Grasshopper
{
    public class ComputeSolution : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeSolution()
            : base("Compute Solution", "CFD Solution",
                "Create the Solution Parameters for a CFD Case",
                "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddIntegerParameter("CPUs", "CPUs",
                "Number of CPUs to run the simulation across. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96",
                GH_ParamAccess.item, 4);
            pManager.AddIntegerParameter("Solver", "Solver", "Select which OpenFOAM solver to use.",
                GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Case Type", "Case Type", "Available Options: SimpleCase, VirtualWindTunnel",
                GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Boundary Conditions", "Boundary Conditions", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Iterations", "Iterations",
                "Number of iterations to run.",
                GH_ParamAccess.item, 100);
            pManager.AddIntegerParameter("Number of Angles", "Number of Angles",
                "Number of Angles. Only used for Virtual Wind Tunnel. Default is 16",
                GH_ParamAccess.item, 16);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: {'setup': [...], 'fields': [...], 'presets': [...], 'caseFiles: [...]'}",
                GH_ParamAccess.item);
            /*pManager.AddTextParameter("Files", "Files",
                "Extra files to write. This input should be a list of dictionaries in the format of: {'path': 'path/to/file/on/server', 'text': 'text to write in the file.'}",
                GH_ParamAccess.list);*/

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            //pManager[8].Optional = true;

            AddNamedValues(pManager[2] as Param_Integer, _solvers);
            AddNamedValues(pManager[3] as Param_Integer, _caseTypes);
        }

        private static void AddNamedValues(Param_Integer param, List<string> values)
        {
            var index = 0;
            foreach (var value in values)
            {
                param.AddNamedValue(value, index);
                index++;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            var cpus = 4;

            var solver = 0;
            var caseType = 0;
            var boundaryConditions = new List<string>();

            var iterations = 100;

            var numberOfAngles = 16;
            string overrides = null;
            List<string> files = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref cpus)) return;
            DA.GetData(2, ref solver);
            DA.GetData(3, ref caseType);
            if (!DA.GetDataList(4, boundaryConditions)) return;
            DA.GetData(5, ref iterations);
            DA.GetData(6, ref numberOfAngles);
            DA.GetData(7, ref overrides);
            //DA.GetData(8, ref files);
            var _iterations = $"{{'init': {iterations}}}";

            var outputs = CFDSolution.Setup(
                inputJson,
                _validateCPUs(cpus),
                _solvers[solver],
                _caseTypes[caseType],
                boundaryConditions,
                _iterations,
                numberOfAngles,
                overrides,
                files
            );


            DA.SetData(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                if (Environment.GetEnvironmentVariable("RIDER") == "true")
                {
                    return null;
                }

                return Resources.IconSolver;
            }
        }

        private static List<string> _solvers = new List<string>
        {
            "simpleFoam",
            "potentialFoam",
            "pisoFoam",
            "buoyantSimpleFoam",
            "buoyantPimpleFoam",
            "buoyantBoussinesqSimpleFoam",
            "buoyantBoussinesqPimpleFoam"
        };

        private static List<string> _caseTypes = new List<string>
        {
            "SimpleCase", "VirtualWindTunnel"
        };

        private static List<int> _validateCPUs(int cpus)
        {
            if (cpus == 1)
            {
                return new List<int> {1, 1, 1};
            }
            if (cpus == 2)
            {
                return new List<int> {2, 1, 1};
            }
            if (cpus == 4)
            {
                return new List<int> {2, 2, 1};
            }
            if (cpus == 8)
            {
                return new List<int> {4, 2, 1};
            }
            if (cpus == 16)
            {
                return new List<int> {4, 4, 1};
            }
            if (cpus == 18)
            {
                return new List<int> {6, 3, 1};
            }
            if (cpus == 24)
            {
                return new List<int> {6, 4, 1};
            }
            if (cpus == 36)
            {
                return new List<int> {6, 6, 1};
            }
            if (cpus == 48)
            {
                return new List<int> {12, 4, 1};
            }
            if (cpus == 64)
            {
                return new List<int> {8, 8, 1};
            }
            if (cpus == 72)
            {
                return new List<int> {12, 6, 1};
            }
            if (cpus == 96)
            {
                return new List<int> {12, 8, 1};
            }

            throw new Exception($"Number of CPUs ({cpus}) were not valid. Valid choices are: 1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96");
        } 

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("31b37cda-f5cc-4ffe-86b2-00bd457e4311"); }
        }
    }
}