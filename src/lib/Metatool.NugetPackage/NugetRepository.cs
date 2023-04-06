using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;

namespace Metatool.NugetPackage;

public class NugetRepository
{
	public string Name { get; set; }
	public string Source { get; set; }
	public int Order { get; set; } = 0;
	public bool IsPrivate { get; set; } = false;
	public bool IsPasswordClearText { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }

	static public List<NugetRepository> GetRepos(IEnumerable<PackageSource> sources) =>
		sources.Select(s => new NugetRepository() {Name = s.Name, Source = s.Source}).ToList();
}