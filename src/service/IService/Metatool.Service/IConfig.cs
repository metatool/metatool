using System;

namespace Metatool.Service;

public interface IConfig<out T>
{
	/// <summary>
	/// Returns the current <typeparamref name="T" /> instance/>.
	/// </summary>
	T CurrentValue { get; }

	// /// <summary>
	// /// Returns a configured <typeparamref name="T" /> instance with the given name.
	// /// </summary>
	// T Get(string name);

	/// <summary>
	/// Registers a listener to be called whenever a named <typeparamref name="T" /> changes.
	/// </summary>
	/// <param name="listener">The action to be invoked when <typeparamref name="T" /> has changed.</param>
	/// <returns>An <see cref="T:System.IDisposable" /> which should be disposed to stop listening for changes.</returns>
	IDisposable OnChange(Action<T, string> listener);
}