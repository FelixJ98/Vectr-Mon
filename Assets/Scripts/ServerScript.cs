using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;

public class ServerScript : NetworkBehaviour
{
    #region Variables
    public bool gameStarted = false;
    public int damageP1 = 25;
    public int damageP2 = 25;
    public MonScript player1;
    public MonScript player2;

    private bool p1MoveSubmitted = false;
    private bool p2MoveSubmitted = false;
    
    public static ServerScript Instance;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    #region Functions

    public void RegisterPlayer(MonScript mon)
    {
        if (player1 == null)
            player1 = mon;
        else if (player2 == null && mon != player1)
            player2 = mon;
    }

    public void RegisterMove(MonScript mon)
    {
        if (mon == player1)
            p1MoveSubmitted = true;

        if (mon == player2)
            p2MoveSubmitted = true;

    }
    #endregion

    void Update()
    {
        if (!IsServer) return;

        #region Game Loop
        if (p1MoveSubmitted && p2MoveSubmitted && !gameStarted)
        {
            damageP1 = 25;
            damageP2 = 25;

            #region Damage Calculation
            if (player1.selectedMove == player2.selectedMove)
            {
                damageP1 = 0;
                damageP2 = 0;
            }
            else if (player1.selectedMove == MonScript.moves.Attack && player2.selectedMove == MonScript.moves.Defend)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Defender)
                {
                    damageP2 = 40;
                }
            }
            else if (player1.selectedMove == MonScript.moves.Attack && player2.selectedMove == MonScript.moves.Grab)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Attacker)
                {
                    damageP1 = 40;
                }
            }
            else if (player1.selectedMove == MonScript.moves.Defend && player2.selectedMove == MonScript.moves.Attack)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Defender)
                {
                    damageP1 = 40;
                }
            }
            else if (player1.selectedMove == MonScript.moves.Defend && player2.selectedMove == MonScript.moves.Grab)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Grappler)
                {
                    damageP2 = 40;
                }
            }
            else if (player1.selectedMove == MonScript.moves.Grab && player2.selectedMove == MonScript.moves.Attack)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Attacker)
                {
                    damageP2 = 40;
                }
            }
            else if (player1.selectedMove == MonScript.moves.Grab && player2.selectedMove == MonScript.moves.Defend)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Grappler)
                {
                    damageP1 = 40;
                }
            }
            #endregion

            player1.health.Value -= damageP2;
            player2.health.Value -= damageP1;

            if(player1.health.Value <= 0 || player2.health.Value <= 0)
            {
                player1.selectedMove = MonScript.moves.None;
                player2.selectedMove = MonScript.moves.None;
                return;
            }

            p1MoveSubmitted = false;
            p2MoveSubmitted = false;
            player1.selectedMove = MonScript.moves.None;
            player2.selectedMove = MonScript.moves.None;
            gameStarted = false;
        }
        #endregion
    }
}
