namespace essentialMix.Web
{
	public struct CallbackOptions
	{
		public CallbackOptions(string onCallbackCompleteName)
		{
			Target = null;
			Argument = null;
			OnCallbackCompleteName = onCallbackCompleteName;
			OnBeforeCallbackScript = null;
			OnErrorName = null;
			Asynchronous = false;
		}

		public CallbackOptions(string onCallbackCompleteName, string argument)
		{
			Target = null;
			Argument = argument;
			OnCallbackCompleteName = onCallbackCompleteName;
			OnBeforeCallbackScript = null;
			OnErrorName = null;
			Asynchronous = false;
		}

		public CallbackOptions(string onCallbackCompleteName, string argument, bool asynchronous)
		{
			Target = null;
			Argument = argument;
			OnCallbackCompleteName = onCallbackCompleteName;
			OnBeforeCallbackScript = null;
			OnErrorName = null;
			Asynchronous = asynchronous;
		}

		public CallbackOptions(string onCallbackCompleteName, string argument, bool asynchronous, string onErrorName)
		{
			Target = null;
			Argument = argument;
			OnCallbackCompleteName = onCallbackCompleteName;
			OnBeforeCallbackScript = null;
			OnErrorName = onErrorName;
			Asynchronous = asynchronous;
		}

		public string Target;
		public string Argument;
		public string OnCallbackCompleteName;
		public string OnBeforeCallbackScript;
		public string OnErrorName;
		public bool Asynchronous;
	}
}