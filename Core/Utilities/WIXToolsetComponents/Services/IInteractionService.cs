using System;
using JetBrains.Annotations;

namespace WIXToolsetComponents.Services
{
	public interface IInteractionService
    {
		bool IsHandleCreated { get; }
		IntPtr MainWindowHandle { get; }
        
		void OnUI([NotNull] Action body);
        void CloseAndExit();
    }
}
