﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ComputeGH.Utils
{
    public class GHDownloadEPW : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHDownloadEPW class.
        /// </summary>
        public GHDownloadEPW()
          : base("Download EPW", "Download EPW",
              "Download an EPW file from EnergyPlus' website. This component will open your web browser directly to EnergyPlus' website so you can download an EPW file.",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Open Browser", "Open Browser", "Connect a button to lauch your webbrowser onto the EnergyPlus website", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var launch = false;

            if (!DA.GetData(0, ref launch)) return;

            var url = "https://energyplus.net/weather";
            if (launch)
            {
                System.Diagnostics.Process.Start(url);
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fdb4b28d-727a-44b5-9676-2e3a447c4610"); }
        }
    }
}