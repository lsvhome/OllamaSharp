namespace OllamaSharp;

/// <summary>
/// Factory for creating <see cref="IChat"/> instances.
/// </summary>
public interface IChatFactory
{
	/// <summary>
	/// Creates a new chat instance.
	/// </summary>
	/// <param name="model">The model to use for the chat.</param>
	/// <returns>A new <see cref="IChat"/> instance.</returns>
	IChat CreateChat(string model);
}

/// <summary>
/// Factory for creating <see cref="Chat"/> instances for Ollama.
/// </summary>
public class OllamaChatFactory : IChatFactory
{
	private readonly IOllamaApiClient _client;

	public OllamaChatFactory(IOllamaApiClient client)
	{
		_client = client;
	}

	public IChat CreateChat(string model)
	{
		return new Chat(_client)
		{
			Model = model,
		};
	}
}

/// <summary>
/// Factory for creating <see cref="ChatExtensionsAI"/> instances for llama.cpp or other OpenAI-compatible backends using Microsoft.Extensions.AI.
/// </summary>
public class LlamaCppChatFactory : IChatFactory
{
	private readonly Microsoft.Extensions.AI.IChatClient _client;

	public LlamaCppChatFactory(Microsoft.Extensions.AI.IChatClient client)
	{
		_client = client;
	}

	public IChat CreateChat(string model)
	{
		return new ChatExtensionsAI(_client, model);
	}
}
