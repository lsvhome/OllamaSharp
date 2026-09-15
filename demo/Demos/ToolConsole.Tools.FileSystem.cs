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
	[OllamaTool]
	public static void FileWrite(string path, string contents) => System.IO.File.WriteAllText(path, contents);

	/// <summary>
	/// Read file contents from a given path.
	/// </summary>
	/// <param name="path">The path of the file to read.</param>
	/// <returns>The contents of the file.</returns>
	[OllamaTool]
	public static string FileRead(string path) => System.IO.File.ReadAllText(path);

	/// <summary>
	/// Executes a local command-line application/tool with arguments(parameters).
	/// </summary>
	/// <param name="commandLine">The complete command or program with parameters for execution.</param>
	/// <returns>The result of the command-line tool execution.</returns>
	[OllamaTool]
	public static string CommandLineToolExecute(string commandLine)
	{
		Console.WriteLine("DEBUG: Executing command-line tool: " + commandLine);
		// 1. Configure the process start information
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = commandLine,          // Use "powershell.exe" for PowerShell
			Arguments = string.Empty,               // arguments to pass to the command-line tool
			RedirectStandardOutput = true,       // Allows C# to read the output
			RedirectStandardError = true,        // Allows C# to read errors
			UseShellExecute = false,             // Required to redirect streams
			CreateNoWindow = true                // Runs the process invisibly
		};


		if (commandLine.StartsWith("\""))
		{
			string pattern = "^\\s*\"(?<command>.*)\"\\s";
			//string pattern = @"^\s*""(?<command>.*)""\s";
			var match = System.Text.RegularExpressions.Regex.Match(pattern, commandLine);
			startInfo.FileName = match.Groups["command"].Value;
			startInfo.Arguments = commandLine.Substring(startInfo.FileName.Length + 2).Trim(); // +2 to account for the quotes and space
		}
		else
		{
			startInfo.FileName =  commandLine.Split(' ')[0];
			startInfo.Arguments = commandLine.Substring(startInfo.FileName.Length).Trim();
		}

		try
		{
			// 2. Run the process and read the output
			using (Process process = Process.Start(startInfo))
			{
				// Wait for the command to finish
				process.WaitForExit();

				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Executed command-line tool: \"{startInfo.FileName}\" with parameters \"{startInfo.Arguments}\"");
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
			sb.AppendLine($"An error occurred while executing the command-line tool [ \"{startInfo.FileName}\" with parameters \"{startInfo.Arguments}\" ]:");
			sb.AppendLine($"Error: {ex.Message}");

			Console.WriteLine(sb.ToString());
			return sb.ToString();
		}
	}

	/// <summary>
	/// Executes a list of commands sequentially.
	/// </summary>
	/// <param name="script">The list of commands to execute. Each line is a separate command to execute.</param>
	/// <returns>The result of the command-line tool execution.</returns>
	[OllamaTool]
	public static string MultilineScriptExecute(string script)
	{
		StringBuilder sb0 = new StringBuilder();
		
		sb0.AppendLine("#!/bin/bash");
		sb0.AppendLine("# BEGIN OF SCRIPT");
		sb0.AppendLine();
		sb0.AppendLine(script);
		sb0.AppendLine();
		sb0.AppendLine("# END OF SCRIPT");

		File.WriteAllText("script.sh", sb0.ToString());

		Console.WriteLine("DEBUG: Executing script: \r\n" + sb0.ToString());
		// 1. Configure the process start information
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = "/bin/bash",              // Use "powershell.exe" for PowerShell
			Arguments = "script.sh",             // arguments to pass to the command-line tool
			RedirectStandardOutput = true,       // Allows C# to read the output
			RedirectStandardError = true,        // Allows C# to read errors
			UseShellExecute = false,             // Required to redirect streams
			CreateNoWindow = true                // Runs the process invisibly
		};

		try
		{
			// 2. Run the process and read the output
			using (Process process = Process.Start(startInfo))
			{
				// Wait for the command to finish
				process.WaitForExit();

				StringBuilder sb = new StringBuilder();
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
			sb.AppendLine($"An error occurred while executing multiline script:");
			sb.AppendLine(script);
			sb.AppendLine("--------------------------------");
			sb.AppendLine($"Error: {ex.Message}");

			Console.WriteLine(sb.ToString());
			return sb.ToString();
		}
	}
}