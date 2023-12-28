using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public float Speed = 45f;
    public bool Opened;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SwingTo(Quaternion targetRot)
    {
        while (Quaternion.Angle(targetRot, transform.localRotation) > 1f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot, Time.deltaTime * Speed);
            Debug.Log("target: " +  targetRot);
            Debug.Log("current: " + transform.localRotation);
            yield return new WaitForEndOfFrame();
        }
        
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(SwingTo(transform.localRotation * Quaternion.Euler(0, 90, 0)));
        Opened = true;
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(SwingTo(Quaternion.identity));
        Opened = false;
    }


    public void Interact()
    {
        if(!Opened)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
}
