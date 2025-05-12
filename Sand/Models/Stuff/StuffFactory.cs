using Microsoft.Xna.Framework;
using Sand.Models.Stuff;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Sand;

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

	public Stuff Get(string name)
	{
		if (!_stuffDescriptors.TryGetValue(name, out StuffDescriptor descriptor))
		{
			throw new ArgumentException($"Cant create Stuff from name \"{name}\"!");
		}
		return new Stuff(descriptor);
	}

	private void LoadDescriptorsFromFile(string filename)
	{
		using var filestream = File.OpenRead(filename);
		using var streamReader = new StreamReader(filestream);

		var descriptors = _yamlDeserializer.Deserialize<List<StuffDescriptor>>(streamReader);

		for (int i = 0; i < descriptors.Count; i++)
		{
			var d = descriptors[i];
			if (d.ColorsSource != null && d.ColorsSource.Count > 0)
			{
				var colors = new Color[d.ColorsSource.Count];
				for (var j = 0; j < d.ColorsSource.Count; j++)
				{
					var rgba = d.ColorsSource[j];

					var c = new Color
					{
						R = rgba[0],
						G = rgba[1],
						B = rgba[2],
						A = rgba.Length >= 3 ? rgba[3] : (byte)1
					};

					colors[j] = c;
				}
				d.Colors = colors;
			}
			_stuffDescriptors.AddOrUpdate(d.Name, d, (name, desc) => desc);
		}
		//foreach (var d in descriptors)
		//{
		//	if (d.ColorsSource != null && d.ColorsSource.Count > 0)
		//	{
		//		var colors = new Color[d.ColorsSource.Count];
		//		for (var i = 0; i < d.ColorsSource.Count; i++)
		//		{
		//			var rgba = d.ColorsSource[i];

		//			var c = new Color
		//			{
		//				R = rgba[0],
		//				G = rgba[1],
		//				B = rgba[2],
		//				A = rgba.Length >= 3 ? rgba[3] : (byte)1
		//			};

		//			colors[i] = c;
		//		}
		//		d.Colors = colors;
		//	}
		//	_stuffDescriptors.AddOrUpdate(descriptor.Name, descriptor, (name, desc) => desc);
		//}
	}

	[GeneratedRegex("^StuffDescriptors.*\\.yaml$")]
	private static partial Regex StuffDescriptorFilenames();
}
