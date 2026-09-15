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
	/// Executes a local command-line application/tool.
	/// </summary>
	/// <param name="commandLineTool">The full path to the command-line tool to execute or command name for shell commands.</param>
	/// <param name="arguments">The arguments to pass to the command-line tool.</param>
	/// <returns>The result of the command-line tool execution.</returns>
	[OllamaTool]
	public static string CommandLineToolExecute(string commandLineTool, string arguments)
	{
		try
		{
			// 1. Configure the process start information
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = commandLineTool,          // Use "powershell.exe" for PowerShell
				Arguments = arguments,               // arguments to pass to the command-line tool
				RedirectStandardOutput = true,       // Allows C# to read the output
				RedirectStandardError = true,        // Allows C# to read errors
				UseShellExecute = false,             // Required to redirect streams
				CreateNoWindow = true                // Runs the process invisibly
			};

			// 2. Run the process and read the output
			using (Process process = Process.Start(startInfo))
			{
				// Wait for the command to finish
				process.WaitForExit();

				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Executed command-line tool: {commandLineTool} {arguments}");
				sb.AppendLine($"Exit code: {process.ExitCode}");

				// Read the output and errors
				sb.AppendLine("Output:");
				sb.AppendLine(process.StandardOutput.ReadToEnd());

				string errors = process.StandardError.ReadToEnd();
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
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"An error occurred while executing the command-line tool [ {commandLineTool} {arguments} ]:");
			sb.AppendLine($"Error: {ex.Message}");

			Console.WriteLine(sb.ToString());
			return sb.ToString();
		}
	}
}