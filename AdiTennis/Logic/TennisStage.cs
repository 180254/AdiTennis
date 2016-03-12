#region usings
using System;
using AdiTennis.StageAbstract.Stages;

#endregion

namespace AdiTennis.Logic
{
    internal class TennisStage : INonblockingStage
    {
        private readonly TennisState _tennisState;
        private static TennisState _prevTennisState;

        public TennisStage(TennisState tennisState)
        {
            _tennisState = tennisState;
        }

        public void Go()
        {
            if (!TennisState.HasSameRacketPos(_tennisState, _prevTennisState, PlayerTypeEnum.Server))
                DrawRacket(PlayerTypeEnum.Server);

            if (!TennisState.HasSameRacketPos(_tennisState, _prevTennisState, PlayerTypeEnum.Client))
                DrawRacket(PlayerTypeEnum.Client);

            if (!TennisState.HasSameBallPos(_tennisState, _prevTennisState))
                DrawBall();

            if (!TennisState.HasSamePoints(_tennisState, _prevTennisState))
                PrintResult();

            SetPromptPos();
            _prevTennisState = _tennisState.Clone();
        }

        public static void Init()
        {
            _prevTennisState = null;
            Console.Clear();
            DrawBorder();
            SetPromptPos();
        }

        private static void DrawBorder()
        {
            var horizontalBorder = String.Empty.PadLeft(GameState.Config.StageWidth - 1, '-');
            var verticalBorder = "|" + String.Empty.PadLeft(GameState.Config.StageWidth - 3) + "|";

            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            Console.Write(horizontalBorder);

            Console.CursorLeft = 0;
            Console.CursorTop = GameState.Config.StageHeight - 1;
            Console.Write(horizontalBorder);

            for (var i = 1; i < GameState.Config.StageHeight - 1; i++)
            {
                Console.CursorTop = i;
                Console.CursorLeft = 0;
                Console.Write(verticalBorder);
            }
        }

        private static void SetPromptPos()
        {
            Console.CursorTop = 1;
            Console.CursorLeft = 1;
        }

        private void DrawRacket(PlayerTypeEnum playerTypeR)
        {
            var consoleLeft = playerTypeR == PlayerTypeEnum.Server
                ? GameState.Config.RacketPosServer
                : GameState.Config.RacketPosClient;
            var posY = _tennisState.RacketPosY[(int) playerTypeR];

            for (var i = 1; i <= GameState.Config.StageHeight - 2; i++)
            {
                Console.CursorTop = i;
                Console.CursorLeft = consoleLeft;

//                if (Console.CursorLeft != consoleLeft)
//                    Console.CursorLeft = consoleLeft;
//                    throw new System.Exception();

                if (i >= posY && i <= posY + GameState.Config.RacketSize)
                    Console.Write('X');
                else
                    Console.Write(' ');
            }
        }

        private void DrawBall()
        {
            if (_prevTennisState != null)
            {
                Console.CursorTop = _prevTennisState.BallPosY;
                Console.CursorLeft = _prevTennisState.BallPosX;

                if (!((_prevTennisState.BallPosX == 0
                       || _prevTennisState.BallPosX == GameState.Config.StageWidth - 2)))
                    Console.Write(' ');

            }

            Console.CursorTop = _tennisState.BallPosY;
            Console.CursorLeft = _tennisState.BallPosX;
            Console.Write('o');
        }

        private void PrintResult()
        {
            var resultString = _tennisState.PointsAsString();
            var colonPos = resultString.IndexOf(':');

            Console.CursorTop = GameState.Config.ResultTopPos;
            Console.CursorLeft = GameState.Config.StageWidth/2 - colonPos;
            Console.Write(resultString);
        }
    }
}