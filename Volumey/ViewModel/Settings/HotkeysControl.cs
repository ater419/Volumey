﻿using System;
using System.Windows.Input;
using Volumey.Controls;
using Volumey.Helper;
using Volumey.DataProvider;

namespace Volumey.ViewModel.Settings
{
	public enum HotkeysState
	{
		NotRegistered,
		Enabled,
		Disabled
	}
	
	public static class HotkeysControl
	{
		/// <summary>
		/// Invokes when a hotkey manager is set and is ready to register hotkeys
		/// </summary>
		internal static event Action Activated;
		internal static event Action<HotKey> HotkeyPressed;
		internal static event Action OpenHotkeyPressed;
		internal static bool IsActive { get; private set; }

		internal static HotkeysState VolumeHotkeysState { get; private set; } = HotkeysState.NotRegistered;

		private static int volumeStep = 1;
		public static int VolumeStep
		{
			get => volumeStep;
			private set => volumeStep = value;
		}

		private static HotKey openMixer;
		private static IHotkeyManager hotkeyManager;

		public static bool RegisterVolumeHotkeys(HotKey volUp, HotKey volDown)
		{
			if(hotkeyManager == null)
				throw new NullReferenceException("Hotkey manager is not set");
			try
			{
				hotkeyManager.RegisterHotkey(volUp);
				hotkeyManager.RegisterHotkey(volDown);
				if(VolumeHotkeysState == HotkeysState.NotRegistered)
					VolumeHotkeysState = HotkeysState.Enabled;
				return true;
			}
			catch
			{
				hotkeyManager.UnregisterHotkey(volUp);
				hotkeyManager.UnregisterHotkey(volDown);
				throw;
			}
		}

		public static void UnregisterVolumeHotkeys(HotKey volUp, HotKey volDown)
		{
			if(hotkeyManager == null)
				throw new NullReferenceException("Hotkey manager is not set");
			try
			{
				hotkeyManager.UnregisterHotkey(volUp);
				hotkeyManager.UnregisterHotkey(volDown);
				if(hotkeyManager.RegisteredHotkeysCount == 0)
					VolumeHotkeysState = HotkeysState.NotRegistered;
			}
			catch { }
		}

		public static bool RegisterOpenMixerHotkey(HotKey hotkey)
		{
			if(hotkeyManager == null)
				return false;
			if(hotkey == null)
				return false;
			try
			{
				openMixer = hotkey;
				hotkeyManager.RegisterHotkey(hotkey);
				return true;
			}
			catch { }
			return false;
		}

		public static void UnregisterOpenMixerHotkey(HotKey hotkey)
		{
			if(hotkeyManager == null)
				return;
			if(hotkey == null)
				return;
			try
			{
				openMixer = null;
				hotkeyManager.UnregisterHotkey(hotkey);
			}
			catch {}
		}

		public static ErrorMessageType HotkeyIsValid(HotKey hotkey)
		{
			if(hotkey == null)
				return ErrorMessageType.OpenReg;
			if(hotkey.ModifierKeys == ModifierKeys.None && hotkey.Key == Key.F12)
				return ErrorMessageType.F12;
			if(SettingsProvider.Settings.HotkeyExists(hotkey))
				return ErrorMessageType.HotkeyExists;
			return ErrorMessageType.None;
		}

		public static ErrorMessageType HotkeysAreValid(HotKey up, HotKey down)
		{
			if(up == null || down == null)
				return ErrorMessageType.VolumeReg;
			if((up.ModifierKeys == ModifierKeys.None && up.Key == Key.F12) ||
			   (down.ModifierKeys == ModifierKeys.None && down.Key == Key.F12))
				return ErrorMessageType.F12;
			if(up.Equals(down))
				return ErrorMessageType.Diff;
			if(SettingsProvider.Settings.HotkeysExist(up, down))
				return ErrorMessageType.HotkeyExists;
			return ErrorMessageType.None;
		}

		internal static void SetHotkeysState(HotkeysState newState)
		{
			VolumeHotkeysState = newState;
		}

		internal static void SetVolumeStep(int newValue)
		{
			VolumeStep = newValue;
		}

		public static void SetHotkeyManager(IHotkeyManager hm)
		{
			hotkeyManager = hm;
            hm.HotkeyPressed += OnHotkeyPressed;
            Activated?.Invoke();
            IsActive = true;
		}

		private static void OnHotkeyPressed(HotKey hotkey)
		{
			if(VolumeHotkeysState == HotkeysState.Enabled)
				HotkeyPressed?.Invoke(hotkey);
			if(openMixer != null && hotkey.Equals(openMixer))
				OpenHotkeyPressed?.Invoke();
		}

		public static void Dispose()
		{
			hotkeyManager.Dispose();
		}
	}
}