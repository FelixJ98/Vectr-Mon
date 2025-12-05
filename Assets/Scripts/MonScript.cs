using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MonScript : NetworkBehaviour
{
    #region Variables
    public NetworkVariable<int> health = new NetworkVariable<int>(
    100,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    public TextMeshProUGUI text;
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
        Debug.Log("TESTAttack command recieved");
        Debug.Log($"TESTAttack called, IsOwner = {IsOwner}");
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
        Debug.Log("TESTMove submitted");
        this.selectedMove = move;

        // Register player if not already assigned
        ServerScript.Instance.RegisterPlayer(this);

        ServerScript.Instance.RegisterMove(this);
    }
    #endregion

    private void Update()
    {
        text.text = "Health: " + health.Value;
    }
}
