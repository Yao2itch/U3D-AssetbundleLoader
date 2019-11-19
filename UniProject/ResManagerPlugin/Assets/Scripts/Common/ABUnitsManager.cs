using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Common
{
    public class ABUnitsManager
    {
        private static ABUnitsManager _instance;

        public static ABUnitsManager Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new ABUnitsManager();
                }
                return _instance;
            }
        }

        private ABUnit _sceneABUnit;

        public ABUnit SceneABUnit
        {
            get { return _sceneABUnit; }
            set { _sceneABUnit = value; }
        }
        
        private List<ABUnit> _abUnits = new List<ABUnit>();

        public List<ABUnit> ABUnits
        {
            get { return _abUnits; }
        }
        
        public ABUnitsManager()
        {
            _sceneABUnit = new ABUnit();
        }
        
        public void ParseConfig( string data )
        {
            if ( string.IsNullOrEmpty( data ) )
            {
                return;
            }
            
            JObject jObj = JObject.Parse( data );
            
            if ( jObj != null )
            {
                JToken token = null;

                if ( jObj.TryGetValue( "SceneAB", out token ) )
                {
                    JObject jobj = token.ToObject<JObject>();
                    
                    if (jobj.TryGetValue("ABName", out token))
                    {
                        _sceneABUnit.ABName = token.ToString();

                        Debug.Log("## Uni Output ## cls:ABUnitsManager func:ParseConfig info: ab Name " +
                                  _sceneABUnit.ABName);
                    }

                    if (jobj.TryGetValue("ABPath", out token))
                    {
                        _sceneABUnit.ABPath = token.ToString() + "\\";

                        Debug.Log("## Uni Output ## cls:ABUnitsManager func:ParseConfig info: ab Path " +
                                  _sceneABUnit.ABPath);
                    }
                }

                if ( jObj.TryGetValue( "ABs", out token ) )
                {
                    JArray jArray = token.ToObject<JArray>();
                    
                    if ( jArray != null )
                    {
                        for ( int i = 0; i < jArray.Count; ++i )
                        {
                            JObject abObj = jArray[i].ToObject<JObject>();
                            
                            if ( abObj != null )
                            {
                                ABUnit unit = new ABUnit();
                                
                                if ( abObj.TryGetValue( "ABName", out token ) )
                                {
                                    unit.ABName = token.ToString();
                                    
                                    Debug.Log("## Uni Output ## cls:ABUnitsManager func:ParseConfig info: comm ab Name " + unit.ABName);
                                }
                                
                                if ( abObj.TryGetValue( "ABPath", out token ) )
                                {
                                    unit.ABPath = token.ToString() + "\\";
                                    
                                    Debug.Log("## Uni Output ## cls:ABUnitsManager func:ParseConfig info: comm ab Path " + unit.ABPath);
                                }

                                if ( _abUnits != null
                                    && !_abUnits.Contains( unit ) )
                                {
                                    _abUnits.Add( unit );
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}