using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResManagerPlugin
{
    public class GameObjectHelper
    {
        public static GameObject FindGObj( string objName,GameObject[] objs )
        {
            GameObject gObj = null;

            if( string.IsNullOrEmpty(objName)
               || objs == null )
            {
                return gObj;
            }
            
            for( int i = 0; i < objs.Length; ++i )
            {
                if( objs[i].name.Equals(objName) )
                {
                    gObj = objs[i];

                    break;
                }
            }
            
            return gObj;
        }
    }
}
