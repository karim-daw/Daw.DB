using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Daw.DB.GH
{
    public class Daw_DB_GhcInfo : GH_AssemblyInfo
    {
        public override string Name => "Daw.DB.GH";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("ecb09394-c703-43c3-ab71-8a253bc8ea9d");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}