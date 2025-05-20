using UnityEngine;

public class DestroyRoad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            Destroy(other.gameObject);
        }
    }


}
