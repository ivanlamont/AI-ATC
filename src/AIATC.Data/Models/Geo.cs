namespace AIATC.Data.Models;

using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;

using Comms;
using Ground;
using Routing;
using Airspace;
using Navigation;

public abstract class Geo : Record424
{
    public Coordinates Coordinates { get; set; }
}
