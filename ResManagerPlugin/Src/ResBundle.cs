using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResManagerPlugin
{
    public class ResBundle
    {
        public string Name;
        public List<ResUnit> ResUnits = new List<ResUnit>();
        public int ResUnitCount;

        public bool isLoaded = false;
        private AssetBundle _resAB;
        public AssetBundle ResAB
        {
            get { return _resAB; }
            set { _resAB = value; }
        }

        private Task _unLoadTask;
        
        public void Release()
        {
            isLoaded = false;

            if( ResUnits != null 
               && ResUnits.Count > 0 )
            {
                ResUnits.Clear();
            }

            if ( _unLoadTask != null )
            {
                _unLoadTask.Stop();
            }

            if( _resAB != null )
            {
                _resAB.Unload(true);
            }
        }

        public ResBundle()
        {
            if ( _unLoadTask == null )
            {
                _unLoadTask = Task.CreateTask( UnLoadAction() );
            }
        }

        private IEnumerator UnLoadAction()
        {
            while ( true )
            {
                if ( _resAB != null 
                     && isLoaded )
                {
                    _resAB.Unload( false );
                    
                    isLoaded = false;
                    
                    yield break;
                }
                yield return null;
            }
        }
        
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
