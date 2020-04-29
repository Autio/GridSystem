using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job 
{
    // Holds information for a queued up job, including
    // placing furniture, moving stored inventory, working at a location

    // What tile is the job for?
    public Tile tile { get; protected set; }
    float jobTime; // Time until job is complete

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancel;

    public Job(Tile tile, Action<Job> cbJobComplete, float jobTime = 1f)
    {
        this.tile = tile;
        this.cbJobComplete += cbJobComplete;
    }

    public void RegisterJobCompleteCallback(Action<Job> cb)
    {
        cbJobComplete += cb;
    }

    public void RegisterJobCancelCallback(Action<Job> cb)
    {
        cbJobCancel += cb;
    }

    public void DoWork(float workTime)
    {
        jobTime -= workTime;

        if (jobTime <= 0)
        {
            if (cbJobComplete != null)
            {
                cbJobComplete(this);
            }
        }
    }

    public void CancelJob()
    {
        if(cbJobCancel != null)
        {
            cbJobCancel(this);
        }
    }
}

