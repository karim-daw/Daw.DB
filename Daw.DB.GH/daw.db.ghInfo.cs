using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace daw.db.gh
{
  public class daw_db_ghInfo : GH_AssemblyInfo
  {
    public override string Name => "daw.db.gh Info";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("82725b9b-7dfc-483f-81b9-979ace4f7734");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";
  }
}