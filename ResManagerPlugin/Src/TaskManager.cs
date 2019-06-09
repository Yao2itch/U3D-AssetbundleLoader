// Decompiled with JetBrains decompiler
// Type: ND3DPPTCommon.TaskManager
// Assembly: 3DPPTCommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A434974A-D5B1-4710-9F0A-F22D02237E8F
// Assembly location: F:\NDWork\3DPPT\3DPPTPro_Scene\3DPPTPro\Project\Assets\Plugins\ThirdPlugins\3DPPTControl\3DPPTCommon.dll

using System.Collections;
using UnityEngine;

namespace ResManagerPlugin
{
  internal class TaskManager : MonoBehaviour
  {
    private static TaskManager singleton;

    public static TaskManager.TaskState CreateTask(IEnumerator coroutine)
    {
      if ((Object) TaskManager.singleton == (Object) null)
        TaskManager.singleton = new GameObject(typeof(TaskManager).Name).AddComponent<TaskManager>();
      return new TaskManager.TaskState(coroutine);
    }

    public class TaskState
    {
      private IEnumerator enumerator;
      private Coroutine coroutine;
      private bool running;
      private bool paused;
      private bool stopped;

      public bool Running
      {
        get
        {
          return this.running;
        }
      }

      public bool Paused
      {
        get
        {
          return this.paused;
        }
      }

      public Coroutine Coroutine
      {
        get
        {
          return this.coroutine;
        }
      }

      public event TaskManager.TaskState.FinishedHandler Finished;

      public TaskState(IEnumerator c)
      {
        this.enumerator = c;
      }

      public void Pause()
      {
        this.paused = true;
      }

      public void Unpause()
      {
        this.paused = false;
      }

      public void Start()
      {
        this.running = true;
        this.coroutine = TaskManager.singleton.StartCoroutine(this.CallWrapper());
      }

      public void Stop()
      {
        this.stopped = true;
        this.running = false;
      }

      private IEnumerator CallWrapper()
      {
        IEnumerator e = this.enumerator;
        while (this.running)
        {
          if (this.paused)
            yield return (object) null;
          else if (e != null && e.MoveNext())
            yield return e.Current;
          else
            this.running = false;
        }
        // ISSUE: reference to a compiler-generated field
        TaskManager.TaskState.FinishedHandler handler = this.Finished;
        if (handler != null)
          handler(this.stopped);
      }

      public delegate void FinishedHandler(bool manual);
    }
  }
}
