using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix
{
	public static class SystemInfo
	{
		private static ushort? __adminPasswordStatus;
		private static bool? __automaticManagedPageFile;
		private static bool? __automaticResetBootOption;
		private static bool? __automaticResetCapability;
		private static ushort? __bootOptionOnLimit;
		private static ushort? __bootOptionOnWatchDog;
		private static bool? __bootROMSupported;
		private static string __bootupState;
		private static ushort[] __bootStatus;
		private static string __caption;
		private static ushort? __chassisBootupState;
		private static string __chassisSkuNumber;
		private static string __creationClassName;
		private static short? __currentTimeZone;
		private static bool? __daylightInEffect;
		private static string __description;
		private static string __dnsHostName;
		private static string __domain;
		private static ushort? __domainRole;
		private static bool? __enableDaylightSavingsTime;
		private static ushort? __frontPanelResetStatus;
		private static bool? __hypervisorPresent;
		private static bool? __infraredSupported;
		private static string[] __initialLoadInfo;
		private static DateTime? __installDate;
		private static ushort? __keyboardPasswordStatus;
		private static string __lastLoadInfo;
		private static string __manufacturer;
		private static string __model;
		private static string __name;
		private static string __nameFormat;
		private static bool? __networkServerModeEnabled;
		private static uint? __numberOfLogicalProcessors;
		private static uint? __numberOfProcessors;
		private static byte[] __oemLogoBitmap;
		private static string[] __oemStringArray;
		private static bool? __partOfDomain;
		private static long? __pauseAfterReset;
		private static ushort? __pcSystemType;
		private static ushort? __pcSystemTypeEx;
		private static ushort[] __powerManagementCapabilities;
		private static bool? __powerManagementSupported;
		private static ushort? __powerOnPasswordStatus;
		private static ushort? __powerState;
		private static ushort? __powerSupplyState;
		private static string __primaryOwnerContact;
		private static string __primaryOwnerName;
		private static ushort? __resetCapability;
		private static short? __resetCount;
		private static short? __resetLimit;
		private static string[] __roles;
		private static string __status;
		private static string[] __supportContactDescription;
		private static string __systemFamily;
		private static string __systemSkuNumber;
		private static ushort? __systemStartupDelay;
		private static string[] __systemStartupOptions;
		private static byte? __systemStartupSetting;
		private static string __systemType;
		private static ushort? __thermalState;
		private static ulong? __totalPhysicalMemory;
		private static string __userName;
		private static ushort? __wakeUpType;
		private static string __workgroup;

		[NotNull]
		public static IEnumerable<ManagementObject> Get([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>() 
					: collection.OfType<ManagementObject>().Where(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return Enumerable.Empty<ManagementObject>();
			}
		}

		[NotNull]
		public static IEnumerable<T> Get<T>([NotNull] SystemInfoRequest<T> request) { return Get((SystemInfoRequest)request).Select(request.Converter); }

		public static ManagementObject Single([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().Single() 
					: collection.OfType<ManagementObject>().Single(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T Single<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(Single((SystemInfoRequest)request)); }

		public static ManagementObject SingleOrDefault([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().SingleOrDefault() 
					: collection.OfType<ManagementObject>().SingleOrDefault(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T SingleOrDefault<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(SingleOrDefault((SystemInfoRequest)request)); }

		public static ManagementObject First([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().First() 
					: collection.OfType<ManagementObject>().First(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T First<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(First((SystemInfoRequest)request)); }

		public static ManagementObject FirstOrDefault([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().FirstOrDefault() 
					: collection.OfType<ManagementObject>().FirstOrDefault(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T FirstOrDefault<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(FirstOrDefault((SystemInfoRequest)request)); }

		public static ManagementObject Last([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().Last() 
					: collection.OfType<ManagementObject>().Last(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T Last<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(Last((SystemInfoRequest)request)); }

		public static ManagementObject LastOrDefault([NotNull] SystemInfoRequest request)
		{
			(ManagementObjectSearcher searcher, ManagementObjectCollection collection) = GetObjects(request);

			try
			{
				return request.Filter == null 
					? collection.OfType<ManagementObject>().LastOrDefault() 
					: collection.OfType<ManagementObject>().LastOrDefault(mo => request.Filter(mo));
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
				return null;
			}
		}

		public static T LastOrDefault<T>([NotNull] SystemInfoRequest<T> request) { return request.Converter(LastOrDefault((SystemInfoRequest)request)); }

		 public static ushort AdminPasswordStatus => __adminPasswordStatus ??= GetSystemValue<ushort>(nameof(AdminPasswordStatus));
		 public static bool AutomaticManagedPageFile => __automaticManagedPageFile ??= GetSystemValue<bool>(nameof(AutomaticManagedPageFile));
		 public static bool AutomaticResetBootOption => __automaticResetBootOption ??= GetSystemValue<bool>(nameof(AutomaticResetBootOption));
		 public static bool AutomaticResetCapability => __automaticResetCapability ??= GetSystemValue<bool>(nameof(AutomaticResetCapability));
		 public static ushort BootOptionOnLimit => __bootOptionOnLimit ??= GetSystemValue<ushort>(nameof(BootOptionOnLimit));
		 public static ushort BootOptionOnWatchDog => __bootOptionOnWatchDog ??= GetSystemValue<ushort>(nameof(BootOptionOnWatchDog));
		 public static bool BootROMSupported => __bootROMSupported ??= GetSystemValue<bool>(nameof(BootROMSupported));
		 public static string BootupState => __bootupState ??= GetSystemValue(nameof(BootupState), string.Empty);
		 public static ushort[] BootStatus => __bootStatus ??= GetSystemValue(nameof(BootStatus), Array.Empty<ushort>());
		 public static string Caption => __caption ??= GetSystemValue(nameof(Caption), string.Empty);
		 public static ushort ChassisBootupState => __chassisBootupState ??= GetSystemValue<ushort>(nameof(ChassisBootupState));
		 public static string ChassisSKUNumber => __chassisSkuNumber ??= GetSystemValue(nameof(ChassisSKUNumber), string.Empty);
		 public static string CreationClassName => __creationClassName ??= GetSystemValue(nameof(CreationClassName), string.Empty);
		 public static short CurrentTimeZone => __currentTimeZone ??= GetSystemValue<short>(nameof(CurrentTimeZone));
		 public static bool DaylightInEffect => __daylightInEffect ??= GetSystemValue<bool>(nameof(DaylightInEffect));
		 public static string Description => __description ??= GetSystemValue(nameof(Description), string.Empty);
		 public static string DNSHostName => __dnsHostName ??= GetSystemValue(nameof(DNSHostName), string.Empty);
		 public static string Domain => __domain ??= GetSystemValue(nameof(Domain), string.Empty);
		 public static ushort DomainRole => __domainRole ??= GetSystemValue<ushort>(nameof(DomainRole));
		 public static bool EnableDaylightSavingsTime => __enableDaylightSavingsTime ??= GetSystemValue<bool>(nameof(EnableDaylightSavingsTime));
		 public static ushort FrontPanelResetStatus => __frontPanelResetStatus ??= GetSystemValue<ushort>(nameof(FrontPanelResetStatus));
		 public static bool HypervisorPresent => __hypervisorPresent ??= GetSystemValue<bool>(nameof(HypervisorPresent));
		 public static bool InfraredSupported => __infraredSupported ??= GetSystemValue<bool>(nameof(InfraredSupported));
		 public static string[] InitialLoadInfo => __initialLoadInfo ??= GetSystemValue(nameof(InitialLoadInfo), Array.Empty<string>());
		 public static DateTime InstallDate => __installDate ??= GetSystemValue<DateTime>(nameof(InstallDate));
		 public static ushort KeyboardPasswordStatus => __keyboardPasswordStatus ??= GetSystemValue<ushort>(nameof(KeyboardPasswordStatus));
		 public static string LastLoadInfo => __lastLoadInfo ??= GetSystemValue(nameof(LastLoadInfo), string.Empty);
		 public static string Manufacturer => __manufacturer ??= GetSystemValue(nameof(Manufacturer), string.Empty);
		 public static string Model => __model ??= GetSystemValue(nameof(Model), string.Empty);
		 public static string Name => __name ??= GetSystemValue(nameof(Name), string.Empty);
		 public static string NameFormat => __nameFormat ??= GetSystemValue(nameof(NameFormat), string.Empty);
		 public static bool NetworkServerModeEnabled => __networkServerModeEnabled ??= GetSystemValue<bool>(nameof(NetworkServerModeEnabled));
		 public static uint NumberOfLogicalProcessors => __numberOfLogicalProcessors ??= GetSystemValue<uint>(nameof(NumberOfLogicalProcessors));
		 public static uint NumberOfProcessors => __numberOfProcessors ??= GetSystemValue<uint>(nameof(NumberOfProcessors));
		 public static byte[] OEMLogoBitmap => __oemLogoBitmap ??= GetSystemValue(nameof(OEMLogoBitmap), Array.Empty<byte>());
		 public static string[] OEMStringArray => __oemStringArray ??= GetSystemValue(nameof(OEMStringArray), Array.Empty<string>());
		 public static bool PartOfDomain => __partOfDomain ??= GetSystemValue<bool>(nameof(PartOfDomain));
		 public static long PauseAfterReset => __pauseAfterReset ??= GetSystemValue<long>(nameof(PauseAfterReset));
		 public static ushort PCSystemType => __pcSystemType ??= GetSystemValue<ushort>(nameof(PCSystemType));
		 public static ushort PCSystemTypeEx => __pcSystemTypeEx ??= GetSystemValue<ushort>(nameof(PCSystemTypeEx));
		 public static ushort[] PowerManagementCapabilities => __powerManagementCapabilities ??= GetSystemValue(nameof(PowerManagementCapabilities), Array.Empty<ushort>());
		 public static bool PowerManagementSupported => __powerManagementSupported ??= GetSystemValue<bool>(nameof(PowerManagementSupported));
		 public static ushort PowerOnPasswordStatus => __powerOnPasswordStatus ??= GetSystemValue<ushort>(nameof(PowerOnPasswordStatus));
		 public static ushort PowerState => __powerState ??= GetSystemValue<ushort>(nameof(PowerState));
		 public static ushort PowerSupplyState => __powerSupplyState ??= GetSystemValue<ushort>(nameof(PowerSupplyState));
		 public static string PrimaryOwnerContact => __primaryOwnerContact ??= GetSystemValue(nameof(PrimaryOwnerContact), string.Empty);
		 public static string PrimaryOwnerName => __primaryOwnerName ??= GetSystemValue(nameof(PrimaryOwnerName), string.Empty);
		 public static ushort ResetCapability => __resetCapability ??= GetSystemValue<ushort>(nameof(ResetCapability));
		 public static short ResetCount => __resetCount ??= GetSystemValue<short>(nameof(ResetCount));
		 public static short ResetLimit => __resetLimit ??= GetSystemValue<short>(nameof(ResetLimit));
		 public static string[] Roles => __roles ??= GetSystemValue(nameof(Roles), Array.Empty<string>());
		 public static string Status => __status ??= GetSystemValue(nameof(Status), string.Empty);
		 public static string[] SupportContactDescription => __supportContactDescription ??= GetSystemValue(nameof(SupportContactDescription), Array.Empty<string>());
		 public static string SystemFamily => __systemFamily ??= GetSystemValue(nameof(SystemFamily), string.Empty);
		 public static string SystemSKUNumber => __systemSkuNumber ??= GetSystemValue(nameof(SystemSKUNumber), string.Empty);
		 public static ushort SystemStartupDelay => __systemStartupDelay ??= GetSystemValue<ushort>(nameof(SystemStartupDelay));
		 public static string[] SystemStartupOptions => __systemStartupOptions ??= GetSystemValue(nameof(SystemStartupOptions), Array.Empty<string>());
		 public static byte SystemStartupSetting => __systemStartupSetting ??= GetSystemValue<byte>(nameof(SystemStartupSetting));
		 public static string SystemType => __systemType ??= GetSystemValue(nameof(SystemType), string.Empty);
		 public static ushort ThermalState => __thermalState ??= GetSystemValue<ushort>(nameof(ThermalState));
		 public static ulong TotalPhysicalMemory => __totalPhysicalMemory ??= GetSystemValue<ulong>(nameof(TotalPhysicalMemory));
		 public static string UserName => __userName ??= GetSystemValue(nameof(UserName), string.Empty);
		 public static ushort WakeUpType => __wakeUpType ??= GetSystemValue<ushort>(nameof(WakeUpType));
		 public static string Workgroup => __workgroup ??= GetSystemValue(nameof(Workgroup), string.Empty);
		 public static bool IsHyperThreadingEnabled => NumberOfLogicalProcessors > NumberOfProcessors;

		private static T GetSystemValue<T>([NotNull] string name, T defaultValue = default(T))
		{
			SystemInfoRequest request = new SystemInfoRequest(SystemInfoType.Win32_ComputerSystem)
			{
				Scope = "root\\CIMV2"
			};

			ManagementObject mo = FirstOrDefault(request);
			if (mo == null) return defaultValue;

			object value;

			try
			{
				value = mo[name];
			}
			catch
			{
				value = null;
			}

			return value.To(defaultValue);
		}

		private static (ManagementObjectSearcher Searcher, ManagementObjectCollection Collection) GetObjects([NotNull] SystemInfoRequest request)
		{
			string search = $"Select {request.SelectExpression} From {request.Type}";
			if (!string.IsNullOrEmpty(request.WhereExpression)) search += " where " + request.WhereExpression;

			ManagementObjectSearcher searcher = null;
			ManagementObjectCollection collection = null;
			ManagementScope scope = string.IsNullOrEmpty(request.ScopeRoot)
				? null
				: new ManagementScope(request.ScopeRoot);

			try
			{
				searcher = new ManagementObjectSearcher(scope, new ObjectQuery(search), request.Options);
				collection = searcher.Get();
			}
			catch
			{
				ObjectHelper.Dispose(ref collection);
				ObjectHelper.Dispose(ref searcher);
			}

			return (searcher, collection);
		}
	}
}