using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn_MyProject
{
    public class SpaceZoneNode
    {
        public SpaceZoneNode(Space space)
        {
            Space = space;
        }

        public Space Space { get; }

        public string SpaceName => Space.Name;
        public string ZoneName => Space.Zone.Name;
    }
}
