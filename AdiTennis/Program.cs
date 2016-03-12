using System;
using AdiTennis.Logic;
using AdiTennis.StageAbstract.Stages;

namespace AdiTennis
{
    internal class Program
    {
        private static void Main()
        {
            IStage game = new PrimaryLogic();
            game.Go();

            // ReSharper disable once EmptyEmbeddedStatement
            while (!Console.KeyAvailable) ;
        }
    }
}