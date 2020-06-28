using System.Collections;
using UnityEngine;

namespace ResManagerPlugin
{
  internal class TaskManager : MonoBehaviour
  {
    private static TaskManager singleton;

    public static TaskManager.TaskState CreateTask(IEnumerator coroutine)
    {
      if ( TaskManager.singleton == null)
      {
        //Debug.Log(" Create Task !! " );
        
        TaskManager.singleton = new GameObject( typeof(TaskManager).Name ).AddComponent<TaskManager>();
      }

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
          //Debug.Log(" ## CallWrapper ## " );

          if ( this.paused )
          {
            //Debug.Log(" ## CallWrapper ## paused ! " );
            
            yield return (object) null;
          }
          else if ( e != null && e.MoveNext() )
          {
            //Debug.Log(" ## CallWrapper ## e move next ! " );
            
            yield return e.Current;
          }
          else
          {
            //Debug.Log(" ## CallWrapper ## running " );
            
            this.running = false;
          }
        }
        
        //Debug.Log(" ## CallWrapper End ## " );
        
        // ISSUE: reference to a compiler-generated field
        TaskManager.TaskState.FinishedHandler handler = this.Finished;
        if (handler != null)
          handler(this.stopped);
      }

      public delegate void FinishedHandler(bool manual);
    }
  }
}
