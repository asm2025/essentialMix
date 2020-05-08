namespace asm.Network
{
	public struct NetworkJoinInformation
	{
		public string ComputerName;
		public string Domain;
		public NetworkJoinStatus Status;

		public static void Zero(ref NetworkJoinInformation info)
		{
			info.ComputerName = null;
			info.Domain = null;
			info.Status = NetworkJoinStatus.Unknown;
		}
	}
}