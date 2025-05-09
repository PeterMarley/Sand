using Sand.Models.Stuff;
using Sand.Services;
using Sand.Stuff.StuffDescriptors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Sand.Stuff;

public partial class StuffFactory
{
	#region Singleton
	private static StuffFactory _instance;
	public static StuffFactory Instance { get; set; } = _instance ??= new StuffFactory();
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
			dir = Path.Combine(AppContext.BaseDirectory, "Data");
			var filenames = Directory.EnumerateFiles(dir);
			var filereadtasks = new List<Task>();
			foreach (var filename in filenames.Where(f => StuffDescriptorFilenames().IsMatch(Path.GetFileName(f))))
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

	public AbstractStuff Get(string name)
	{
		if (!_stuffDescriptors.TryGetValue(name, out StuffDescriptor descriptor))
		{
			throw new ArgumentException($"Cant create Stuff from name \"{name}\"!");
		}

		if (!string.IsNullOrEmpty(descriptor.SpriteSource))
		{
			return new FileSpriteStuff(descriptor);
		}

		//// this one has descriptor.ColorRgba by default, or should do
		//return new PolygonStuff(descriptor);
		throw new InvalidOperationException("Unhandles stuff name dpassed to Get");
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

	[GeneratedRegex("^StuffDescriptors.*\\.yaml$")]
	private static partial Regex StuffDescriptorFilenames();
}
