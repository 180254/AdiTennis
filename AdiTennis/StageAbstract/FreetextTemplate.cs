using System;
using AdiTennis.Exception;
using AdiTennis.Logic;
using AdiTennis.StageAbstract.Menus;

namespace AdiTennis.StageAbstract
{
    internal class FreetextTemplate
    {
        private readonly string _additionalMsg;
        private readonly string _promptMsg;
        public string Freetext;

        public FreetextTemplate(string promptMsg, string additionalMsg)
        {
            _promptMsg = promptMsg;
            _additionalMsg = additionalMsg;
        }

        public FreetextTemplate(string promptMsg) : this(promptMsg, null)
        {
        }

        public void Go()
        {
            do
            {
                Console.Clear();
                MenuTemplate.PrintLogo();

                if (_additionalMsg != null)
                {
                    Console.CursorTop = GameState.Config.MsgCursorTop;
                    Console.CursorLeft = GameState.Config.MsgCursorLeft;
                    Console.ForegroundColor = GameState.Config.MsgColorError;
                    Console.Write(_additionalMsg);
                }

                Console.CursorTop = GameState.Config.MsgCursorTop +
                                    (_additionalMsg != null ? 1 : 0);
                Console.CursorLeft = GameState.Config.MsgCursorLeft;
                Console.ForegroundColor = GameState.Config.MsgColorStandard;
                Console.Write(_promptMsg);
                Console.ResetColor();


                Freetext = Console.ReadLine();
                if (Freetext == null)
                    throw new CtrlZException();
                Freetext = Freetext.Trim().Split(null)[0];
            } while (Freetext == "");
        }
    }
}