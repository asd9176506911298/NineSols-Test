using NineSolsAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class code : MonoBehaviour
{
    void Start()
    {
        ToastManager.Toast("Start");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //ToastManager.Toast($"OnTriggerEnter2D {collision.gameObject.GetComponent<EffectDealer>().OwnerComponent}");
        ToastManager.Toast($"OnTriggerEnter2D {collision.gameObject}");
        //Player.i.health.currentValue -= 5;
        //Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        ToastManager.Toast("OnCollisionEnter2D");
        //Player.i.health.currentValue -= 5;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        ToastManager.Toast("OnCollisionStay2D");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ToastManager.Toast("OnTriggerStay2D");
    }
}
