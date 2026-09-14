using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
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
	/// Write contents to a file at a given path.
	/// </summary>
	/// <param name="path">The path of the file to write to.</param>
	/// <param name="contents">The contents to write to the file.</param>
	//[OllamaTool]
	public static void FileWrite(string path, string contents) => System.IO.File.WriteAllText(path, contents);

	/// <summary>
	/// Read file contents from a given path.
	/// </summary>
	/// <param name="path">The path of the file to read.</param>
	/// <returns>The contents of the file.</returns>
	//[OllamaTool]
	public static string FileRead(string path) => System.IO.File.ReadAllText(path);

	/// <summary>
	/// Executes a shell script using Bash.
	/// </summary>
	/// <param name="script">The shell script to execute.</param>
	/// <returns>The result of the shell script execution.</returns>
	[OllamaTool]
	public static string BashExecute(string script)
	{
		try{
		// TODO: Execute the shell script and return the output
		string shellFileName = "/bin/bash";

		string escapedScript = script.Replace("\"", "\\\"");
		// 1. Configure the process start information
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = shellFileName,          // Use "powershell.exe" for PowerShell
			Arguments = $"-c \"{escapedScript}\"",    // "-c" tells bash to run the command and exit
			RedirectStandardOutput = true,     // Allows C# to read the output
			RedirectStandardError = true,      // Allows C# to read errors
			UseShellExecute = false,           // Required to redirect streams
			CreateNoWindow = true              // Runs the process invisibly
		};

		Console.WriteLine($"Arguments: [ {startInfo.Arguments} ]");

		// 2. Run the process and read the output
		using (Process process = Process.Start(startInfo))
		{
			// Read the output and errors
			string result = process.StandardOutput.ReadToEnd();
			string errors = process.StandardError.ReadToEnd();

			process.WaitForExit(); // Wait for the command to finish

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Executed script: {script}");
			sb.AppendLine($"Exit code: {process.ExitCode}");

			// 3. Use the result
			sb.AppendLine("Output:");
			sb.AppendLine(result);

			if (!string.IsNullOrEmpty(errors))
			{
				sb.AppendLine("Errors:");
				sb.AppendLine(errors);
			}

			Console.WriteLine(sb.ToString());
			return sb.ToString();
		}

		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred while executing the script: {ex.Message}");
			return $"Error: {ex.Message}";
		}
	}
}