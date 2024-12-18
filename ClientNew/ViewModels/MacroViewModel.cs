using SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels;
using SharpMacroPlayer.ClientNew.Views.Pages;
using SharpMacroPlayer.Macros.MacroElements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
	public partial class MacroElementSelectable(MacroElement element) : ObservableObject
	{
		[ObservableProperty]
		private MacroElement _element = element;
		[ObservableProperty]
		private bool _selected = false;
    }
    public sealed partial class MacroViewModel : ObservableValidator
	{
		[ObservableProperty]
		private ObservableCollection<MacroElementSelectable> _macroElements = [];
		[ObservableProperty]
		private MacroElement? _selectedMacroElement;

		[ObservableProperty]
		private string _macroFile = string.Empty;
		partial void OnMacroFileChanging(string value) => OnPropertyChanging(nameof(MacroName));
		partial void OnMacroFileChanged(string value) => OnPropertyChanged(nameof(MacroName));
        public string MacroName { get => Path.GetFileNameWithoutExtension(MacroFile); }

		private MacroElementEditorViewModel _editorViewModel;
		private INavigationWindow _navigationView;

		public MacroViewModel(MacroElementEditorViewModel editorViewModel, INavigationWindow navigationView)
		{
			_editorViewModel = editorViewModel;
			_navigationView = navigationView;
		}

		public Macro GetMacro() => new(MacroElements.Select(e => e.Element));
		public void SetMacro(string macroFile)
		{
			MacroElements = new(MacroLoaderSaver.LoadMacro(macroFile).MacroElements.Select(e => new MacroElementSelectable(e)));
			MacroFile = macroFile;
        }

		[RelayCommand]
		private void MoveDown(MacroElementSelectable macroElement)
		{
			int index = MacroElements.IndexOf(macroElement);
			if (index < MacroElements.Count - 1)
				(MacroElements[index], MacroElements[index + 1]) = (MacroElements[index + 1], MacroElements[index]);
		}
		[RelayCommand]
		private void MoveDownSelected()
		{
            List<MacroElementSelectable> itemsToMove = [];
            foreach (MacroElementSelectable macroElement in MacroElements)
                if (macroElement.Selected) itemsToMove.Add(macroElement);
			itemsToMove.Reverse();
            foreach (MacroElementSelectable macroElement in itemsToMove)
                MoveDown(macroElement);
        }

		[RelayCommand]
		private void MoveUp(MacroElementSelectable macroElement)
		{
			int index = MacroElements.IndexOf(macroElement);
			if (index > 0 && MacroElements.Count > 1)
				(MacroElements[index], MacroElements[index - 1]) = (MacroElements[index - 1], MacroElements[index]);
		}
		[RelayCommand]
		private void MoveUpSelected()
		{
            List<MacroElementSelectable> itemsToMove = [];
            foreach (MacroElementSelectable macroElement in MacroElements)
                if (macroElement.Selected) itemsToMove.Add(macroElement);
            foreach (MacroElementSelectable macroElement in itemsToMove)
                MoveUp(macroElement);
        }

		[RelayCommand]
		private void Remove(MacroElementSelectable macroElement) => MacroElements.Remove(macroElement);
		[RelayCommand]
		private void RemoveSelected()
		{
			List<MacroElementSelectable> itemsToRemove = [];
            foreach (MacroElementSelectable macroElement in MacroElements)
                if (macroElement.Selected) itemsToRemove.Add(macroElement);
			foreach (MacroElementSelectable macroElement in itemsToRemove)
				Remove(macroElement);
        }

		[RelayCommand]
		private void Edit(MacroElementSelectable macroElement)
		{
			_editorViewModel.ElementViewModel = MacroElementViewModel.GetViewModelForMacroElement(macroElement.Element);
			_editorViewModel.ElementIndex = MacroElements.IndexOf(macroElement);
			_navigationView.Navigate(typeof(MacroElementEditorPage));
		}
		[RelayCommand]
		private void EditSelected()
		{
			foreach (MacroElementSelectable macroElement in MacroElements)
				if (macroElement.Selected)
				{
					Edit(macroElement);
					break;
				}
        }

		public void SaveElement(int index, MacroElement element) => MacroElements[index] = new(element);


		[RelayCommand]
		private void AddWaitTimeMacroElement() => MacroElements.Add(new(new WaitTimeMacroElement(100)));
		[RelayCommand]
		private void AddLoopMacroElement() => MacroElements.Add(new(new LoopMacroElement(3)));
		[RelayCommand]
		private void AddEndOfLoopMacroElement() => MacroElements.Add(new(new EndOfLoopMacroElement()));
		[RelayCommand]
		private void AddMouseEventMacroElement() => MacroElements.Add(new(new MouseEventMacroElement(true, true, MouseButton.LMB, false, true, 100, 100)));
		[RelayCommand]
		private void AddKeyboardEventMacroElement() => MacroElements.Add(new(new KeyboardEventMacroElement([(ushort)VK.VK_W], [KeyEventType.VIRTUAL_KEY], true, true)));
		[RelayCommand]
		private void SaveMacro() => MacroLoaderSaver.SaveMacros(GetMacro(), MacroFile);
		[RelayCommand]
		private void AddVkCode(VK vk) => MacroElements.Add(new(new KeyboardEventMacroElement([(ushort)vk], [KeyEventType.VIRTUAL_KEY], true, true)));
    }
}
