using UnityEngine;
using System.Collections;

public class ColliderDestroy : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
            Destroy(collision.gameObject);
    }
}
