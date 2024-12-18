using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFakes : MonoBehaviour
{
    private Vector3 lastPosition; 
    private float elapsedTime; 
    private const float interval = 0.1f; 

    public Animator animator;

    void Start()
    {
        lastPosition = transform.position;
        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= interval)
        {
            Vector3 currentPosition = transform.position;

            if (currentPosition == lastPosition)
            {
                //El objeto no se ha movido en los últimos 1/10 de segundo
                animator.SetBool("Walk", false);
            }
            else
            {
                //El objeto se ha movido en los últimos 1/10 de segundo
                animator.SetBool("Walk", true);
            }

            lastPosition = currentPosition;
            elapsedTime = 0f;
        }
    }
}
