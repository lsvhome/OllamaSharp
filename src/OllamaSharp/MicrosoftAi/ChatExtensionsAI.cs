using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models.Chat;

namespace OllamaSharp;

using ChatRole = OllamaSharp.Models.Chat.ChatRole;

/// <summary>
/// An implementation of <see cref="IChat"/> that uses the <see cref="IChatClient"/> from Microsoft.Extensions.AI.
/// </summary>
public class ChatExtensionsAI : IChat
{
	public event EventHandler<MessageEventArgs>? OnBeforeSend;
	public event EventHandler<ChatEventArgs>? OnThink;

	public event EventHandler<ChatEventArgs>? OnThought;

	/// <summary>
	/// Gets fired when the AI model wants to invoke a tool.
	/// </summary>
	public event EventHandler<ToolCallEventArgs>? OnToolCall;

	/// <summary>
	/// Gets fired after a tool was invoked and the result is available.
	/// </summary>
	public event EventHandler<ToolResultEventArgs>? OnToolResult;

	public event EventHandler<MessageEventArgs>? OnMessage;

	private readonly IChatClient _client;

	public List<Message> Messages { get; set; } = [];

	public string Model { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChatExtensionsAI"/> class.
	/// </summary>
	/// <param name="client">The <see cref="IChatClient"/> to use for communication with the AI backend.</param>
	/// <param name="model">The model to chat with.</param>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="client"/> parameter is <c>null</c>.</exception>
	public ChatExtensionsAI(IChatClient client, string model)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		Model = model;
	}

	public IAsyncEnumerable<string> SendAsync(string message, object? state = null, CancellationToken cancellationToken = default)
		=> SendAsAsync(ChatRole.User, message, state: state, cancellationToken: cancellationToken);

	public IAsyncEnumerable<string> SendAsync(string message, IEnumerable<string>? imagesAsBase64, object? state = null, CancellationToken cancellationToken = default)
		=> SendAsAsync(ChatRole.User, message, imagesAsBase64: imagesAsBase64, state: state, cancellationToken: cancellationToken);

	public IAsyncEnumerable<string> SendAsync(string message, IEnumerable<object>? tools, IEnumerable<string>? imagesAsBase64 = null, object? format = null, object? state = null, CancellationToken cancellationToken = default)
		=> SendAsAsync(ChatRole.User, message, tools: tools, imagesAsBase64: imagesAsBase64, format: format, state: state, cancellationToken: cancellationToken);

	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, object? state = null, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, tools: null, imagesAsBase64: null, state: state, cancellationToken: cancellationToken);

	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<string>? imagesAsBase64, object? state = null, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, tools: null, imagesAsBase64: imagesAsBase64, state: state, cancellationToken: cancellationToken);

	public async IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<object>? tools, IEnumerable<string>? imagesAsBase64 = null, object? format = null, object? state = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var inputMessage = new Message(role, message, imagesAsBase64?.ToArray());
		Messages.Add(inputMessage);

		var chatMessages = Messages.Select(AbstractionMapper.ToChatMessage).ToList();

		var options = new ChatOptions
		{
			ModelId = Model,
		};

		// Using dynamic to bypass compile-time check and see if it works at runtime
		// but this is just a hack to find the method name.
		// In a real scenario, I'd check the assembly.
		//dynamic client = _client;
		var response = await _client.GetResponseAsync(chatMessages, options).ConfigureAwait(false);
		//response.
		var content = response.Text ?? string.Empty;
		yield return content;

		var assistantMessage = new Message(ChatRole.Assistant, content);
		Messages.Add(assistantMessage);
	}
}
