using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResManagerPlugin
{
    public class ResBundle
    {
        public string Name;
        public List<ResUnit> ResUnits = new List<ResUnit>();
        public int ResUnitCount;

        public bool ContainResUnit( string name )
        {
            bool res = false;

            if( ResUnits != null )
            {
                if( ResUnits.Find( u => u.ResName.Equals(name) ) != null )
                {
                    res = true;
                }
            }

            return res;
        }

        public ResUnit FindResUnit( string name )
        {
            ResUnit unit = null;

            if( ResUnits != null )
            {
                unit = ResUnits.Find( u => u.ResName.Equals( name ) );
            }

            return unit;
        }
    }
}
