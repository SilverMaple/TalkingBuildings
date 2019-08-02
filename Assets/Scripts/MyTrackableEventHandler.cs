using UnityEngine;
using System.Collections;
using Vuforia;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// A custom handler that implements the ITrackableEventHandler interface.
///
/// Changes made to this file could be overwritten when upgrading the Vuforia version.
/// When implementing custom event handler behavior, consider inheriting from this class instead.
/// </summary>
public class MyTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    #region PROTECTED_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;
    protected TrackableBehaviour.Status m_PreviousStatus;
    protected TrackableBehaviour.Status m_NewStatus;

    #endregion // PROTECTED_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES
    private bool initialState = false;
    #endregion

    #region UNITY_MONOBEHAVIOUR_METHODS

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
            mTrackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        m_PreviousStatus = previousStatus;
        m_NewStatus = newStatus;

        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            //Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NO_POSE)
        {
            //Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PROTECTED_METHODS

    protected virtual async void OnTrackingFound()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = true;

        //if (AppManager.Instance.commentTextArray == null)
        //{
        //    AppManager.Instance.commentTextArray = new List<UnityEngine.UI.Image>();
        //}
        //AppManager.Instance.commentTextArray.Clear();


        if (! this.initialState){
            this.initialState = true;
            /*
             * Animation Attach
             * 
             */
            if (this.gameObject.name == "ImageTarget (23)" || this.gameObject.name == "ImageTarget (30)")
            {
                Transform tmp = this.transform.Find("ButterflyPrehab");
                if (tmp == null)
                {
                    GameObject butterflyTemplate = GameObject.FindGameObjectWithTag("ButterflyPrehab");
                    if (butterflyTemplate != null)
                    {

                        GameObject butterflyObject = GameObject.Instantiate<GameObject>(butterflyTemplate);
                        butterflyObject.transform.SetParent(this.transform);
                        butterflyObject.transform.localPosition = new Vector3(0, 0.1f, 0);
                        butterflyObject.SetActive(true);
                        butterflyObject.transform.localScale = new Vector3(1, 1, 1);
                        return;
                    }
                    //butterflyObject.transform.up = imageTargets[i].up;
                    //butterflyObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(-1, 0, 0));
                    //butterflyObject.transform.localScale = new Vector3(scaleFactor * 10, scaleFactor * 10, scaleFactor * 10);
                }
                GameObject b = AppManager.Instance.butterflyAnimation;
                b.transform.SetParent(this.transform);
                b.transform.localPosition = new Vector3(0, 0.1f, 0);
                b.SetActive(true);
                b.transform.localScale = new Vector3(1, 1, 1);

            }

            /*
            * Barrage Text Attach
            * 
            */
            List<string> imageTargetComments = await AppManager.Instance.GetPlaceComment(this.gameObject.name);
            UnityEngine.UI.Image templateImage = GameObject.FindGameObjectWithTag("ImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();
            Transform canvasGameObject = this.gameObject.transform.Find("DisplayCanvas");
            float scaleFactor = 0.1f;
            scaleFactor /= 5f;
            foreach (var s in imageTargetComments)
            {
                UnityEngine.UI.Image tmpImage = UnityEngine.UI.Image.Instantiate<UnityEngine.UI.Image>(templateImage);
                tmpImage.transform.SetParent(canvasGameObject.transform);

                tmpImage.transform.rotation = canvasGameObject.GetChild(0).rotation;
                tmpImage.transform.localPosition = new Vector3(
                    UnityEngine.Random.Range(-Screen.width / 2 * scaleFactor, Screen.width / 2 * scaleFactor),
                    0,
                    UnityEngine.Random.Range(-Screen.height / 2 * scaleFactor, Screen.height / 2 * scaleFactor)
                );
                tmpImage.transform.localScale = new Vector3(scaleFactor / 2, scaleFactor / 2, scaleFactor / 2);

                tmpImage.GetComponentInChildren<Text>().text = s;
                //tmpImage.GetComponentInChildren<Text>().transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                tmpImage.GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;

                tmpImage.gameObject.AddComponent<TextItem>();
                tmpImage.GetComponent<TextItem>().speed = UnityEngine.Random.Range(30f * scaleFactor, 60f * scaleFactor);
                tmpImage.GetComponent<TextItem>().enabled = true;

                tmpImage.gameObject.SetActive(AppManager.Instance.showBarrage);
            }
        }
        
    }


    protected virtual void OnTrackingLost()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;

        /*
        * Barrage Text Unattach
        * 
        */
        //var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
        //UnityEngine.UI.Image templateImage = GameObject.FindGameObjectWithTag("ImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();
        //Transform canvasGameObject = this.gameObject.transform.Find("DisplayCanvas");
        //float scaleFactor = 0.1f;
        //scaleFactor /= 5f;
        //for (int i = 0; i<canvasGameObject.childCount; i++)
        //{
        //    canvasGameObject.GetChild(i);
        //}
    }

    #endregion // PROTECTED_METHODS
}
