using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.UI;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class UpdatePanelExtension
{
	public static void ChildrenAsTriggers([NotNull] this UpdatePanel thisValue, bool includeChildren = true)
	{
		if (thisValue.ChildrenAsTriggers)
		{
			foreach (Control control in thisValue.Controls)
			{
				foreach (Control ctl in control.Controls)
					ControlAsTriggerInternal(thisValue, ctl, includeChildren);
			}

			return;
		}

		foreach (Control control in thisValue.Controls)
			ControlAsTriggerInternal(thisValue, control, includeChildren);
	}

	[ItemNotNull]
	public static IEnumerable<UpdatePanelTrigger> FindTriggers([NotNull] this UpdatePanel thisValue, Control control)
	{
		if (thisValue.Triggers.Count == 0 || string.IsNullOrEmpty(control.ID) && string.IsNullOrEmpty(control.UniqueID)) yield break;

		StringComparer comparer = StringComparer.OrdinalIgnoreCase;

		foreach (UpdatePanelTrigger trigger in thisValue.Triggers)
		{
			switch (trigger)
			{
				case AsyncPostBackTrigger asyncPostBackTrigger:
					if (!string.IsNullOrEmpty(control.ID) && comparer.Equals(control.ID, asyncPostBackTrigger.ControlID) ||
						!string.IsNullOrEmpty(control.UniqueID) && comparer.Equals(control.UniqueID, asyncPostBackTrigger.ControlID)
						) yield return trigger;
					break;
				case PostBackTrigger postBackTrigger:
					if (!string.IsNullOrEmpty(control.ID) && comparer.Equals(control.ID, postBackTrigger.ControlID) ||
						!string.IsNullOrEmpty(control.UniqueID) && comparer.Equals(control.UniqueID, postBackTrigger.ControlID)
						) yield return trigger;
					break;
			}
		}
	}

	[ItemNotNull]
	public static IEnumerable<UpdatePanelTrigger> FindTriggers(this UpdatePanel thisValue, string controlID)
	{
		if (string.IsNullOrEmpty(controlID) || thisValue.Triggers.Count == 0) yield break;

		StringComparer comparer = StringComparer.OrdinalIgnoreCase;

		foreach (UpdatePanelTrigger trigger in thisValue.Triggers)
		{
			switch (trigger)
			{
				case AsyncPostBackTrigger asyncPostBackTrigger:
					if (comparer.Equals(controlID, asyncPostBackTrigger.ControlID)) yield return trigger;
					break;
				case PostBackTrigger postBackTrigger:
					if (comparer.Equals(controlID, postBackTrigger.ControlID)) yield return trigger;
					break;
			}
		}
	}

	public static UpdatePanelTrigger FindTrigger([NotNull] this UpdatePanel thisValue, Control control, string eventName = null)
	{
		return string.IsNullOrEmpty(eventName)
					? FindTriggers(thisValue, control).FirstOrDefault()
					: FindTriggers(thisValue, control)
					.OfType<AsyncPostBackTrigger>()
					.FirstOrDefault(e => StringComparer.OrdinalIgnoreCase.Equals(eventName, e.EventName));
	}

	public static UpdatePanelTrigger FindTrigger(this UpdatePanel thisValue, string controlID, string eventName = null)
	{
		return string.IsNullOrEmpty(eventName)
					? FindTriggers(thisValue, controlID).FirstOrDefault()
					: FindTriggers(thisValue, controlID)
					.OfType<AsyncPostBackTrigger>()
					.FirstOrDefault(e => StringComparer.OrdinalIgnoreCase.Equals(eventName, e.EventName));
	}

	public static bool HasTrigger(this UpdatePanel thisValue, string controlID, string eventName = null) { return FindTrigger(thisValue, controlID, eventName) != null; }

	public static bool HasTrigger([NotNull] this UpdatePanel thisValue, Control control, string eventName = null) { return FindTrigger(thisValue, control, eventName) != null; }

	/// <summary>
	/// https://docs.microsoft.com/en-us/dotnet/api/system.web.ui.asyncpostbacktrigger
	/// The control that the AsyncPostBackTrigger control references must be in the same naming container as the update panel for which it is a trigger. Triggers that are based on controls in other naming containers are not supported.
	/// </summary>
	/// <param name="updatePanel"></param>
	/// <param name="control"></param>
	/// <param name="includeChildren"></param>
	public static void ControlAsTrigger([NotNull] this UpdatePanel updatePanel, [NotNull] Control control, bool includeChildren = false)
	{
		if (updatePanel.NamingContainer != control.NamingContainer) throw new ArgumentException("The control that the AsyncPostBackTrigger control references must be in the same naming container as the update panel for which it is a trigger.", nameof(control));
		ControlAsTriggerInternal(updatePanel, control, includeChildren);
	}

	private static void ControlAsTriggerInternal([NotNull] UpdatePanel updatePanel, [NotNull] Control control, bool includeChildren = false)
	{
		if (updatePanel.NamingContainer != control.NamingContainer || HasTrigger(updatePanel, control) || !IsSupportedPanelType(control.GetType())) return;
		updatePanel.Triggers.Add(new AsyncPostBackTrigger { ControlID = control.UniqueID });
		if (!includeChildren || control.Controls.Count == 0) return;

		foreach (Control ctl in control.Controls)
			ControlAsTriggerInternal(updatePanel, ctl, true);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static bool IsSupportedPanelType(Type type)
	{
		return type != null && (type.IsAssignableFrom(typeof(INamingContainer)) || type.IsAssignableFrom(typeof(IPostBackDataHandler)) || type.IsAssignableFrom(typeof(IPostBackEventHandler)));
	}
}