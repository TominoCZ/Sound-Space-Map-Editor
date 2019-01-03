using System;
using System.Collections.Generic;
using System.Linq;

namespace Blox_Saber_Editor
{
	class UndoRedo
	{
		private readonly List<UndoRedoAction> _actions = new List<UndoRedoAction>();

		public void AddUndoRedo(Action undo, Action redo)
		{
			Console.WriteLine("done");
			_actions.RemoveAll(a => a.Undone);
			_actions.Add(new UndoRedoAction(undo, redo));
		}

		public void Undo()
		{
			var action = _actions.LastOrDefault(a => !a.Undone);

			if (action == null)
				return;

			Console.WriteLine("undone");

			action.Undo?.Invoke();
			action.Undone = true;
		}

		public void Redo()
		{
			var action = _actions.FirstOrDefault(a => a.Undone);

			if (action == null)
				return;

			Console.WriteLine("redone");

			action.Redo?.Invoke();
			action.Undone = false;
		}
	}
}