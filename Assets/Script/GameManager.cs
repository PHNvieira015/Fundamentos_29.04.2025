using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool canMoveRoad = true;

    public void End()
    {
        canMoveRoad = false;
    }
}
