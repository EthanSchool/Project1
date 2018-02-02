using System;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NFLDraftPicker.Table;

namespace NFLDraftPicker {
	internal class Program {
		private const ConsoleColor BACKGROUND_COLOR = (ConsoleColor) (-1);
		private const ConsoleColor SELECTED_COLOR = ConsoleColor.DarkYellow;
		private const ConsoleColor CURSOR_COLOR = ConsoleColor.Blue;

		private const uint MAX_COST = 95_000_000;
		private const uint MAX_EFFECTIVE_COST = 65_000_000;

		private static void Main() {
			new Program().Run();
		}

		private readonly Player[,] _playerArray; //Y,X

		private bool[,] _selectedCells; //X,Y

		private readonly DataTableDisplay _display;
		private readonly int _height;
		private readonly int _width;

		private uint _totalCost;

		private Program() { //Make sure to have a large enough console window for this to display properly
			Console.CursorVisible = false;
			DataTable table = new DataTable();
			JObject jObject = JObject.Parse(File.ReadAllText("players.json"));
			JArray temp = jObject["values"].ToObject<JArray>();

			_playerArray = temp.ToObject<Player[,]>();

			string[] yLabels = jObject["lables"]["y"].ToObject<string[]>();
			string[] xLabels = jObject["lables"]["x"].ToObject<string[]>();
			foreach(string label in xLabels) {
				table.Columns.Add(label, typeof(object));
			}

			_height = yLabels.Length + 1;
			_width = xLabels.Length;

			for(int y = 0; y < _height - 1; y++) {
				table.Rows.Add(yLabels[y], _playerArray[y, 0], _playerArray[y, 1], _playerArray[y, 2], _playerArray[y, 3], _playerArray[y, 4]);
			}

			_display = new DataTableDisplay(table) {
				TableStyle = DataTableStyles.DoublePipes,
				Padding = 2,
				UseEvenCellWidths = true
			};
		}

		private void Run() {
			Exit exitProgram = Exit.None;
			while(exitProgram != Exit.Program) {
				_display.ReDraw();
				_selectedCells = new bool[_width - 1, _height - 1];
				_cursorPositon = new IntVector2(1, 1);
				_display.ChangeCellColor(_cursorPositon.X, _cursorPositon.Y, CURSOR_COLOR);
				_display.RefreshCell(_cursorPositon.X, _cursorPositon.Y);

				_totalCost = 0;
				_display.EditCellAndColor(_width - 1, 1, $"Total:\n${0:N0}");
				_display.EditCellAndColor(_width - 1, 2, $"Remaining:\n${MAX_COST:N0}");
				_display.EditCellAndColor(_width - 1, 5, "Instructions:\nArrow keys to move\nEnter for selection");
				_display.EditCellAndColor(_width - 1, 7, "Restart");
				_display.EditCellAndColor(_width - 1, 8, "Exit");

				exitProgram = Exit.None;

				while(exitProgram == Exit.None) {
					exitProgram = ProcessSelection(PromptSelection());
				}
			}

			Console.Clear();
			Console.CursorVisible = true;
		}

		private enum Exit {
			Selection,
			Program,
			None
		}

		private Exit ProcessSelection(IntVector2 pos) {
			bool selected = _selectedCells[pos.X - 1, pos.Y - 1];

			if(pos.X == 6) {
				switch(pos.Y) {
					case 7:
						return Exit.Selection;
					case 8:
						return Exit.Program;
				}
			}

			if(!selected && (from bool val in _selectedCells where val select val).Count() >= 5) {
				UpdateErrorBox("Limit of 5\ndraft prospects");
				return Exit.None;
			}

			CalcTotal(pos, selected);

			return Exit.None;
		}

		private void UpdateErrorBox(string message) {
			if(message == null) {
				_display.ClearCell(_width - 1, 4);
				return;
			}

			_display.EditCellAndColor(_width - 1, 4, "Error:\n" + message, ConsoleColor.DarkRed);
			Console.Beep();
		}

		private void CalcTotal(IntVector2 pos, bool remove) {
			Player player = _playerArray[pos.Y - 1, pos.X - 1];
			if(!remove && _totalCost + player.Cost > MAX_COST) {
				UpdateErrorBox("Cannot exceed 95\nmillion dollars");
				return;
			}

			_selectedCells[pos.X - 1, pos.Y - 1] = !remove;
			if(remove) {
				_totalCost -= player.Cost;
			} else {
				_totalCost += player.Cost;
			}

			_display.EditCell(_width - 1, 1, $"Total:\n${_totalCost:N0}");
			_display.EditCell(_width - 1, 2, $"Remaining:\n${MAX_COST - _totalCost:N0}");

			int count = 0;
			for(int y = 0; y < _height-1; y++) {
				for(int x = 0; x < 4; x++) {
					if(_selectedCells[x,y])
						count++;
				}
			}
			
			if(_totalCost < MAX_EFFECTIVE_COST && count >= 3) {
				_display.EditCellAndColor(_width - 1, 3, "Cost Effective", ConsoleColor.DarkGreen);
			} else {
				_display.ClearCell(_width - 1, 3);
			}
		}

		private IntVector2 PromptSelection() {
			while(true) {
				ConsoleKeyInfo input = Console.ReadKey(true);
				UpdateErrorBox(null);
				if(input.Key == ConsoleKey.Enter)
					return _cursorPositon;
				HandleKeyInput(input);
			}
		}

		private IntVector2 _cursorPositon;

		private void HandleKeyInput(ConsoleKeyInfo input) {
			IntVector2 newCursorPosition = _cursorPositon.Clone();
			switch(input.Key) {
				case ConsoleKey.UpArrow:
					newCursorPosition.Y -= 1;
					break;
				case ConsoleKey.DownArrow:
					newCursorPosition.Y += 1;
					break;
				case ConsoleKey.LeftArrow:
					newCursorPosition.X -= 1;
					break;
				case ConsoleKey.RightArrow:
					newCursorPosition.X += 1;
					break;
				default:
					Console.Beep();
					break;
			}

			if(!CheckBounds(newCursorPosition)) {
				Console.Beep();
				return;
			}

			_display.ChangeCellColor(_cursorPositon.X, _cursorPositon.Y,
				_selectedCells[_cursorPositon.X - 1, _cursorPositon.Y - 1] ? SELECTED_COLOR : BACKGROUND_COLOR);
			_display.ChangeCellColor(newCursorPosition.X, newCursorPosition.Y, CURSOR_COLOR);
			_display.RefreshCell(_cursorPositon.X, _cursorPositon.Y);
			_display.RefreshCell(newCursorPosition.X, newCursorPosition.Y);

			_cursorPositon = newCursorPosition.Clone();
		}

		private bool CheckBounds(IntVector2 pos) {
			return pos.Y >= 1 && pos.Y < _height && pos.X >= 1 && pos.X < _width - 1 || pos.X == 6 && (pos.Y == 7 || pos.Y == 8);
		}
	}

	internal class IntVector2 {
		public int X;
		public int Y;

		public IntVector2(int x, int y) {
			X = x;
			Y = y;
		}

		public IntVector2 Clone() {
			return new IntVector2(X, Y);
		}
	}
}