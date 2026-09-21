using System.Diagnostics;
using System.Text;
using OllamaSharp;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Console that demonstrates tool usage with the Ollama API.
/// </summary>
public partial class ToolConsole
{
	public static object commandLock = new object();
	/// <summary>
	/// Write contents to a file at a given path.
	/// </summary>
	/// <param name="path">The path of the file to write to.</param>
	/// <param name="contents">The contents to write to the file.</param>
	[OllamaTool]
	public static string FileWrite(string path, string contents)
	{
		var parentDirectory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(parentDirectory))
		{
			EnsureDirectoryExists(parentDirectory);
		}

		System.IO.File.WriteAllText(path, contents);
		return $"Contents has been written successfully to: {path}";
	}

	/// <summary>
	/// Delete a file at a given path.
	/// </summary>
	/// <param name="path">The path of the file to delete.</param>
	[OllamaTool]
	public static string FileDelete(string path) { System.IO.File.Delete(path); return $"File deleted: {path}"; }

	/// <summary>
	/// Read file contents from a given path.
	/// </summary>
	/// <param name="path">The path of the file to read.</param>
	/// <returns>The contents of the file.</returns>
	[OllamaTool]
	public static string FileRead(string path) => System.IO.File.ReadAllText(path);

	/// <summary>
	/// Check if a file exists at a given path.
	/// </summary>
	/// <param name="path">The path of the file to check.</param>
	/// <returns>A boolean indicating whether the file exists.</returns>
	[OllamaTool]
	public static string FileExists(string path) => System.IO.File.Exists(path).ToString();

	/// <summary>
	/// Move (rename) a file from one path to another.
	/// </summary>
	/// <param name="oldPath">The current path of the file.</param>
	/// <param name="newPath">The new path for the file.</param>
	/// <returns>A message indicating the result of the operation.</returns>
	[OllamaTool]
	public static string FileMove(string oldPath, string newPath)
	{
		var parentDirectory = Path.GetDirectoryName(newPath);
		if (!string.IsNullOrWhiteSpace(parentDirectory))
		{
			EnsureDirectoryExists(parentDirectory);
		}

		System.IO.File.Move(oldPath, newPath);
		return $"Moved file from {oldPath} to {newPath}";
	}

	/// <summary>
	/// Gets the current working directory.
	/// </summary>
	/// <returns>The current working directory.</returns>
	[OllamaTool]
	public static string GetCurrentDirectory() => System.IO.Directory.GetCurrentDirectory();

	/// <summary>
	/// Sets (changes) the current working directory.
	/// </summary>
	/// <param name="path">The path of the directory to set as the current working directory.</param>
	/// <returns>The current working directory.</returns>
	[OllamaTool]
	public static string SetCurrentDirectory(string path) { System.IO.Directory.SetCurrentDirectory(path); return $"Current directory now is: \"{System.IO.Directory.GetCurrentDirectory()}\""; }

	/// <summary>
	/// Check if a directory exists at a given path.
	/// </summary>
	/// <param name="path">The path of the directory to check.</param>
	/// <returns>A boolean indicating whether the directory exists.</returns>
	[OllamaTool]
	public static string DirectoryExists(string path) => System.IO.Directory.Exists(path).ToString();

	/// <summary>
	/// Ensure a directory exists at a given path. When directory does not exist, it will be created. If any of parent directories do not exist, they will also be created.
	/// </summary>
	/// <param name="path">The path of the directory to ensure.</param>
	/// <returns>A boolean indicating whether the directory exists.</returns>
	[OllamaTool]
	public static string EnsureDirectoryExists(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
		}

		if (!System.IO.Directory.Exists(path))
		{
			var parentDirectory = Path.GetDirectoryName(path);
			if (!string.IsNullOrWhiteSpace(parentDirectory) && !System.IO.Directory.Exists(parentDirectory))
			{
				EnsureDirectoryExists(parentDirectory);
			}

			System.IO.Directory.CreateDirectory(path);
		}

		return System.IO.Directory.Exists(path).ToString();
	}

	/// <summary>
	/// Create a directory at a given path.
	/// </summary>
	/// <param name="path">The path of the directory to create.</param>
	/// <returns>The directory info of the created directory.</returns>
	[OllamaTool]
	public static string DirectoryCreate(string path) { EnsureDirectoryExists(path); return $"Created directory: {path}"; }

	/// <summary>
	/// Delete a directory at a given path.
	/// </summary>
	/// <param name="path">The path of the directory to delete.</param>
	/// <returns>A boolean indicating whether the directory was deleted.</returns>
	[OllamaTool]
	public static string DirectoryDelete(string path) { System.IO.Directory.Delete(path); return $"Deleted directory: {path}"; }

	/// <summary>
	/// Move (rename) a directory from one path to another.
	/// </summary>
	/// <param name="oldPath">The current path of the directory.</param>
	/// <param name="newPath">The new path for the directory.</param>
	/// <returns>A message indicating the result of the operation.</returns>
	[OllamaTool]
	public static string DirectoryMove(string oldPath, string newPath)
	{
		var parentDirectory = Path.GetDirectoryName(newPath);
		if (!string.IsNullOrWhiteSpace(parentDirectory))
		{
			EnsureDirectoryExists(parentDirectory);
		}

		System.IO.Directory.Move(oldPath, newPath);
		return $"Moved directory from {oldPath} to {newPath}";
	}

	/// <summary>
	/// Get the contents of a directory at a given path.
	/// </summary>
	/// <param name="path">The path of the directory to read.</param>
	/// <returns>A list of files and subdirectories in the specified directory.</returns>
	[OllamaTool]
	public static string DirectoryContents(string path) { return $"Contents of directory {path}: {string.Join(System.Environment.NewLine, System.IO.Directory.GetFileSystemEntries(path))}"; }

	/// <summary>
	/// Get the folders and files tree of a current directory.
	/// </summary>
	/// <returns>A list of files and subdirectories in the current directory recursively.</returns>
	[OllamaTool]
	public static string CurrentDirectoryTree()
	{
		return CommandLineToolExecute($"tree -a -n --gitignore --dirsfirst --noreport");
	}

	/// <summary>
	/// Executes a local application/tool with arguments(parameters) and without chaining commands. Just execution of single program.
	/// Program path and arguments must be provided in a single string. Example:
	/// "C:\Program Files\MyApp\myapp.exe --option1 value1 --option2 value2"
	/// The method will parse the command line to separate the executable path from its arguments, execute the command, and return the output and any errors encountered during execution.
	/// </summary>
	/// <param name="commandLine">The complete command or program with parameters for execution.</param>
	/// <returns>The result of the command-line tool execution.</returns>
	[OllamaTool]
	public static string CommandLineToolExecute(string commandLine)
	{
		Console.WriteLine("DEBUG: Executing single-line command-line tool: " + commandLine);

		// 1. Configure the process start information
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = "/bin/bash",
			Arguments = $"-c \"{commandLine.Replace("\"", "\\\"")}\"", // Escape quotes for bash
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
				StringBuilder sb = new StringBuilder();

				// Read the output and errors
				sb.AppendLine("Output:");
				sb.AppendLine(process.StandardOutput.ReadToEnd());

				string errors = process.StandardError.ReadToEnd();
				if (!string.IsNullOrEmpty(errors))
				{
					sb.AppendLine("Errors:");
					sb.AppendLine(errors);
				}

				// Wait for the command to finish
				process.WaitForExit(30000);

				sb.Insert(0, $"Executed command-line tool: \"{startInfo.FileName}\" with parameters \"{startInfo.Arguments}\"{Environment.NewLine}");
				sb.Insert(1, $"Exit code: {process.ExitCode}{Environment.NewLine}");

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
	/// <param name="script">The list of commands to execute. Each line is a separate command to execute. Provided script would be placed into shell script file and executed with /bin/bash.</param>
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

		lock (commandLock)
		{

			File.WriteAllText("script.sh", sb0.ToString());

			Console.WriteLine("DEBUG: Executing multiline script: \r\n" + sb0.ToString());
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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
				using (Process process = Process.Start(startInfo))
				{
					StringBuilder sb = new StringBuilder();

					// Read the output and errors
					sb.AppendLine("Output:");
					sb.AppendLine(process.StandardOutput.ReadToEnd());

					string errors = process.StandardError.ReadToEnd();
					if (!string.IsNullOrEmpty(errors))
					{
						sb.AppendLine("Errors:");
						sb.AppendLine(errors);
					}

					// Wait for the command to finish
					process!.WaitForExit(30000);

					sb.Insert(0, $"Executed multiline script: {Environment.NewLine}{sb0}{Environment.NewLine}");
					sb.Insert(1, $"Exit code: {process.ExitCode}{Environment.NewLine}");

					Console.WriteLine(sb.ToString());
					return sb.ToString();
				}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
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
			finally
			{
				// Clean up the script file after execution
				if (File.Exists("script.sh"))
				{
					File.Delete("script.sh");
				}
			}
		}
	}
}