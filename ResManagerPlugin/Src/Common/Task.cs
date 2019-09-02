// Decompiled with JetBrains decompiler
// Type: ND3DPPTCommon.Task
// Assembly: 3DPPTCommon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A434974A-D5B1-4710-9F0A-F22D02237E8F
// Assembly location: F:\NDWork\3DPPT\3DPPTPro_Scene\3DPPTPro\Project\Assets\Plugins\ThirdPlugins\3DPPTControl\3DPPTCommon.dll

using System.Collections;
using UnityEngine;

namespace ResManagerPlugin
{
  public class Task
  {
    private TaskManager.TaskState task;

    public static Task CreateTask(IEnumerator c, bool autoStart = true)
    {
      return new Task(c, autoStart);
    }

    /// Returns true if and only if the coroutine is running.  Paused tasks  
    ///             are considered to be running.
    public bool Running
    {
      get
      {
        return this.task.Running;
      }
    }

    /// Returns true if and only if the coroutine is currently paused.
    public bool Paused
    {
      get
      {
        return this.task.Paused;
      }
    }

    /// Termination event.  Triggered when the coroutine completes execution.
    public event Task.FinishedHandler Finished;

    /// Creates a new Task object for the given coroutine.  
    ///              
    ///             If autoStart is true (default) the task is automatically started  
    ///             upon construction.
    public Task(IEnumerator c, bool autoStart = true)
    {
      this.task = TaskManager.CreateTask(c);
      this.task.Finished += new TaskManager.TaskState.FinishedHandler(this.TaskFinished);
      if (!autoStart)
        return;
      this.Start();
    }

    /// Begins execution of the coroutine
    public void Start()
    {
      this.task.Start();
    }

    /// Discontinues execution of the coroutine at its next yield.
    public void Stop()
    {
      this.task.Stop();
    }

    public void Pause()
    {
      this.task.Pause();
    }

    public void Unpause()
    {
      this.task.Unpause();
    }

    public Coroutine Coroutine
    {
      get
      {
        return this.task.Coroutine;
      }
    }

    private void TaskFinished(bool manual)
    {
      // ISSUE: reference to a compiler-generated field
      Task.FinishedHandler finished = this.Finished;
      if (finished == null)
        return;
      finished(this, manual);
    }

    /// Delegate for termination subscribers.  manual is true if and only if  
    ///             the coroutine was stopped with an explicit call to Stop().
    public delegate void FinishedHandler(Task task, bool manual);
  }
}
