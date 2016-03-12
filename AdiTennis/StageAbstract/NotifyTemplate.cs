using System;
using AdiTennis.Logic;
using AdiTennis.StageAbstract.Menus;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis.StageAbstract
{
    internal class NotifyTemplate : IStage
    {
        private readonly string _notifyHeader;
        private readonly string _notifyText;

        public NotifyTemplate(string notifyHeader, string notifyText)
        {
            _notifyHeader = notifyHeader;
            _notifyText = notifyText;
        }

        public void Go()
        {
            lock (GameState.Config.TennisStagePrinterLock)
            {
                Console.Clear();
                MenuTemplate.PrintLogo();
                Console.CursorTop = GameState.Config.MsgCursorTop;
                Console.CursorLeft = GameState.Config.MsgCursorLeft;
                Console.ForegroundColor = GameState.Config.MsgColorHeader;
                Console.WriteLine(_notifyHeader);

                Console.CursorLeft = GameState.Config.MsgCursorLeft;
                Console.ForegroundColor = GameState.Config.MsgColorStandard;
                Console.Write(_notifyText);
                Console.ResetColor();
            }
        }
    }
}