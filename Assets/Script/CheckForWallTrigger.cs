using UnityEngine;

public class CheckForWallTrigger : MonoBehaviour
{
    [SerializeField] private GameObject roadPrefab;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Road"))
        {
            Instantiate(roadPrefab,other.transform.position + new Vector3(0, 0, 75),Quaternion.identity);
        }
        if(other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("obstacle");
            GetComponentInChildren<Animator>().SetTrigger("End");
            GameObject.FindAnyObjectByType<GameManager>().End();
        }
    }

}