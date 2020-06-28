using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace ResManagerPlugin
{
    public class ResManager
    {
        public static bool isShowDebugInfo = true;

        private static Dictionary< string, ResBundle > _resCache = new Dictionary<string, ResBundle>();

        public static void Release()
        {
            if( _resCache != null 
                && _resCache.Keys.Count > 0 )
            {
                foreach( var key in _resCache.Keys )
                {
                    _resCache[key].Release();
                }

                _resCache.Clear();
            }
        }
        
        /// <summary>
        /// 异步加载 Assetbundles
        /// </summary>
        /// <param name="abPath"> Assetbundles 路径 </param>
        /// <param name="abNames"> Assetbundles 名称 </param>
        /// <param name="callback"> 加载完一个回调 param1 状态码, param2 对象们 </param>
        public static IEnumerator AsyncLoadScenes( string abPath, string[] abNames = null, Action<int,GameObject[]> callback = null )
        {
            if ( !Directory.Exists( abPath ) )
            {
                yield break;
            }

            if ( abNames != null 
                && abNames.Length > 0 )
            {
                for ( int i = 0; i < abNames.Length; ++i )
                {
                    if ( string.IsNullOrEmpty( abNames[i] ) )
                    {
                        continue;
                    }

                    string fullPath = Path.Combine( abPath, abNames[i] );
                    
                    if ( File.Exists( fullPath ) )
                    {
                        Debug.Log(" ## Uni Output ## cls:ResManager func:LoadAssetBundles info: load ab Path " + fullPath );
                        
                        Task.CreateTask( AsyncLoadScene( abNames[i], fullPath, ( state, gObjs ) =>
                        {
                            if ( callback != null )
                            {
                                callback( state, gObjs );
                            }
                        }));
                    }
                    else
                    {
                        Debug.LogError( " ## Uni Output ## cls:ResManager func:LoadAssetBundles info: ab file not exist " + fullPath );
                    }
                }
            }
        }
        
        /// <summary>
        /// 加载 Assetbundle
        /// </summary>
        /// <param name="abPath"> Assetbundle 路径 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="objName"> 对象名 </param>
        public static ResUnit Load<T>( string abPath, string abName, string objName ) where T : UnityEngine.Object
        {
            ResUnit unit = null;
            
            string fullPath = Path.Combine( abPath, abName );

            if( _resCache != null 
                && _resCache.ContainsKey( abName ) )
            {
                unit = _resCache[ abName ].FindResUnit<T>( objName );
                
                return unit;
            }

            ResBundle resBundle = new ResBundle();
            resBundle.Name = abName;
            
            AssetBundle ab = AssetBundle.LoadFromFile( fullPath );
            
            if( ab != null )
            {
                var objs = ab.LoadAllAssets();
                
                if ( objs != null )
                {
                    resBundle.ResUnitCount = objs.Length;
                    
                    for ( int i = 0; i < objs.Length; ++i )
                    {
                        ResUnit u = new ResUnit();
                        
                        u.Progress = 1;
                        u.ResName  = objs[i].name;
                        u.ResObj   = objs[i];
                        
                        if( objs[i] == null )
                        {
                            Debug.LogError(" ## Uni Output ## cls:ResManager func:Load info: load obj error ! ");
                        }

                        if ( u.ResName == objName )
                        {                            
                            unit = u;
                        }
                        
                        resBundle.ResUnits.Add( u );
                    }
                }

                _resCache.Add( abName, resBundle );
                //ab.Unload( false );
            }
            
            return unit;
        }

        /// <summary>
        /// 异步加载 Assetbundles
        /// </summary>
        /// <param name="abPath"> Assetbundles 路径 </param>
        /// <param name="abNames"> Assetbundles 名称 </param>
        /// <param name="callback"> 加载完一个回调 param1 状态码, param2 资源 </param>
        public static IEnumerator AsyncLoadAssets( string abPath, string[] abNames, Action<int, ResUnit> callback = null )
        {
            if ( !Directory.Exists( abPath ) )
            {
                yield break;
            }

            for ( int i = 0; i < abNames.Length; ++i )
            {
                string fullPath = Path.Combine( abPath, abNames[i] );

                if ( !File.Exists( fullPath ) )
                {
                   continue; 
                }
                
                AsyncLoad( abPath, abNames[i], (state, unit) =>
                {
                    if ( callback != null )
                    {
                        callback(state, unit);
                    }
                });
            }
        }
        
        /// <summary>
        /// 加载 Assetbundle
        /// </summary>
        /// <param name="abPath"> Assetbundle 路径 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        public static void AsyncLoad( string abPath, string abName, Action<int, ResUnit> callback = null )
        {
            string fullPath = Path.Combine( abPath, abName );

            if( isShowDebugInfo )
            {
                Debug.Log( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoad info: load ab fullpath " + fullPath );
            }

            if ( Application.platform == RuntimePlatform.WindowsEditor 
                || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                if ( !File.Exists( fullPath ) )
                {
                    if ( isShowDebugInfo )
                    {
                        Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoad info: ab fullpath is not exist " + fullPath);
                    }

                    return;
                }

            }

            Task.CreateTask( AsyncLoadAllAssets( abName, fullPath, callback ));
        }

        /// <summary>
        /// 加载 Assetbundle 中全部对象
        /// </summary>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="fullPath"> Assetbundle 文件路径 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        /// <returns></returns>
        private static IEnumerator AsyncLoadAllAssets( string abName, string fullPath, Action<int, ResUnit> callback = null )
        {
            if ( string.IsNullOrEmpty( abName ) )
            {
                if ( isShowDebugInfo )
                {
                    Debug.LogWarning( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: ab name is empty " );
                }

                yield break;
            }

            if ( isShowDebugInfo )
            {
                Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: ab name " + abName );
            }

            // Assetbundle 未加载完成，等待加载完成
            while ( _resCache != null
                    && _resCache.ContainsKey( abName )
                    && _resCache[ abName ].ResUnits.Count <= 0 )
            {
                yield return null;
            }

            ResBundle resBundle = null;
            
            // 首次加载 Assetbundle
            if ( _resCache != null 
                && !_resCache.ContainsKey( abName ) )
            {
                resBundle = new ResBundle();
                resBundle.Name = abName;

                List<ResUnit> resUnits = new List<ResUnit>();

                resBundle.ResUnits = resUnits;

                if ( _resCache != null )
                {
                    _resCache.Add(abName, resBundle);
                }
            }
            else// 再次加载 Assetbundle
            {
                resBundle = _resCache[ abName ];

                // 对象还未加载，等待加载完成
                while ( ( resBundle.ResUnits.Count <= 0
                    || ( resBundle.ResUnits.Count > 0
                        && resBundle.ResUnits.Count < resBundle.ResUnitCount ) ) )
                {
                    resBundle = _resCache[abName];

                    yield return null;
                }

                if ( isShowDebugInfo )
                {
                    Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: all objs Count " + resBundle.ResUnits.Count );
                }

                // 
                for ( int i = 0; i < resBundle.ResUnits.Count; ++i )
                {
                    if ( isShowDebugInfo )
                    {
                        Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: cache obj Name " + resBundle.ResUnits[i].ResName );
                    }
                    
                    if ( callback != null )
                    {
                        callback( 1, resBundle.ResUnits[i] );
                    }

                    yield return null;
                }
                
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( fullPath );

            while ( !request.isDone )
            {
                yield return null;
            }

            var bundles = request.assetBundle;
            
            if ( bundles != null )
            {
                resBundle.ResAB = bundles;
                
                var objs = bundles.LoadAllAssets();

                if ( objs != null )
                {
                    if( resBundle != null )
                    {
                        if ( isShowDebugInfo )
                        {
                            UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: all objs Count " + objs.Length );
                        }

                        resBundle.ResUnitCount = objs.Length;
                    }

                    for ( int i = 0; i < objs.Length; ++i )
                    {
                        if( resBundle != null 
                           && !resBundle.ContainResUnit( objs[i].name, objs[i].GetType() ) ) 
                        {
                            ResUnit unit = new ResUnit
                            {
                                ResName = objs[i].name,
                                ResObj  = objs[i],
                                Progress = 1
                            };

                            resBundle.ResUnits.Add(unit);

                            if ( isShowDebugInfo )
                            {
                                Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: load obj Type " + unit.ResObj.GetType() );
                                Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: load obj Name " + unit.ResName );
                            }

                            if ( callback != null )
                            {
                                callback( 1, unit );
                            }
                        }
                    }

                    if ( resBundle != null )
                    {
                        resBundle.isLoaded = true;
                    }
                }
                else
                {
                    if ( isShowDebugInfo )
                    {
                        UnityEngine.Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: all objs Count " + resBundle.ResUnits.Count);
                    }

                    callback(0, null);
                }
            }
            else
            {
                callback(0, null);
            }
        }

        /// <summary>
        /// 加载 Assetbundle 中加载指定对象
        /// </summary>
        /// <param name="objName"> 对象名称 </param>
        /// <param name="abPath"> Assetbundle 路径 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        public static void AsyncLoadAssetByKey<T>( string objName, string abPath, string abName, Action<int,ResBundle,ResUnit> callback = null ) where T : UnityEngine.Object
        {
            if ( string.IsNullOrEmpty( objName ) )
            {
                return;
            }
            
            string fullPath = Path.Combine( abPath ,abName );
            
            if ( isShowDebugInfo )
            {
                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: async load asset name " + objName);
            }

            if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                if ( !File.Exists( fullPath ) )
                {
                    UnityEngine.Debug.LogError( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: ab fullpath is not exist " + fullPath );
                 
                    return;
                }
            }

            Task.CreateTask( AsyncLoadByKey<T>( abName, objName, fullPath, callback ) );
        }

        /// <summary>
        /// 异步加载对象
        /// </summary>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="objName"> Assetbundle 中对象名称 </param>
        /// <param name="fullPath"> Assetbundle 文件路径 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        /// <returns></returns>
        private static IEnumerator AsyncLoadByKey<T>(string abName, string objName, string fullPath,
            Action<int, ResBundle, ResUnit> callback = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(abName))
            {
                if (isShowDebugInfo)
                {
                    UnityEngine.Debug.Log(
                        " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: ab name is empty ");
                }

                yield break;
            }

            if (isShowDebugInfo)
            {
                UnityEngine.Debug.Log(
                    " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: ab name is " +
                    abName);
            }

            // 对象还未加载，等待加载完成
            while (_resCache != null
                   && _resCache.ContainsKey(abName)
                   && !_resCache[abName].ContainResUnit<T>(objName))
            {
                if ( isShowDebugInfo )
                {
                    UnityEngine.Debug.Log(
                        " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: waiting asset " +
                        objName);
                }

                yield return null;
            }
            
            ResBundle resBundle = null;

            if (_resCache != null && !_resCache.ContainsKey(abName))
            {
                if (isShowDebugInfo)
                {
                    UnityEngine.Debug.Log(
                        " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: new res bundle ");
                }

                resBundle = new ResBundle()
                {
                    Name = abName
                };

                _resCache.Add(abName, resBundle);
            }
            else
            {
                if (isShowDebugInfo)
                {
                    UnityEngine.Debug.Log(
                        " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: load from cache res bundle ");
                    UnityEngine.Debug.Log(
                        " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: load from cache res num " +
                        _resCache.Count);
                }

                resBundle = _resCache[abName];

                // 对象还未加载，阻塞流程
                while (resBundle.ResUnits == null
                       || resBundle.ResUnits.Count < 0
                       || resBundle.ResUnits.Count > 0 && resBundle.ResUnits.Count < resBundle.ResUnitCount)
                {
                    resBundle = _resCache[abName];

                    yield return null;
                }

                if ( isShowDebugInfo )
                {
                    UnityEngine.Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: load cache obj name " + objName);
                }

                if (callback != null)
                {
                    callback(1, resBundle, resBundle.FindResUnit<T>(objName));
                }

                yield break;
            }

            /*UnityEngine.Debug.Log(
                " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: load from file bundle " +
                fullPath);*/

            //WWW request = WWW.LoadFromCacheOrDownload(fullPath, 1);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( fullPath );

            while ( !request.isDone )
            {
                ResUnit unit = resBundle.FindResUnit<T>(objName);
                if (unit != null)
                {
                    unit.Progress = request.progress;

                    Debug.Log(" load ab progress ++ " + unit.Progress);
                }
                
                yield return null;
            }

            /*if ( request != null && !string.IsNullOrEmpty( request.error ) )
            {
                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: load from file bundle err !! " + request.error );                
            }*/
    
            var bundles = request.assetBundle;
            
            if ( bundles != null )
            {
                var objs = bundles.LoadAllAssets();
                
                if ( objs != null )
                {
                    if ( isShowDebugInfo )
                        UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: total obj num  " + objs.Length );

                    for ( int i = 0; i < objs.Length; ++i )
                    {
                        if( resBundle != null
                           && !resBundle.ContainResUnit<T>( objs[i].name ) )
                        {
                            T resObj = objs[i] as T;
                            
                            ResUnit u = new ResUnit()
                            {
                                ResName = objs[i].name,
                                Progress = 1f,
                                ResObj = ( resObj == null ) ? objs[i] : resObj,
                            };

                            if ( u.ResObj == null )
                            {
                                UnityEngine.Debug.LogError(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: cache obj error " + objs[i].name );
                            }
                            
                            resBundle.ResAB = bundles;
                            resBundle.ResUnits.Add(u);

                            if ( isShowDebugInfo )
                            {
                                UnityEngine.Debug.Log(
                                    " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: obj index " +
                                    i );
                                
                                UnityEngine.Debug.Log(
                                    " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: obj name " +
                                    objs[i].name );
                            }

                            if ( objName.Equals( objs[i].name ) )
                            {
                                if ( isShowDebugInfo )
                                {
                                    UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: target obj type " + objs[i].GetType().Name );
                                    UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: target obj name " + objName);
                                }

                                if ( callback != null )
                                {
                                    callback( 1, resBundle ,u );
                                }
                            }
                        }
                    }

                    if ( resBundle != null )
                    {
                        resBundle.isLoaded = true;
                    }
                }
                else
                {
                    callback( 0, resBundle , null );
                }

                //bundles.Unload(false);
            }
            else
            {
                callback( 0, resBundle, null );
            }
            
            //request.Dispose();
            request = null;
        }
        
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="abPath"> Assetbundle 路径 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        public static void LoadScene( string abPath, string abName, Action<int, GameObject[]> callback = null )
        {
            string fullPath = abPath + abName;

            if ( isShowDebugInfo )
            {
                Debug.Log("## Uni Output ## module:ResManagerPlugin cls:ResManager func:LoadScene info: scene assetbudnle fullPath " + fullPath);
            }

            if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                if ( !File.Exists( fullPath ) )
                {
                    if ( isShowDebugInfo )
                    {
                        Debug.LogWarning("## Uni Output ## module:ResManagerPlugin cls:ResManager func:LoadScene info: assetbudnle is not exist " + fullPath);
                    }

                    return;
                }
            }

            Task.CreateTask( AsyncLoadScene( abName, fullPath, callback ) );
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="key"> 场景名称 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="fullPath"> Assetbundle 路径 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        /// <returns></returns>
        private static IEnumerator AsyncLoadScene( string abName, string fullPath, Action<int, GameObject[]> callback = null )
        {
            if ( isShowDebugInfo )
            {
                Debug.Log(" ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: scene assetbudnle fullPath " + fullPath);
            }

            while ( _resCache != null
                    && _resCache.ContainsKey( abName )
                    && _resCache[ abName ].ResUnits.Count <= 0 )
            {
                yield return null;
            }
            
            ResBundle resBundle = null;
            
            if ( _resCache != null && !_resCache.ContainsKey( abName ) )
            {
                resBundle = new ResBundle()
                {
                    Name = abName
                };

                ResUnit unit = new ResUnit
                {
                    ResName = abName
                };

                resBundle.ResUnits.Add(unit);

                if ( isShowDebugInfo )
                {
                    Debug.Log( " ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load scene name " + unit.ResName );
                }

                _resCache.Add( abName, resBundle );
            }
            else
            {
                resBundle = _resCache[abName];

                while ( resBundle == null 
                    || resBundle.ResUnits.Count == 0
                    || resBundle.ResUnits.Count > 0 && resBundle.ResUnits.Count < resBundle.ResUnitCount  )
                {
                    resBundle = _resCache[abName];

                    yield return null;
                }

                ResUnit unit = resBundle.FindResUnit(abName);

                if ( isShowDebugInfo )
                {
                    Debug.Log(" ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load cache scene name " + unit.ResName );
                }

                if ( callback != null )
                {
                    callback( 0, (GameObject[])unit.ResObj );
                }
            
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( fullPath );

            while ( !request.isDone )
            {
                yield return null;
            }

            AssetBundle bundles = request.assetBundle;

            if ( bundles != null )
            {
                string[] scenePaths = bundles.GetAllScenePaths();
                
                if ( scenePaths != null )
                {
                    if ( isShowDebugInfo )
                    {
                        Debug.Log(" ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load scene count " + scenePaths.Length );
                    }

                    if ( resBundle != null )
                    {
                        resBundle.ResAB = bundles;
                        resBundle.ResUnitCount = scenePaths.Length;
                    }

                    for ( int i = 0; i < scenePaths.Length; ++i )
                    {
                        string scenePath = scenePaths[i];

                        if ( !string.IsNullOrEmpty( scenePath ) )
                        {
                            Scene srcScene = SceneManager.GetActiveScene();
                            
                            AsyncOperation asyncOp = SceneManager.LoadSceneAsync( scenePath, LoadSceneMode.Additive );

                            while ( !asyncOp.isDone )
                            {
                                yield return null;
                            }
                            
                            Scene destScene = SceneManager.GetSceneByPath(scenePath);

                            while ( !destScene.isLoaded )
                            {
                                yield return null;
                            }
                            
                            SceneManager.MergeScenes(srcScene,destScene);

                            SceneManager.SetActiveScene(destScene);

                            ResUnit resUnit = new ResUnit
                            {
                                ResName  = scenePath,
                                Progress = 1f,
                                ResObj   = destScene.GetRootGameObjects()
                            };

                            if( resBundle != null 
                               && !resBundle.ContainResUnit(scenePath) )
                            {
                                resBundle.ResUnits.Add( resUnit );
                            }

                            if ( isShowDebugInfo )
                            {
                                Debug.Log( " ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load scene name " + resUnit.ResName );
                            }

                            if ( callback != null )
                            {
                                callback( 1, resUnit.ResObj as GameObject[] );
                            }
                        }
                    }

                    if ( resBundle != null )
                    {
                        resBundle.isLoaded = true;
                    }
                }

                //bundles.Unload(false);
            }
        }
    }
}
