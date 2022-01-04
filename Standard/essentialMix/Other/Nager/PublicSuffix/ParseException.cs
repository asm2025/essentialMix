using System;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix;

/// <summary>
/// Parse Exception
/// </summary>
public class ParseException : Exception
{
	/// <summary>
	/// Parse Exception
	/// </summary>
	/// <param name="errorMessage"></param>
	/// <param name="winningRule"></param>
	public ParseException(string errorMessage, TldRule winningRule = null)
	{
		ErrorMessage = errorMessage;
		WinningRule = winningRule;
	}

	/// <summary>
	/// Reason of exception
	/// </summary>
	public TldRule WinningRule { get; }

	/// <summary>
	/// Reason of exception
	/// </summary>
	public string ErrorMessage { get; }

	/// <summary>
	/// Message
	/// </summary>
	public override string Message => ErrorMessage;
}