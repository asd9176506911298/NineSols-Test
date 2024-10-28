using NineSolsAPI;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExampleMod
{
    public class Shooter : MonoBehaviour
    {
        private bool isCoroutineRunning = false; // Track if coroutine is running

        //void Update()
        //{
        //    GameObject x = Instantiate(GameObject.Find("Circle"), base.transform.position, Quaternion.identity);
        //    x.SetActive(true);
        //    x.GetComponent<Rigidbody2D>().AddForce(new Vector2(50, 50));

        //}

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    ToastManager.Toast($"OnTriggerEnter2D {collision.gameObject.GetComponent<EffectDealer>().OwnerComponent}");
        //    //Player.i.health.currentValue -= 5;
        //    //Destroy(gameObject);

        //    GameObject x = Instantiate(GameObject.Find("Circle"), new Vector3(Player.i.Center.x, Player.i.Center.y+150,0), Quaternion.identity);
        //    x.SetActive(true);
        //    x.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -250f),ForceMode2D.Impulse);
        //}


        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!isCoroutineRunning) // Ensure the coroutine is called only once
            {
                isCoroutineRunning = true;
                StartCoroutine(DelayedAction());
            }
        }

        // Coroutine to delay execution by 3 seconds
        private IEnumerator DelayedAction()
        {
            // Generate a random delay between 1 and 3 seconds (you can adjust the range)
            float randomDelay = UnityEngine.Random.Range(0.1f, 0.5f);

            // Wait for the random amount of time
            yield return new WaitForSeconds(randomDelay);

            // Instantiate the object after the delay
            GameObject x = Instantiate(GameObject.Find("Ball"), new Vector3(Player.i.Center.x, Player.i.Center.y + 150, 0), Quaternion.identity);
            x.SetActive(true);
            x.GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(-150f, 150f), -250f), ForceMode2D.Impulse);

            // Destroy the instantiated object after another 3 seconds
            Destroy(x, 1.5f);

            isCoroutineRunning = false; // Reset the flag after the coroutine finishes
        }
    }
}
