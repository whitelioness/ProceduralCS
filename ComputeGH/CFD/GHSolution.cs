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
                "Number of CPUs to run the simulation acros. Expects a list of 3 numbers. The sum of the numbers is the amount of CPUs you want to run acros. Visit https://compute.procedural.build/pricing to see how many CPUs the different EC2 instances have. EC2 instances is the virtural machines in the cloud, on which the simulation runs.",
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("Solver", "Solver", "Select which OpenFOAM solver to use.",
                GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Case Type", "Case Type", "Available Options: SimpleCase, VirtualWindTunnel",
                GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Boundary Conditions", "Boundary Conditions", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Iterations", "Iterations",
                "Number of iterations to run. Expects recieve the JSON format: {'init': 100, 'run': '100'}. The 'run' key is optional.",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Angles", "Number of Angles",
                "Number of Angles. Only used for Virtual Wind Tunnel. Default is 16",
                GH_ParamAccess.item, 16);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: {'setup': [...], 'fields': [...], 'presets': [...], 'caseFiles: [...]'}",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Files", "Files",
                "Extra files to write. This input should be a list of dictionaries in the format of: {'path': 'path/to/file/on/server', 'text': 'text to write in the file.'}",
                GH_ParamAccess.list);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;

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
            var cpus = new List<int>();

            var solver = 0;
            var caseType = 0;
            var boundaryConditions = new List<string>();

            string iterations = null;

            var numberOfAngles = 16;
            string overrides = null;
            List<string> files = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, cpus)) return;
            DA.GetData(2, ref solver);
            DA.GetData(3, ref caseType);
            if (!DA.GetDataList(4, boundaryConditions)) return;
            if (!DA.GetData(5, ref iterations)) return;
            DA.GetData(6, ref numberOfAngles);
            DA.GetData(7, ref overrides);
            //DA.GetData(8, ref files);


            var outputs = CFDSolution.Setup(
                inputJson,
                cpus,
                _solvers[solver],
                _caseTypes[caseType],
                boundaryConditions,
                iterations,
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


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("31b37cda-f5cc-4ffe-86b2-00bd457e4311"); }
        }
    }
}