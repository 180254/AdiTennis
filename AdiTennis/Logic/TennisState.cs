using System;
using ProtoBuf;

namespace AdiTennis.Logic
{
    [ProtoContract]
    internal class TennisState
    {
        private readonly Random _rnd = new Random();

        [ProtoMember(1)] public volatile int BallPosX;
        [ProtoMember(2)] public volatile int BallPosY;
        [ProtoMember(3)] public string[] PlayerNickname;
        [ProtoMember(4)] public volatile int[] PlayerPoints;
        [ProtoMember(5)] public volatile int[] RacketPosY;
        [ProtoMember(6)] public volatile bool GameOver;
        [ProtoMember(7)] public volatile PlayerTypeEnum WhoWon;

        private int _ballVx;
        private int _ballVy;
        private int _startRoundBallVx;
        private readonly int[] _ballVyDirections = {-1, 1};

        public TennisState Init()
        {
            PlayerNickname = new string[2];
            PlayerPoints = new int[2];
            RacketPosY = new int[2];

            RacketPosY[(int) PlayerTypeEnum.Server] = GameState.Config.StageHeight/2 - GameState.Config.RacketSize/2;
            RacketPosY[(int) PlayerTypeEnum.Client] = RacketPosY[(int) PlayerTypeEnum.Server];

            _ballVx = 1;
            _startRoundBallVx = 1;

            NextBall(null);
            return this;
        }

        public string PointsAsString()
        {
            return PlayerNickname[(int) PlayerTypeEnum.Server].PadLeft(GameState.Config.NickMaxLen) + " "
                   + PlayerPoints[(int) PlayerTypeEnum.Server] + " : " + PlayerPoints[(int) PlayerTypeEnum.Client]
                   + " " + PlayerNickname[(int) PlayerTypeEnum.Client].PadRight(GameState.Config.NickMaxLen);
        }

        public void RacketPosUp(PlayerTypeEnum playerType)
        {
            if (RacketPosY[(int) playerType] > 1)
            {
                RacketPosY[(int) playerType] -= 1;
            }
        }

        public void RacketPosDown(PlayerTypeEnum playerType)
        {
            if (RacketPosY[(int) playerType] + GameState.Config.RacketSize < GameState.Config.StageHeight - 2)
            {
                RacketPosY[(int) playerType] += 1;
            }
        }

        public void NextBall(BallPhysicsWorker physicsWorker)
        {
            if (physicsWorker != null)
                physicsWorker.Sleep(GameState.Config.PointLostSleepMs);


            BallPosX = GameState.Config.StageWidth/2;
            BallPosY = _rnd.Next(2, GameState.Config.StageHeight - 2);


            _ballVx = -_startRoundBallVx;
            _startRoundBallVx = _ballVx;

            _ballVy = _ballVyDirections[_rnd.Next(0, _ballVyDirections.Length)];

            if (physicsWorker != null)
                physicsWorker.Sleep(GameState.Config.BetweenRoundSleepMs);
        }

        public void NextBallPos(BallPhysicsWorker physicsWorker)
        {
            lock (GameState.Config.TennisStagePrinterLock)
            {
                BallPosX += _ballVx;
                BallPosY += _ballVy;


                if ((BallPosX -1  == GameState.Config.RacketPosServer
                     && BallPosY >= RacketPosY[(int) PlayerTypeEnum.Server]
                     && BallPosY <= RacketPosY[(int) PlayerTypeEnum.Server] + GameState.Config.RacketSize)
                    ||
                    (BallPosX + 1 == GameState.Config.RacketPosClient
                     && BallPosY >= RacketPosY[(int) PlayerTypeEnum.Client]
                     && BallPosY <= RacketPosY[(int) PlayerTypeEnum.Client] + GameState.Config.RacketSize))
                {
                    _ballVx = -_ballVx;
                }


                if (BallPosY == GameState.Config.StageHeight - 2 || BallPosY == 1)
                {
                    _ballVy = -_ballVy;
                }

                if (BallPosX == GameState.Config.StageWidth - 1)
                {
                    AddPoint(PlayerTypeEnum.Server);
                    NextBall(physicsWorker);
                }

                if (BallPosX == -1)
                {
                    PlayerPoints[(int) PlayerTypeEnum.Client]++;
                    NextBall(physicsWorker);
                }
            }
        }

        public void AddPoint(PlayerTypeEnum playerType)
        {
            PlayerPoints[(int) playerType]++;
            if (PlayerPoints[(int) playerType] >= GameState.Config.OneRoundPoints)
//                && Math.Abs(PlayerPoints[(int) PlayerTypeEnum.Server] - PlayerPoints[(int) PlayerTypeEnum.Client]) >= 2)
            {
                GameOver = true;
                WhoWon = playerType;
                GameState.GetInstance().ClientServerVars.TheGameEnd.Set();
            }
        }

        public TennisState Clone()
        {
            var copy = new TennisState
            {
                BallPosX = BallPosX,
                BallPosY = BallPosY,
                PlayerNickname = PlayerNickname,
                PlayerPoints = new[] {PlayerPoints[0], PlayerPoints[1]},
                RacketPosY = new[] {RacketPosY[0], RacketPosY[1]}
            };
            return copy;
        }


        public static bool HasSamePoints(TennisState state1, TennisState state2)
        {
            return state2 != null && state1.PlayerPoints[0] == state2.PlayerPoints[0] &&
                   state1.PlayerPoints[1] == state2.PlayerPoints[1];
        }

        public static bool HasSameRacketPos(TennisState state1, TennisState state2, PlayerTypeEnum playerType)
        {
            return state2 != null && state1.RacketPosY[(int) playerType] == state2.RacketPosY[(int) playerType];
        }


        public static bool HasSameBallPos(TennisState state1, TennisState state2)
        {
            return state2 != null && state1.BallPosY == state2.BallPosY &&
                   state1.BallPosX == state2.BallPosX;
        }
    }
}