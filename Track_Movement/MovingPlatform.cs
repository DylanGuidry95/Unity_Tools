using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour 
{
    #region Fields
    public float moveSpeed = 1;
	public Transform currentPoint;
    public TrackBehaviour Track;
    private bool IsStopped;
    #endregion

    #region Unity Methods
    // Use this for initialization
    void Awake () {
        StartCoroutine(InitalizeStart());
        moveSpeed = moveSpeed < 0 ? moveSpeed * -1 : moveSpeed;
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentPoint == null)
        {
            //StopAllCoroutines();
            StartCoroutine(InitalizeStart());
        }
        if (IsStopped || currentPoint == null)
            return;
        var pointData = Track.TracksPoints[Track.Points.IndexOf(currentPoint)];

        this.transform.position = Vector3.MoveTowards(this.transform.position, currentPoint.position, Time.deltaTime *
            (moveSpeed * pointData.SpeedModifier));
        if (Vector3.Distance(this.transform.position, currentPoint.position) <= 0.1f)
        {
            if (pointData.IsStoppingPoints && !IsStopped && !pointData.HasBeenReached)
            {
                pointData.HasBeenReached = true;
                StartCoroutine(StopDelay(pointData));
                return;
            }
            currentPoint = Track.GetNextPoint(currentPoint);
            pointData.HasBeenReached = false;
        }
    }
    #endregion

    IEnumerator StopDelay(TrackPoint pointData)
    {
        Stop();
        yield return new WaitForSeconds(pointData.StopDelay);
        Resume();
    }

    IEnumerator InitalizeStart()
    {
        yield return new WaitForSeconds(0.5f);
        currentPoint = Track.ClosestPoint(transform.position);
    }

    public void AddSpeed(int s)
    {
        moveSpeed = moveSpeed + s;
    }

    public void Stop()
    {
        IsStopped = true;
    }

    public void Resume()
    {
        IsStopped = false;
    }
}