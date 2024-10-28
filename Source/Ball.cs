using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    public class Ball : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            ToastManager.Toast("OnTriggerEnter2D");
            Player.i.health.currentValue -= 5;
            Destroy(gameObject);
        }

        //void OnTriggerStay2D(Collider2D other)
        //{
        //    ToastManager.Toast("OnTriggerStay2D");
        //}

        //private void OnCollisionEnter2D(Collision2D coll)
        //{
        //    ToastManager.Toast("OnCollisionEnter2D");
        //    //Player.i.health.currentValue -= 5;
        //}

        //void OnCollisionStay2D(Collision2D collision)
        //{
        //    ToastManager.Toast("OnCollisionStay2D");
        //}

        
    }
}
