using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

public class nickmarker : MonoBehaviour
{
    public enum TrackingStatusFilter
    {
        Tracked,
        Tracked_ExtendedTracked,
        Tracked_ExtendedTracked_Limited
    }

    /// <summary>
    /// A filter that can be set to either:
    /// - Only consider a target if it's in view (TRACKED)
    /// - Also consider the target if's outside of the view, but the environment is tracked (EXTENDED_TRACKED)
    /// - Even consider the target if tracking is in LIMITED mode, e.g. the environment is just 3dof tracked.
    /// </summary>
    public TrackingStatusFilter StatusFilter = TrackingStatusFilter.Tracked_ExtendedTracked_Limited;
    public UnityEvent OnTargetFound;
    public UnityEvent OnTargetLost;
    public LaparoCameraManager camManager;

    public TrackableBehaviour mTrackableBehaviour;
    protected TrackableBehaviour.Status m_PreviousStatus;
    protected TrackableBehaviour.Status m_NewStatus;
    protected bool m_CallbackReceivedOnce = false;

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();

        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
        Transform childCam=this.gameObject.transform.Find("childCam");
        childCam.gameObject.GetComponent<MeshRenderer>().material=this.gameObject.GetComponent<MeshRenderer>().material;
        childCam.gameObject.GetComponent<MeshRenderer>().enabled=false;
        //childCam.gameObject.GetComponent<MeshRenderer>().active=false;
        camManager=(LaparoCameraManager)GameObject.FindObjectOfType(typeof(LaparoCameraManager));
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    void OnTrackableStatusChanged(TrackableBehaviour.StatusChangeResult statusChangeResult)
    {
        m_PreviousStatus = statusChangeResult.PreviousStatus;
        m_NewStatus = statusChangeResult.NewStatus;

        Debug.LogFormat("Trackable {0} {1} -- {2}",
            mTrackableBehaviour.TrackableName,
            mTrackableBehaviour.CurrentStatus,
            mTrackableBehaviour.CurrentStatusInfo);

        HandleTrackableStatusChanged();
    }

    protected virtual void HandleTrackableStatusChanged()
    {
        print("!!!status changed"+m_PreviousStatus+", "+m_NewStatus);

        //i actually disabled extended tracking b/c i don't want vuforia to predict donut when it's not pretty sure of the pose
        if (!(m_PreviousStatus==TrackableBehaviour.Status.TRACKED || m_PreviousStatus==TrackableBehaviour.Status.EXTENDED_TRACKED) 
        && (m_NewStatus==TrackableBehaviour.Status.TRACKED || m_NewStatus==TrackableBehaviour.Status.EXTENDED_TRACKED)){
            //it was found
            print("!!!found");
            camManager.markerFound(this);

            /*Transform colorPlane=this.gameObject.transform.Find("colorPlane");
            if (colorPlane){
                Color newColor=Color.green;
                newColor.a=0.5f;
                MeshRenderer meshRenderer=colorPlane.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer){
                    colorPlane.gameObject.GetComponent<MeshRenderer>().material.color=newColor;
                } else{
                    print("no mesh");
                }
            }*/
            OnTrackingFound();
        } else if (!(m_NewStatus==TrackableBehaviour.Status.TRACKED || m_NewStatus==TrackableBehaviour.Status.EXTENDED_TRACKED) 
        && (m_PreviousStatus==TrackableBehaviour.Status.TRACKED || m_PreviousStatus==TrackableBehaviour.Status.EXTENDED_TRACKED)){
            camManager.markerLost(this);
            print("!!!lost");
            Transform colorPlane=this.gameObject.transform.Find("colorPlane");
            if (colorPlane){
                Color newColor=Color.red;
                newColor.a=0.25f;
                MeshRenderer meshRenderer=colorPlane.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer){
                    colorPlane.gameObject.GetComponent<MeshRenderer>().material.color=newColor;
                } else{
                    print("no mesh");
                }
            }
            OnTrackingLost();
        }
        /*
        if (!ShouldBeRendered(m_PreviousStatus) &&
            ShouldBeRendered(m_NewStatus))
        {
            print("_1");
            OnTrackingFound();
        }
        else if (ShouldBeRendered(m_PreviousStatus) &&
                 !ShouldBeRendered(m_NewStatus))
        {
            print("_2");
            OnTrackingLost();
        }
        else
        {
            //print("_4");
            OnTrackingFound();
            if (!m_CallbackReceivedOnce && !ShouldBeRendered(m_NewStatus))
            {
                print("_3");
                // This is the first time we are receiving this callback, and the target is not visible yet.
                // --> Hide the augmentation.
                OnTrackingLost();
            }
        }*/

        m_CallbackReceivedOnce = true;
    }

    protected bool ShouldBeRendered(TrackableBehaviour.Status status)
    {
        if (status == TrackableBehaviour.Status.DETECTED ||
            status == TrackableBehaviour.Status.TRACKED)
        {
            // always render the augmentation when status is DETECTED or TRACKED, regardless of filter
            return true;
        }

        if (StatusFilter == TrackingStatusFilter.Tracked_ExtendedTracked)
        {
            if (status == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                // also return true if the target is extended tracked
                return true;
            }
        }

        if (StatusFilter == TrackingStatusFilter.Tracked_ExtendedTracked_Limited)
        {
            if (status == TrackableBehaviour.Status.EXTENDED_TRACKED ||
                status == TrackableBehaviour.Status.LIMITED)
            {
                // in this mode, render the augmentation even if the target's tracking status is LIMITED.
                // this is mainly recommended for Anchors.
                return true;
            }
        }

        return true;
    }

    protected virtual void OnTrackingFound()
    {
        //print("FOUND!!!");
        //((LaparoCameraManager)GameObject.FindObjectOfType(typeof(LaparoCameraManager))).markerFound(this);
        if (mTrackableBehaviour)
        {/*
            var rendererComponents = mTrackableBehaviour.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mTrackableBehaviour.GetComponentsInChildren<Collider>(true);
            var canvasComponents = mTrackableBehaviour.GetComponentsInChildren<Canvas>(true);
            
            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = true;

            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = true;

            // Enable canvas':
            foreach (var component in canvasComponents)
                component.enabled = true;
                */
        }

        if (OnTargetFound != null)
            OnTargetFound.Invoke();
    }

    protected virtual void OnTrackingLost()
    {
        /*
        print("_5");
        if (mTrackableBehaviour)
        {
            var rendererComponents = mTrackableBehaviour.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mTrackableBehaviour.GetComponentsInChildren<Collider>(true);
            var canvasComponents = mTrackableBehaviour.GetComponentsInChildren<Canvas>(true);
            
            // Disable rendering:
            foreach (var component in rendererComponents)
                component.enabled = false;

            // Disable colliders:
            foreach (var component in colliderComponents)
                component.enabled = false;

            // Disable canvas':
            foreach (var component in canvasComponents)
                component.enabled = false;
                
        }*/

        if (OnTargetLost != null)
            OnTargetLost.Invoke();
    }
}
