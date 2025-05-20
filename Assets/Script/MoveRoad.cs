using UnityEngine;

public class MoveRoad : MonoBehaviour
{
    private GameManager GM;
    [SerializeField] int WallSpeed=4;

    private void Awake()
    {
        GM = GameObject.FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        if (GM.canMoveRoad) transform.position += new Vector3(0, 0, -WallSpeed) * Time.deltaTime;
    }
}
