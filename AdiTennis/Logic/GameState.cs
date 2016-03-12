using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using AdiTennis.GameStages.Client;
using AdiTennis.Sockets;
using AdiTennis.Sockets.ClientServer;

namespace AdiTennis.Logic
{
    internal class GameState
    {
        #region Singleton
        private GameState()
        {
        }

        private static class GameStateHolder
        {
            public static readonly GameState Instance = new GameState();
        }

        public static GameState GetInstance()
        {
            return GameStateHolder.Instance;
        }
        #endregion

        #region Config
        public static class Config
        {
            public static object TennisStagePrinterLock = new object();

            #region General
            public const int Fps = 30;
            public const int Port = 180*254; // Student No. :-)
            public const string GameName = "ADITENNIS";
            public const string DiscoveryRequestText = GameName + "_DISCOVERY";
            public const string DiscoveryServerInfoStructId = GameName + "_SERVERINFOSTRUCT";
            public const int DiscoveryTimeoutAfterFirstReceiveMs = 1000;
            public const int DiscoveryRequestRepeatTimeMs = 3000;
            public const int FpsAsMilisecondsSleep = 1000/Fps;
            public const int ReadKeyCheckFrequencyDivisor = 2;
            public const int ReadKeyFpsAsMilisecondsSleep = FpsAsMilisecondsSleep/ReadKeyCheckFrequencyDivisor;
            public const int ResponseTimeoutMs = 3000;
            public const int BufforsLenght = 256;
            public const int NickMaxLen = 10;
            public const int GameOverMsgMs = 3000;
            public const int BetweenRoundSleepMs = 500;
            public const int PointLostSleepMs = 300;
            public const int StartGameDelayMs = 2000;
            public const int OneRoundPoints = 20;
            #endregion

            #region Tennis stage
            public const int StageWidth = 80;
            public const int StageHeight = 25;
            public const int RacketSize = 5;
            public const int RacketPosLeft = 5;
            public const int RacketPosServer = RacketPosLeft;
            public const int RacketPosClient = StageWidth - RacketPosLeft - 1;
            public const int RacketPosCenter = StageHeight/2 - RacketSize/2 - 1;
            public const int PromptCursorLeftPosServer = 0;
            public const int PromptCursorLeftPosClient = StageWidth - 1;
            public const int ResultTopPos = 2;
            #endregion

            #region Physics
            public const int BallSpeedMs = 50;
            #endregion

            #region Msg
            public const int MsgCursorTop = 5;
            public const int MsgCursorLeft = 10;
            public const ConsoleColor MsgColorStandard = ConsoleColor.DarkGreen;
            public const ConsoleColor MsgColorError = ConsoleColor.DarkYellow;
            public const ConsoleColor MsgColorHeader = ConsoleColor.DarkCyan;
            #endregion
        }
        #endregion

        #region Server vars
        public class ServerVarsStruct
        {
            public ManualResetEvent ClientConnectedEvent = new ManualResetEvent(false);
            public ManualResetEvent GameStateSent = new ManualResetEvent(false);
            public TcpClientStruct ConnectedClient;

            internal ServerVarsStruct()
            {
            }
        }

        public ServerVarsStruct ServerVars = new ServerVarsStruct();
        #endregion

        #region ClientServer vars
        public class ClientServerVarsStruct
        {
            public TennisState TennisState = new TennisState().Init();
            public ManualResetEvent TheGameEnd = new ManualResetEvent(false);
            public PlayerTypeEnum SelectedStartAs;
            public ConnectionStatusEnum ConnectionStatus;

            private string _ownNickname;

            public string OwnNickname
            {
                get { return _ownNickname; }
                set
                {
                    if (value != null)
                    {
                        value = value.ToUpper();
                        value = value.Substring(0, Math.Min(Config.NickMaxLen, value.Length));
                        value = new Regex("[^a-zA-Z0-9_]").Replace(value, "X");
                    }
                    _ownNickname = value;
                }
            }

            internal ClientServerVarsStruct()
            {
            }
        }

        public ClientServerVarsStruct ClientServerVars = new ClientServerVarsStruct();
        #endregion

        #region Client vars
        public class ClientVarsStruct
        {
            public ConnectionMethodEnum ConnectionMethod;
            public List<ServerInfoStruct> DiscoveredServers = new List<ServerInfoStruct>();
            public ManualResetEvent ServersDiscoveredEvent = new ManualResetEvent(false);
            public ManualResetEvent ConnectionEstabilishedEvent = new ManualResetEvent(false);

            public IPEndPoint SelectedServer;
            public ServerToClientResponseEnum ServerResponse;
            public TcpClientStruct ConnectedServer;

            internal ClientVarsStruct()
            {
            }

            public List<ServerInfoStruct> DiscoveredServersDistinct()
            {
                DiscoveredServers = DiscoveredServers.Distinct().ToList();
                return DiscoveredServers;
            }
        }

        public ClientVarsStruct ClientVars = new ClientVarsStruct();
        #endregion

        #region Functions
        public void ResetAll()
        {
//            var ownNicknameCopy = ClientServerVars.OwnNickname;
//            GameStateHolder.Instance = new GameState();
//            GetInstance().ClientServerVars.OwnNickname = ownNicknameCopy;

            ServerVars.ClientConnectedEvent = new ManualResetEvent(false);
            ServerVars.GameStateSent = new ManualResetEvent(false);
            ServerVars.ConnectedClient = null;
            ClientServerVars.TennisState = new TennisState().Init();
            ClientServerVars.TheGameEnd = new ManualResetEvent(false);
            ClientServerVars.SelectedStartAs = PlayerTypeEnum.Client;
            ClientServerVars.ConnectionStatus = ConnectionStatusEnum.None;
            ClientVars.ConnectionMethod = ConnectionMethodEnum.DiscoveryFirstFound;
            ClientVars.DiscoveredServers = new List<ServerInfoStruct>();
            ClientVars.ServersDiscoveredEvent = new ManualResetEvent(false);
            ClientVars.ConnectionEstabilishedEvent = new ManualResetEvent(false);
            ClientVars.SelectedServer = null;
            ClientVars.ServerResponse = ServerToClientResponseEnum.None;
            ClientVars.ConnectedServer = null;
        }
        #endregion
    }
}