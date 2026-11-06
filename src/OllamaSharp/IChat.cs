namespace OllamaSharp;

using OllamaSharp.Models.Chat;

/// <summary>
/// Interface for a chat session that handles message history and interaction with an AI model.
/// </summary>
public interface IChat
{
	/// <summary>
	/// Gets or sets the messages of the chat history.
	/// </summary>
	List<Message> Messages { get; set; }

	/// <summary>
	/// Gets or sets the AI model to chat with.
	/// </summary>
	string Model { get; set; }

	/// <summary>
	/// Sends a message to the currently selected model and streams its response.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsync(string message, object? state = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsync(string message, IEnumerable<string>? imagesAsBase64, object? state = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <param name="tools">A collection of tools that the model can utilize.</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model.</param>
	/// <param name="format">Specifies the response format.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsync(string message, IEnumerable<object>? tools, IEnumerable<string>? imagesAsBase64 = null, object? format = null, object? state = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response.
	/// </summary>
	/// <param name="role">The role in which the message should be sent.</param>
	/// <param name="message">The message to be sent as a string.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, object? state = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response.
	/// </summary>
	/// <param name="role">The role in which the message should be sent.</param>
	/// <param name="message">The message to be sent as a string.</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<string>? imagesAsBase64, object? state = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response.
	/// </summary>
	/// <param name="role">The role in which the message should be sent.</param>
	/// <param name="message">The message to be sent as a string.</param>
	/// <param name="tools">A collection of tools that the model can utilize.</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model.</param>
	/// <param name="format">Specifies the response format.</param>
	/// <param name="state">Defines the state object which would be returned with events.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable stream of string responses from the model.</returns>
	IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<object>? tools, IEnumerable<string>? imagesAsBase64 = null, object? format = null, object? state = null, CancellationToken cancellationToken = default);
}
