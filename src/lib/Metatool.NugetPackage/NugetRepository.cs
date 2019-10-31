namespace Metatool.NugetPackage
{
    public class NugetRepository
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public int Order { get; set; } = 0;
        public bool IsPrivate { get; set; } = false;
        public bool IsPasswordClearText { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
