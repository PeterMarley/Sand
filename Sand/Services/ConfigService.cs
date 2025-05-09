using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Sand.Services;

public class ConfigService
{
	#region Singleton
	public static ConfigService _instance;
	public static ConfigService Instance => _instance ??= new();
	#endregion
	private IDeserializer _yamlDeserializer;

	/// <summary>A Stuff element is this many pixels per side.</summary>
	public int StuffScale { get; private set; }// = 15;
	/// <summary>Window resolution x</summary>
	public int ResolutionX { get; private set; } //= 1920;
	/// <summary>Window resolution y</summary>
	public int ResolutionY { get; private set; }// = 1080;
	/// <summary>The number of X-axis elements</summary>
	public int StuffWidth { get; private set; }// = RESOLUTION_X / STUFF_SCALE;
	/// <summary>The number of Y-axis elements</summary>
	public int StuffHeight { get; private set; }// = RESOLUTION_Y / STUFF_SCALE;

	private ConfigService() 
	{
		throw new NotImplementedException(nameof(ConfigService));
		_yamlDeserializer = new DeserializerBuilder()
			.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
			.Build();

		var path = Path.Combine(AppContext.BaseDirectory, "Data", "settings.yaml");
		using var fileStream = File.OpenRead(path);
		using var reader  = new StreamReader(fileStream);

		var fileContents = reader.ReadToEnd();

		var yDoc = new YamlDocument(fileContents);

		//StuffScale = yDoc.RootNode
	}
}
