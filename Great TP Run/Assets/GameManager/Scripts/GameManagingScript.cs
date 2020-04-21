﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class GameManagingScript : NetworkComponent
{
    public Material[] materialList;
    public NetworkPlayerOption[] playerList;
    public Canvas endCanvas;
    public bool GameStarted = false;
    public bool GameEnded = false;
    public Text playerScoreText;
    public Text apText;
    public Text winnerText;

    public override void HandleMessage(string flag, string value)
    {
        playerList = FindObjectsOfType<NetworkPlayerOption>();

        //Removing player options UI.
        if (flag == "START")
        {
            for (int i = 0; i < playerList.Length; i++)
            {
                playerList[i].apCanvas.enabled = false;
            }
        }
        if (flag == "END")
        {
            GameEnded = true;
            EndScreenSetup();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        
        if (IsServer)
        {
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
            while (!GameStarted || MyCore.Connections.Count == 0)
            {
                if (MyCore.Connections.Count != 0)
                {
                    GameStarted = true;
                }

                playerList = FindObjectsOfType<NetworkPlayerOption>();
                foreach (NetworkPlayerOption player in playerList)
                {
                    if (!player.ready)
                    {
                        GameStarted = false;
                    }
                }

                yield return new WaitForSeconds(MyCore.MasterTimer);
            }

            SendUpdate("START", "True");

            for (int i = 0; i < playerList.Length; i++)
            {
                GameObject temp = MyCore.NetCreateObject(playerList[i].shape, playerList[i].Owner, spawnObjects[playerList[i].Owner%4].transform.position);
                GameCharacter gcTemp = temp.GetComponent<GameCharacter>();

                //Setting values of GameCharacter
                gcTemp.Pname = playerList[i].Pname;
                gcTemp.color = playerList[i].color;
            }

            while (!GameEnded)
            {
                GameEnded = true;

                GameObject[] coins = GameObject.FindGameObjectsWithTag("coin");

                if (coins.Length > 0)
                {
                    GameEnded = false;
                }

                yield return new WaitForSeconds(MyCore.MasterTimer);
            }

            //When this part is reached, the game has ended.
            Debug.Log("Game has ended.");
            SendUpdate("END", "True");
        }  

    }

    public void EndScreenSetup()
    {
        int maxScore = 0;
        string winnerName = "Default Text";
        endCanvas.enabled = true;
        GameCharacter[] players = FindObjectsOfType<GameCharacter>();
        foreach (GameCharacter player in players)
        {
            if(player.score > maxScore)
            {
                maxScore = player.score;
                winnerName = player.Pname;
            }
            apText.text += player.Pname + "\n";
            playerScoreText.text += player.score + "\n";
        }
        winnerText.text = winnerName + " Wins!";
    }

    void Start()
    {
        endCanvas.enabled = false;
    }
}
