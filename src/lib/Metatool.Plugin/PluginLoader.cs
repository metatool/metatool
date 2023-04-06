using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Threading;
using Metatool.Metatool.Plugin.Loader;

namespace Metatool.Metatool.Plugin;

/// <summary>
/// This loader attempts to load binaries for execution (both managed assemblies and native libraries)
/// in the same way that .NET Core would if they were originally part of the .NET Core application.
/// <para>
/// This loader reads configuration files produced by .NET Core (.deps.json and runtimeconfig.json)
/// as well as a custom file (*.config files). These files describe a list of .dlls and a set of dependencies.
/// The loader searches the plugin path, as well as any additionally specified paths, for binaries
/// which satisfy the plugin's requirements.
/// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md">
/// https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md
/// </seealso>
/// </para>
/// </summary>
public class PluginLoader : IDisposable
{
	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
		=> CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes,
		Action<PluginConfig> configure)
	{
		return CreateFromAssemblyFile(assemblyFile,
			sharedTypes,
			config =>
			{
				config.IsUnloadable = isUnloadable;
				configure(config);
			});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
		=> CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes,
		Action<PluginConfig> configure)
	{
		return CreateFromAssemblyFile(assemblyFile,
			config =>
			{
				if (sharedTypes != null)
				{
					foreach (var type in sharedTypes)
					{
						config.SharedAssemblies.Add(type.Assembly.GetName());
					}
				}

				configure(config);
			});
	}

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile)
		=> CreateFromAssemblyFile(assemblyFile, _ => { });

	public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Action<PluginConfig> configure)
	{
		if (configure == null)
		{
			throw new ArgumentNullException(nameof(configure));
		}

		var config = new PluginConfig(assemblyFile);
		configure(config);
		//Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
		//return Application.Current.Dispatcher.Invoke(() => new PluginLoader(config));
		return new PluginLoader(config);
	}

	private readonly PluginConfig        _config;
	private readonly WeakReference       _contextWeekReference;
	private          AssemblyLoadContext Context => _contextWeekReference.Target as AssemblyLoadContext;
	internal         bool       IsAlive => _contextWeekReference.IsAlive;
	private volatile bool       _disposed;
	private          Assembly   _mainAssembly;
	private static   Dispatcher _dispatcher;

	public PluginLoader(PluginConfig config)
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
		_config = config ?? throw new ArgumentNullException(nameof(config));
		var alc = CreateLoadContext(config);
		_contextWeekReference = new WeakReference(alc, trackResurrection: true);
	}

	public bool IsUnloadable => Context.IsCollectible;

	internal AssemblyLoadContext LoadContext => Context;

	public Assembly MainAssembly => _mainAssembly ??= LoadMainAssembly();


	public Assembly LoadMainAssembly()
	{
		EnsureNotDisposed();
		return Context.LoadFromAssemblyPath(_config.MainAssemblyPath);
	}

	public Assembly LoadAssembly(AssemblyName assemblyName)
	{
		EnsureNotDisposed();
		return Context.LoadFromAssemblyName(assemblyName);
	}

	public Assembly LoadAssemblyFromPath(string assemblyPath)
		=> Context.LoadFromAssemblyPath(assemblyPath);

	public Assembly LoadAssembly(string assemblyName)
	{
		return LoadAssembly(new AssemblyName(assemblyName));
	}

	public void Dispose()
	{
		_mainAssembly = null;
		if (_disposed)
		{
			return;
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		_disposed = true;
		if (IsUnloadable)
		{
			_dispatcher.BeginInvoke(() => Context?.Unload());
			for (var i = 0; _contextWeekReference.IsAlive && (i < 10); i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}
	}

	private void EnsureNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PluginLoader));
		}
	}

	private static AssemblyLoadContext CreateLoadContext(PluginConfig config)
	{
		var builder = new AssemblyLoadContextBuilder();

		builder.SetMainAssemblyPath(config.MainAssemblyPath);

		foreach (var ext in config.PrivateAssemblies)
		{
			builder.PreferLoadContextAssembly(ext);
		}

		if (config.PreferSharedTypes)
		{
			builder.PreferDefaultLoadContext(true);
		}

		if (config.IsUnloadable)
		{
			builder.EnableUnloading();
		}

		foreach (var assemblyName in config.SharedAssemblies)
		{
			builder.PreferDefaultLoadContextAssembly(assemblyName);
		}


		builder.SetupSharedAssemblyPrefix(config.SharedAssemblyPrefixes);

		return builder.Build();
	}
}