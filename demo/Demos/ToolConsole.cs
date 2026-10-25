using System.Reflection;
using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Exceptions;
using OllamaSharp.Tools;
using Spectre.Console;
using static OllamaSharp.Chat;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Console that demonstrates tool usage with the Ollama API.
/// </summary>
public partial class ToolConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	internal const string SystemPrompt = @"You are a Senior Software Architect.
You can use available tools and functions for your work.
When source changes are requested, request is completed only when all requested changes are committed.
When source changes are requested, they should be split into small, meaningful pieces.
Each source change should be committed with a relevant message.
Build & Quality: The solution must be buildable, and all warnings must be fixed. If a warning cannot be fixed, it must be documented in the 'Issues' paragraph of technical-design.md with an explanation of why it cannot be fixed.
Testing: All tests must be passing (green).
Documentation: The technical-design.md or DESIGN.md file (if it is present) must be updated to reflect the new state of the system.
Formatting: for dotnet projects and solutions dotnet format must be executed before committing changes.
Commit Message: The commit message must clearly reflect the changes that have been implemented. After excecuting commit command git repo should be verified commit is available, commit hash is present. 
Follow repo-specific rules in file AGENTS.md when it is present.
When user request completed, working folder should not contain uncommitted files.
When user request completed type 'Task completed in {commit hashes}'.
				";
	public static List<string> CommandHistory = new List<string>();

	//public static List<OllamaSharp.Models.Chat.Tool> Tools1 { get; } = [new FileWriteTool()];
	/// <summary>
	/// Gets the collection of available tools.
	/// </summary>
	public List<OllamaSharp.Models.Chat.Tool> Tools { get; } = Assembly.GetExecutingAssembly().GetTypes()
		.Where(t => typeof(OllamaSharp.Models.Chat.Tool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
		.Select(t => (OllamaSharp.Models.Chat.Tool)Activator.CreateInstance(t)!)
		//.Union(Tools1)
		.Distinct()
		.ToList();

	/// <inheritdoc/>
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Tool chat").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			var keepChatting = true;
			var systemPrompt = ReadInput($"Define a system prompt [{HintTextColor}](optional)[/]");
			if (string.IsNullOrWhiteSpace(systemPrompt))
				systemPrompt = SystemPrompt;

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather, population or the GPS coordinates for a city, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{LIST_TOOLS_COMMAND}[/] to list all available tools.[/]");

				var chat = new Chat(Ollama, systemPrompt) { Think = Think };
				//chat.OnThink += (sender, thoughts) => AnsiConsole.MarkupInterpolated($"[{AiThinkTextColor}]{thoughts}[/]");
				chat.OnThink += (sender, thoughts) => { Console.WriteLine(DateTime.Now); Console.WriteLine(thoughts.Text); Console.WriteLine(DateTime.Now); };
				chat.OnToolResult += (sender, result) => RenderToolResult(result);

				string message;

				do
				{
					AnsiConsole.WriteLine();
					Console.Beep();
					message = ReadInput();

					if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = false;
						break;
					}

					if (message.Equals(TOGGLETHINK_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						ToggleThink();
						keepChatting = true;
						chat.Think = Think;
						continue;
					}

					if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						break;
					}

					if (message.Equals(LIST_TOOLS_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						ListTools(Tools);
						break;
					}

					CommandHistory.Add(message);

					if (string.IsNullOrWhiteSpace(message))
					{
						AnsiConsole.MarkupLine($"[{WarningTextColor}]Please enter a message or command.[/]");
						continue;
					}

					//AnsiConsole.MarkupLine("Timeout happened - interrupt or still wait for response? [[i/W]]");

					CancellationTokenSource s = new CancellationTokenSource();
					/*
					s.OnTimeoutElapsed += (object sender, ConditionalCancellationTokenSourceEventArgs aargs) =>
					{
						bool userAnswered = false;
						do
						{
							using (var _timer = new Timer((state) => { Console.SetIn(new StringReader(Environment.NewLine)); }, new object(), (int)(aargs.ElapsedTimeout.Divide(2.0)).TotalMilliseconds, 0))
							{
								AnsiConsole.MarkupLine("Timeout happened - interrupt or still wait for response? [[i/W]]");
								var answer = Console.ReadLine() ?? "";

								if(s.IsCancellationRequested)
								{
									userAnswered = true;
									AnsiConsole.MarkupLine("Cancellation requested - exiting timeout handler.");
									break;
								}

								if ("w" != answer.Trim().ToLower() || "" != answer.Trim().ToLower())
								{
									userAnswered = true;
									aargs.NewTimeout = aargs.ElapsedTimeout;
									AnsiConsole.MarkupLine($"Waiting prolonged for {aargs.NewTimeout}");
								}
								else if ("i" != answer.Trim().ToLower())
								{
									userAnswered = true;
									AnsiConsole.MarkupLine($"Waiting interrupted");
									//// do nothing - default behavior
									//// aargs.NewTimeout == Zero and cancellation event happened
								}
								else
								{
									AnsiConsole.MarkupLine("User input didn't recognized - interrupt ot still wait for response? [[i/W]]");
								}
							}

							Console.SetIn(new StreamReader(Console.OpenStandardInput()));
						}
						while (!userAnswered);
					};
					*/

					try
					{
						var resp = chat.SendAsync(message, Tools, cancellationToken: s.Token);

						await foreach (var answerToken in resp)
							AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
					}
					catch (OllamaException ex)
					{
						AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"An error occurred while sending the message: {ex.Message}");
					}

					/*
					try
					{
						s.Cancel();
					}
					catch (Exception ex)
					{
						//AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}
					*/

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static void RenderToolResult(ToolResultEventArgs toolResultEventArgs)
	{
		Console.WriteLine(DateTime.Now);
		try
		{
			Console.WriteLine($"{RenderToolSignature(toolResultEventArgs.ToolResult)}");
			Console.WriteLine($"      = {toolResultEventArgs.ToolResult.Result?.ToString() ?? ""}");
			AnsiConsole.WriteLine("");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred while rendering tool result: {ex.Message}");
		}

		Console.WriteLine(DateTime.Now);
	}

	private static string RenderToolSignature(ToolResult toolResult)
	{
		var builder = new StringBuilder(toolResult.Tool.Function?.Name ?? "Unknown");
		builder.Append('(');

		var separator = "";

		if (toolResult.ToolCall?.Function?.Arguments is not null)
		{
			foreach (var argument in toolResult.ToolCall.Function.Arguments)
			{
				builder.Append($"{separator}{argument.Key}: {argument.Value}");
				separator = ", ";
			}
		}

		builder.Append(")");
		return builder.ToString();
	}

	private static void ListTools(IEnumerable<OllamaSharp.Models.Chat.Tool> tools)
	{
		AnsiConsole.MarkupLine("\n[purple]Available tools:[/]");

		foreach (var tool in tools)
		{
			AnsiConsole.MarkupLineInterpolated($"{tool.Function?.Name ?? "Unknown"}\t\t[purple]{tool.Function?.Description}[/]");
		}
	}
}