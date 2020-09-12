using System.Threading.Tasks;

namespace asm.Threading.Extensions
{
	public static class TaskStatusExtension
	{
		public static bool IsReady(this TaskStatus thisValue)
		{
			switch (thisValue)
			{
				case TaskStatus.Created:
				case TaskStatus.WaitingForActivation:
					return true;
				default:
					return false;
			}
		}

		public static bool IsRunning(this TaskStatus thisValue)
		{
			switch (thisValue)
			{
				case TaskStatus.Running:
				case TaskStatus.WaitingForChildrenToComplete:
					return true;
				default:
					return false;
			}
		}

		public static bool IsStarted(this TaskStatus thisValue)
		{
			switch (thisValue)
			{
				case TaskStatus.Running:
				case TaskStatus.WaitingForChildrenToComplete:
				case TaskStatus.RanToCompletion:
				case TaskStatus.Canceled:
				case TaskStatus.Faulted:
					return true;
				default:
					return false;
			}
		}

		public static bool IsFinished(this TaskStatus thisValue)
		{
			switch (thisValue)
			{
				case TaskStatus.RanToCompletion:
				case TaskStatus.Canceled:
				case TaskStatus.Faulted:
					return true;
				default:
					return false;
			}
		}
	}
}