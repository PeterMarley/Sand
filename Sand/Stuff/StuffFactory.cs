using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Sand.Stuff;

public class StuffFactory
{
	#region Singleton
	private static StuffFactory _instance;
	public static StuffFactory Instance = _instance ??= new StuffFactory();
	#endregion

	private ConcurrentDictionary<string, StuffDescriptor> _stuffDescriptors = [];
	private IDeserializer _yamlDeserializer;

	private StuffFactory() 
	{
		_yamlDeserializer = new DeserializerBuilder()
			.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
			.Build();
	}

	public async Task LoadMaterialsAsync() 
	{
		string dir = null;
		try
		{
			dir = Path.Combine(AppContext.BaseDirectory, "Stuff");
			var filenames = Directory.EnumerateFiles(dir);
			var filereadtasks = new List<Task>();
			foreach (var filename in filenames.Where(f => Regex.IsMatch(Path.GetFileName(f), "^StuffDescriptors.*\\.yaml$")))
			{
				filereadtasks.Add(Task.Run(() => LoadDescriptorsFromFile(filename)));
			}

			await Task.WhenAll(filereadtasks);
		}
		catch (Exception materialFileLoadEx)
		{
			Logger.Instance.LogError(materialFileLoadEx, $"Failed to load materials from files in directory \"{dir}\"");
			throw;
		}
	}

	public StuffBasic Get(string name)
	{
		if (!_stuffDescriptors.TryGetValue(name, out StuffDescriptor descriptor))
		{
			throw new ArgumentException($"Cant create Stuff from name \"{name}\"!");
		}
		return new StuffBasic(descriptor);
	}

	private void LoadDescriptorsFromFile(string filename)
	{
		using var filestream = File.OpenRead(filename);
		using var streamReader = new StreamReader(filestream);

		var descriptors = _yamlDeserializer.Deserialize<IEnumerable<StuffDescriptor>>(streamReader);

		foreach (var descriptor in descriptors)
		{
			_stuffDescriptors.AddOrUpdate(descriptor.Name, descriptor, (name, desc) => desc);
		}
	}
}
