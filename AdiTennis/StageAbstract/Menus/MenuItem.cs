namespace AdiTennis.StageAbstract.Menus
{
    internal class MenuItem
    {
        public bool IsSelected = false;
        public string Name;

        public MenuItem(string name)
        {
            Name = name;
        }
    }
}