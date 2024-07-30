using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace Daw.DB.GH
{
  public class Daw_DB_GHInfo : GH_AssemblyInfo
  {
    public override string Name => "Daw.DB.GH Info";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("5f7a4f7b-a573-40d3-af68-e91f3c874c80");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";
  }
}