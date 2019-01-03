using System;

namespace Blox_Saber_Editor
{
	class UndoRedoAction
	{
		public Action Undo;
		public Action Redo;

		public bool Undone;

		public UndoRedoAction(Action undo, Action redo)
		{
			Undo = undo;
			Redo = redo;
		}
	}
}