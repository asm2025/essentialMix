namespace asm.Web
{
	/// <summary>
	/// https://github.com/alexanderar/Mvc.CascadeDropDown/blob/master/Mvc.CascadeDropDown/CascadeDropDownOptions.cs
	/// </summary>
	public struct JQueryClientSideEvents
	{
		/// <summary>
		/// Gets or sets the JavaScript function to be called before data is sent to server.
		/// This function could be used to modify the data that is sent to server.
		/// Function gets as argument an object that is going to be sent to the server
		/// Function should return an object that is going to be sent to server in order to fetch the data.
		/// </summary>
		public string BeforeSend { get; set; }

		/// <summary>
		/// Gets or sets the JavaScript function to be called after the data is successfully loaded.
		/// This function could be used to modify the loaded data before it's used to fill the drop down. 
		/// The format of the data should remain the same
		/// </summary>
		public string OnSuccess { get; set; }

		/// <summary>
		/// Gets or sets the JavaScript function to be called if get data request results with failure.
		/// The function receives the following parameters: responseText, responseStatus, statusText
		/// </summary>
		public string OnFailure { get; set; }

		/// <summary>
		/// Gets or sets the JavaScript function to be called when get data request is completed (both in case of success and failure).
		/// </summary>
		public string OnComplete { get; set; }
	}
}
