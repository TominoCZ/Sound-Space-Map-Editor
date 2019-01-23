using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Blox_Saber_Editor
{
	class UndoRedo
	{
		private readonly List<UndoRedoAction> _actions = new List<UndoRedoAction>();

		public bool CanUndo => _actions.LastOrDefault(a => !a.Undone) != null;
		public bool CanRedo => _actions.FirstOrDefault(a => a.Undone) != null;

		public void AddUndoRedo(Action undo, Action redo)
		{
			_actions.RemoveAll(a => a.Undone);
			_actions.Add(new UndoRedoAction(undo, redo));
		}

		public void Undo()
		{
			var action = _actions.LastOrDefault(a => !a.Undone);

			if (action == null)
				return;

			if (EditorWindow.Instance.GuiScreen is GuiScreenEditor editor)
				editor.ShowToast("UNDO", Color.Chartreuse);

			action.Undo?.Invoke();
			action.Undone = true;
		}

		public void Redo()
		{
			var action = _actions.FirstOrDefault(a => a.Undone);

			if (action == null)
				return;

			if (EditorWindow.Instance.GuiScreen is GuiScreenEditor editor)
				editor.ShowToast("REDO", Color.Chartreuse);

			action.Redo?.Invoke();
			action.Undone = false;
		}
	}
}