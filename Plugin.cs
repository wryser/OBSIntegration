using BepInEx;
using System;
using UnityEngine;
using System.Net;
using System.Threading;
using System.Text;
using Photon.Pun;
using System.Linq;
using OBSWebServer.Patches;
using BepInEx.Configuration;

namespace OBSWebServer
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>
    /// 
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private HttpListener listener;
        private Thread listenerThread;
        string roomCode = "Null";
        string gameMode = "Null";
        string image = "Null";
        ConfigEntry<bool> showCodePubs;
        ConfigEntry<bool> showCodePrivates;

        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        void OnGameInitialized()
        {
            /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */
            showCodePubs = Config.Bind("Settings", "Show Room Code In Publics", true, "This is whether it should show your room code when you are in a public lobby.");
            showCodePrivates = Config.Bind("Settings", "Show Room Code In Privates", false, "This is whether it should show your room code when you are in a private lobby.");
            image = Config.Bind("Settings", "For putting a image (if you dont wanna a image put this blank.", "https://static.wikia.nocookie.net/character-stats-and-profiles/images/f/ff/Gorilla_%28Monke.png/revision/latest/scale-to-width-down/800?cb=20240302030939");
            HarmonyPatches.ApplyHarmonyPatches();
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            listenerThread = new Thread(HandleRequests);
            listenerThread.Start();
            Debug.Log("Listening for connections on http://localhost:8080/");
        }

        void HandleRequests()
        {
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerResponse response = context.Response;
                string responseString;
                if (PhotonNetwork.InRoom)
                {
                    string map = FormatMap(MapPatch.ActiveZones.First().ToString().ToUpper());
                    responseString = $"<html><h1 style=color:white;font-size:300%;font-family:Arial>Gorilla Tag<br>Room Code: {roomCode.ToUpper()}<br>Map: {map}<br>Gamemode: {gameMode}</h1><meta http-equiv=refresh content=4> <img src="{image}"></img></html>";
                }
                else
                {
                    responseString = $"<html><h1 style=color:white;font-size:300%;font-family:Arial>Gorilla Tag<br>Not In Room</h1><meta http-equiv=refresh content=4> <img src="{image}"></img></html>";
                }
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        void OnApplicationQuit()
        {
            listener.Stop();
            listenerThread.Abort();
        }

        void Update()
        {
            /* Code here runs every frame when the mod is enabled */
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.CurrentRoom.IsVisible)
                {
                    if (showCodePubs.Value)
                    {
                        roomCode = PhotonNetwork.CurrentRoom.Name;
                    }
                    else
                    {
                        roomCode = "CODE HIDDEN";
                    }
                }
                else
                {
                    if (showCodePrivates.Value)
                    {
                        roomCode = PhotonNetwork.CurrentRoom.Name;
                    }
                    else
                    {
                        roomCode = "PRIVATE ROOM";
                    }
                }
                gameMode = GorillaGameManager.instance.GameModeName();
            }
            else
            {
                roomCode = "Not In Room";
            }
        }
        string FormatMap(string map)
        {
            string formattedmap;
            switch (map)
            {
                case "CANYON":
                    formattedmap = "CANYONS";
                    break;
                case "CAVE":
                    formattedmap = "CAVES";
                    break;
                case "ROTATING":
                    formattedmap = "OG CAVES";
                    break;
                default:
                    Debug.Log(map);
                    formattedmap = map;
                    break;
            }
            return formattedmap;
        }
    }
}
