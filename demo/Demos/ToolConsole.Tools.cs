using System.Reflection;
using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Exceptions;
using OllamaSharp.Tools;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Console that demonstrates tool usage with the Ollama API.
/// </summary>
public partial class ToolConsole
{

	/// <summary>
	/// Units of temperature measurement.
	/// </summary>
	public enum Unit
	{
		/// <summary>
		/// Celsius temperature unit.
		/// </summary>
		Celsius,
		/// <summary>
		/// Fahrenheit temperature unit.
		/// </summary>
		Fahrenheit
	}

	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for.</param>
	/// <param name="unit">The unit to measure the temperature in.</param>
	/// <returns>The weather description for the specified location.</returns>
	[OllamaTool]
	public static string GetWeather(string location, Unit unit) => $"It's cold at only 6° {unit} in {location}.";

	/// <summary>
	/// Gets the latitude and longitude for a given location.
	/// </summary>
	/// <param name="location">The location to get the latitude and longitude for.</param>
	/// <returns>A string containing the latitude and longitude.</returns>
	[OllamaTool]
	public async static Task<string> GetLatLonAsync(string location)
	{
		await Task.Delay(200).ConfigureAwait(false);
		return $"{new Random().Next(20, 50)}.4711, {new Random().Next(3, 15)}.0815";
	}

	/// <summary>
	/// Gets the amount of people living in a given city.
	/// </summary>
	/// <param name="city">The city to get the population info for.</param>
	/// <returns>The population of the specified city.</returns>
	[OllamaTool]
	public static int GetPopulation(string city) => new Random().Next(1000, 10000000);
}