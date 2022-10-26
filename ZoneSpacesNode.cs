using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn_MyProject
{
    public class ZoneSpacesNode
    {
        public ZoneSpacesNode(Zone zone)
        {
            Zone = zone;

            SpaceList = new List<Space>();
            foreach (Space space in Zone.Spaces)
            {
                SpaceList.Add(space);
            }
        }

        public Zone Zone { get; }

        public string ZoneName => Zone.Name;

        public List<Space> SpaceList { get; set; }
    }
}
