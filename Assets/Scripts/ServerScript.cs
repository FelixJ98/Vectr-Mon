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

    public AudioClip startAudioClip;
    public AudioClip battleAudioClip;

    public AudioClip attackClip;
    public AudioClip grabClip;
    public AudioClip blockClip;
    public AudioClip noneClip;
    public GameObject attackFX;
    public GameObject blockFX;
    public GameObject grabFX;

    public AudioSource audioSource;
    public AudioSource audioSourceMusic;
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
        
        if(player1 != null && player2 != null)
        {
            PlayStartAudioClientRpc();
        }
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void PlayStartAudioClientRpc()
    {
        audioSourceMusic.volume = 1;
        audioSourceMusic.PlayOneShot(startAudioClip, 0.2f);

        Invoke("PlayBattle", 2); // would use an IEnumerator but... eh
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void PlayAudioClientRpc(MonScript.moves type)
    {
        if (type == MonScript.moves.Attack)
        {
            audioSource.PlayOneShot(attackClip);
        }
        else if (type == MonScript.moves.Defend)
        {
            audioSource.PlayOneShot(blockClip);
        }
        else if (type == MonScript.moves.Grab)
        {
            audioSource.PlayOneShot(grabClip);

        }
        else
        {
            
            audioSource.PlayOneShot(noneClip);
        }
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void PlayFXClientRpc(MonScript.moves typeP1, Vector3 posP1, Quaternion rotP1, MonScript.moves typeP2, Vector3 posP2, Quaternion rotP2)
    {
        SpawnFX(typeP1, posP1, rotP1);
        SpawnFX(typeP2, posP2, rotP2);
    }

    public void SpawnFX(MonScript.moves type, Vector3 pos, Quaternion rot)
    {
        GameObject newObj = null;
        if (type == MonScript.moves.Attack)
        {
            newObj = Instantiate(attackFX, pos, rot);
        }
        else if (type == MonScript.moves.Defend)
        {
            newObj = Instantiate(blockFX, pos, rot);
        }
        else if (type == MonScript.moves.Grab)
        {
            newObj = Instantiate(grabFX, pos, rot);
        }
        else
        {
            Debug.LogError("Error!!");
        }

        // Issue with positioning. Rotating does not seem to affect it
        newObj.transform.Rotate(new Vector3(90, 0, 0));

        // Add a forward backwards offset bc of the way the Mon is rotated
        newObj.transform.position -= Vector3.forward * 0.3f;
    }

    private void PlayBattle()
    {
        audioSourceMusic.clip = battleAudioClip;
        audioSourceMusic.volume = 0.1f;
        audioSource.loop = true;
        audioSourceMusic.Play();
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
                PlayAudioClientRpc(MonScript.moves.None);
            }
            else if (player1.selectedMove == MonScript.moves.Attack && player2.selectedMove == MonScript.moves.Defend)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Defender)
                {
                    damageP2 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Defend);
            }
            else if (player1.selectedMove == MonScript.moves.Attack && player2.selectedMove == MonScript.moves.Grab)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Attacker)
                {
                    damageP1 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Attack);
            }
            else if (player1.selectedMove == MonScript.moves.Defend && player2.selectedMove == MonScript.moves.Attack)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Defender)
                {
                    damageP1 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Defend);
            }
            else if (player1.selectedMove == MonScript.moves.Defend && player2.selectedMove == MonScript.moves.Grab)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Grappler)
                {
                    damageP2 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Grab);
            }
            else if (player1.selectedMove == MonScript.moves.Grab && player2.selectedMove == MonScript.moves.Attack)
            {
                damageP1 = 0;
                damageP2 = 25;
                if (player2.MonType == MonScript.typing.Attacker)
                {
                    damageP2 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Attack);
            }
            else if (player1.selectedMove == MonScript.moves.Grab && player2.selectedMove == MonScript.moves.Defend)
            {
                damageP1 = 25;
                damageP2 = 0;
                if (player1.MonType == MonScript.typing.Grappler)
                {
                    damageP1 = 40;
                }
                PlayAudioClientRpc(MonScript.moves.Grab);
            }
            PlayFXClientRpc(player1.selectedMove, player1.transform.position, player1.transform.rotation, 
                player2.selectedMove, player2.transform.position, player2.transform.rotation);
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
