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
            if( _resCache != null )
            {
                _resCache.Clear();
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
                UnityEngine.Debug.Log( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoad info: load ab fullpath " + fullPath );
            }

            if ( Application.platform == RuntimePlatform.WindowsEditor 
                || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                if ( !File.Exists(fullPath) )
                {
                    if ( isShowDebugInfo )
                    {
                        UnityEngine.Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoad info: ab fullpath is not exist " + fullPath);
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
                    UnityEngine.Debug.LogWarning( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: ab name is empty " );
                }

                yield break;
            }

            if ( isShowDebugInfo )
            {
                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: ab name " + abName );
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
            if ( _resCache != null && !_resCache.ContainsKey( abName ) )
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
                    UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: all objs Count " + resBundle.ResUnits.Count );
                }

                // 
                for ( int i = 0; i < resBundle.ResUnits.Count; ++i )
                {
                    if ( isShowDebugInfo )
                    {
                        UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: cache obj Name " + resBundle.ResUnits[i].ResName );
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
                           && !resBundle.ContainResUnit( objs[i].name ) )
                        {
                            ResUnit unit = new ResUnit()
                            {
                                ResName = objs[i].name,
                                ResObj  = objs[i],
                                Progress = 1
                            };

                            resBundle.ResUnits.Add(unit);

                            if ( isShowDebugInfo )
                            {
                                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAllAssets info: load obj Name " + unit.ResName );
                            }

                            if ( callback != null )
                            {
                                callback( 1, unit );
                            }
                        }
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

                bundles.Unload(false);
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
        public static void AsyncLoadAssetByKey<T>( string objName, string abPath, string abName, Action<int,ResUnit> callback = null ) where T : UnityEngine.Object
        {
            string fullPath = Path.Combine( abPath ,abName );

            if ( isShowDebugInfo )
            {
                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: async load ab fullpath " + fullPath);
            }

            if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                if ( !File.Exists( fullPath ) )
                {
                    if ( isShowDebugInfo )
                    {
                        UnityEngine.Debug.LogWarning( " ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: ab fullpath is not exist " + fullPath );
                    }

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
        private static IEnumerator AsyncLoadByKey<T>( string abName, string objName, string fullPath, Action<int,ResUnit> callback = null ) where T : UnityEngine.Object
        {
            if ( string.IsNullOrEmpty( abName ) )
            {
                if ( isShowDebugInfo )
                {
                    UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: ab name is empty ");
                }

                yield break;
            }

            if ( isShowDebugInfo )
            {
                UnityEngine.Debug.Log(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadByKey info: ab name is " + abName );
            }

            // 对象还未加载，等待加载完成
            while ( _resCache != null
                    && _resCache.ContainsKey( abName )
                    && !_resCache[abName].ContainResUnit( objName ) )
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
                    
                _resCache.Add( abName, resBundle );
            }
            else
            {
                resBundle = _resCache[objName];

                // 对象还未加载，阻塞流程
                while ( resBundle.ResUnits == null
                    || resBundle.ResUnits.Count < 0
                    || resBundle.ResUnits.Count > 0 && resBundle.ResUnits.Count < resBundle.ResUnitCount )
                {
                    resBundle = _resCache[objName];

                    yield return null;
                }

                if ( isShowDebugInfo )
                {
                    UnityEngine.Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: cache obj name " + objName);
                }

                if ( callback != null)
                {
                    callback( 1, resBundle.FindResUnit( objName ) );
                }

                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( fullPath );

            while ( !request.isDone )
            {
                ResUnit unit = resBundle.FindResUnit( objName );
                if( unit != null )
                {
                    unit.Progress = request.progress;
                }

                yield return null;
            }
            
            var bundles = request.assetBundle;

            if ( bundles != null )
            {
                var objs = bundles.LoadAllAssets();

                if ( objs != null )
                {
                    for ( int i = 0; i < objs.Length; ++i )
                    {
                        if( resBundle != null
                           && !resBundle.ContainResUnit( objs[i].name ) )
                        {
                            ResUnit u = new ResUnit()
                            {
                                ResName = objs[i].name,
                                Progress = 1f,
                                ResObj = objs[i] as T
                            };

                            resBundle.ResUnits.Add(u);

                            if ( objName.Equals( objs[i].name ) )
                            {
                                if ( isShowDebugInfo )
                                {
                                    UnityEngine.Debug.LogWarning(" ## Uni Output ## moudule: ResManager Plugin cls:ResManager func:AsyncLoadAssetByKey info: target obj name " + objName);
                                }

                                if ( callback != null )
                                {
                                    callback( 1, u );
                                }
                            }
                        }
                    }
                }
                else
                {
                    callback(0, null);
                }

                bundles.Unload(false);
            }
            else
            {
                callback( 0, null );
            }
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

            Task.CreateTask( AsyncLoadScene( abName, abName, fullPath, callback ) );
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="key"> 场景名称 </param>
        /// <param name="abName"> Assetbundle 名称 </param>
        /// <param name="fullPath"> Assetbundle 路径 </param>
        /// <param name="callback"> 加载完成回调 , param1 状态码, param2 资源 </param>
        /// <returns></returns>
        private static IEnumerator AsyncLoadScene( string key, string abName, string fullPath, Action<int, GameObject[]> callback = null )
        {
            if ( string.IsNullOrEmpty( key ) )
            {
                yield break;
            }

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
                    ResName = key
                };

                resBundle.ResUnits.Add(unit);

                if ( isShowDebugInfo )
                {
                    Debug.Log( " ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load scene name " + unit.ResName );
                }

                _resCache.Add(abName, resBundle);
            }
            else
            {
                resBundle = _resCache[key];

                while ( resBundle == null 
                    || resBundle.ResUnits.Count == 0
                    || resBundle.ResUnits.Count > 0 && resBundle.ResUnits.Count < resBundle.ResUnitCount  )
                {
                    resBundle = _resCache[key];

                    yield return null;
                }

                ResUnit unit = resBundle.FindResUnit(key);

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
                        resBundle.ResUnitCount = scenePaths.Length;
                    }

                    for ( int i = 0; i < scenePaths.Length; ++i )
                    {
                        string scenePath = scenePaths[i];

                        if ( !string.IsNullOrEmpty( scenePath ) )
                        {
                            UnityEngine.SceneManagement.Scene srcScene = SceneManager.GetActiveScene();
                            
                            SceneManager.LoadScene( scenePath, LoadSceneMode.Additive );

                            UnityEngine.SceneManagement.Scene destScene = SceneManager.GetSceneByPath(scenePath);

                            while ( !destScene.isLoaded )
                            {
                                yield return null;
                            }
                            
                            SceneManager.MergeScenes(srcScene,destScene);

                            SceneManager.SetActiveScene(destScene);

                            ResUnit resUnit = new ResUnit()
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

                            if (isShowDebugInfo)
                            {
                                Debug.Log( " ## Uni Output ## module:ResManagerPlugin cls:ResManager func:AsyncLoadScene info: load scene name " + resUnit.ResName );
                            }

                            if ( callback != null )
                            {
                                callback( 1, resUnit.ResObj as GameObject[] );
                            }
                        }
                    }
                }

                bundles.Unload(false);
            }
        }
    }
}
