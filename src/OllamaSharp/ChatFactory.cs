using OllamaSharp.Models.Chat;

namespace OllamaSharp;

using Microsoft.Extensions.Configuration;

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
	IChat CreateChat(string model, ThinkValue thinkLevel);

	string GetDefaultModelName();

	Task<IEnumerable<string>> ListLocalModelsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory for creating <see cref="Chat"/> instances for Ollama.
/// </summary>
public class OllamaChatFactory : IChatFactory
{
	private readonly IOllamaApiClient _client;
	private readonly IConfiguration _configuration;

	public OllamaChatFactory(IOllamaApiClient client, IConfiguration configuration)
	{
		_client = client;
		_configuration = configuration;
	}

	public IChat CreateChat(string model, ThinkValue thinkLevel)
	{
		return new Chat(_client)
		{
			Model = model,
			Think = ThinkValue.Medium
		};
	}

	public string GetDefaultModelName()
	{
		string defaultModelName = _configuration["OLLAMA_MODEL"] ?? throw new ApplicationException("Environment variable OLLAMA_MODEL not configured");
		return defaultModelName;
	}

	public async Task<IEnumerable<string>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		return (await _client.ListLocalModelsAsync(cancellationToken)).Select(m => m.Name);
	}
}

/// <summary>
/// Factory for creating <see cref="ChatExtensionsAI"/> instances for llama.cpp or other OpenAI-compatible backends using Microsoft.Extensions.AI.
/// </summary>
public class LlamaCppChatFactory : IChatFactory
{
	private readonly Microsoft.Extensions.AI.IChatClient _client;
	private readonly IConfiguration _configuration;

	public LlamaCppChatFactory(Microsoft.Extensions.AI.IChatClient client, IConfiguration configuration)
	{
		_client = client;
		_configuration = configuration;
	}

	public IChat CreateChat(string model, ThinkValue thinkLevel)
	{
		return new ChatExtensionsAI(_client, model);
	}

	public string GetDefaultModelName()
	{
		string defaultModelName = _configuration["OLLAMA_MODEL"] ?? throw new ApplicationException("Environment variable OLLAMA_MODEL not configured");
		return defaultModelName;
	}

	public Task<IEnumerable<string>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		var models = new string[1] { this.GetDefaultModelName() };
		return Task.FromResult(models.AsEnumerable());
	}

}
