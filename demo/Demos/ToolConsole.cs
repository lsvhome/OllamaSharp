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
public partial class ToolConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	public static List<string> CommandHistory = new List<string>();

	//public static List<OllamaSharp.Models.Chat.Tool> Tools1 { get; } = [new GetWeatherTool(), new GetLatLonAsyncTool(), new GetPopulationTool()];
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

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather, population or the GPS coordinates for a city, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{LIST_TOOLS_COMMAND}[/] to list all available tools.[/]");

				var chat = new Chat(Ollama, systemPrompt) { Think = Think };
				chat.OnThink += (sender, thoughts) => AnsiConsole.MarkupInterpolated($"[{AiThinkTextColor}]{thoughts}[/]");
				chat.OnToolResult += (sender, result) => RenderToolResult(result);

				string message;

				do
				{
					AnsiConsole.WriteLine();
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

					ConditionalCancellationTokenSource s = new ConditionalCancellationTokenSource(TimeSpan.FromMinutes(10));
					s.OnTimeoutElapsed += (object sender, ConditionalCancellationTokenSourceEventArgs aargs) =>
					{
						AnsiConsole.MarkupLine("Timeout elapsed - interrupt ot still wait for response? [i\\W]");
						bool userAnswered = false;
						do
						{
							using (var _timer = new Timer((state) => { Console.SetIn(new StringReader(Environment.NewLine)); }, new object(), (int)(aargs.ElapsedTimeout.Divide(2.0)).TotalMilliseconds, 0))
							{

								var answer = Console.ReadLine() ?? "";

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
									AnsiConsole.MarkupLine("User input didn't recognized - interrupt ot still wait for response? [i\\W]");
								}
							}
						}
						while (!userAnswered);
					};

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

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static void RenderToolResult(ToolResult toolResult)
	{
		AnsiConsole.MarkupLine($"[gray]Tool: [/][purple]{RenderToolSignature(toolResult)}[/]");
		AnsiConsole.MarkupLineInterpolated($"      = [blue]{toolResult.Result?.ToString() ?? ""}[/]");
		AnsiConsole.WriteLine("");
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
				builder.Append($"[gray]{separator}{argument.Key}:[/] {argument.Value}");
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