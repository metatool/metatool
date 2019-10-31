namespace Metatool.NugetPackage
{
    public class NugetRepository
    {
        public int Order { get; set; }
        public bool IsPrivate { get; set; }
        public string Name { get; set; }
        public bool IsPasswordClearText { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
    }
}
