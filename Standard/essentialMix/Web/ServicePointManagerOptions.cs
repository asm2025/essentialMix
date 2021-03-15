namespace essentialMix.Web
{
	public struct ServicePointManagerOptions
	{
		public bool? SkipCertificateValidation { get; set; }
		public bool? CertificateRevocationList { get; set; }
		public int? DefaultConnectionLimit { get; set; }
		public int? DnsRefreshTimeout { get; set; }
		public bool? DnsRoundRobin { get; set; }
		public bool? Expect100Continue { get; set; }
		public int? ServicePointIdleTime { get; set; }
		public int? MaxServicePoints { get; set; }
		public bool? ReusePort { get; set; }
	}
}