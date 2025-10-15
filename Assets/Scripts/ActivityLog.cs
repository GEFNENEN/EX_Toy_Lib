using EXToyLib;
using UnityEngine;


public class ActivityLog : BaseActivity
{
    public ActivityLog(int id, float duration) : base(id, duration)
    {
    }
    
    public override void OnStart()
    {
        Debug.Log($"Activity {ID} started with duration {Duration}");
    }
    
    public override void OnUpdate()
    {
        Debug.Log($"Activity {ID} is updating. Elapsed time: {_elapsed}");
    }
    
    public override void OnComplete()
    {
        Debug.Log($"Activity {ID} completed after {_elapsed} seconds.");
        base.OnComplete();
    }

    public override void OnInterrupt()
    {
        Debug.Log($"Activity {ID} interrupt after {_elapsed} seconds.");
        base.OnInterrupt();
    }
}
