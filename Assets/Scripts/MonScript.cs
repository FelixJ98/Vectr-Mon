using UnityEngine;
using Unity.Netcode;

public class MonScript : NetworkBehaviour
{
    #region Variables
    public int health = 100;
    public enum typing
    {
        Attacker = 0,
        Defender = 1,
        Grappler = 2,
        None
    }
    public typing MonType;

    public enum moves
    {
        Attack = 0,
        Defend = 1,
        Grab = 2,
        None
    }
    public moves selectedMove;
    #endregion


    #region Move Selection
    public void Attack()
    {
        Debug.Log("Attack command recieved");
        Debug.Log($"Attack called, IsOwner = {IsOwner}");
        SubmitMoveServerRpc(moves.Attack);
    }

    public void Defend()
    {
        SubmitMoveServerRpc(moves.Defend);
    }

    public void Grab()
    {
        SubmitMoveServerRpc(moves.Grab);
    }

    [ServerRpc]
    private void SubmitMoveServerRpc(moves move)
    {
        Debug.Log("Move submitted");
        this.selectedMove = move;

        // Register player if not already assigned
        ServerScript.Instance.RegisterPlayer(this);

        ServerScript.Instance.RegisterMove(this);
    }
    #endregion
}
