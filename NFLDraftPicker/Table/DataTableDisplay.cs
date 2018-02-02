using System;
using System.Data;
using System.Linq;
using System.Text;

namespace NFLDraftPicker.Table {
	public class DataTableDisplay {
		private const ConsoleColor DEFAULT_BACKGROUND_COLOR = (ConsoleColor) (-1);
		private const ConsoleColor DEFAULT_FOREGROUND_COLOR = ConsoleColor.Black; //ConsoleColor.White;

		private readonly DataTable _table;

		//Settings
		public int Padding;
		public bool UseEvenCellWidths;
		public bool UseEvenCellHeights;

		public DataTableStyle TableStyle = DataTableStyles.SinglePipes;

		public DataTableDisplay(DataTable table) {
			_table = table;
			if(_table.Columns[0].DataType != typeof(object)) //Only accepts Columns that accept typeof(object)
				throw new ArgumentException();
		}

		private int _vertialCells;
		private int _horizantalCells;
		private int[] _columnWidths;
		private int[] _rowHeights;

		public void Draw() {
			_vertialCells = _table.Rows.Count + 1;
			_horizantalCells = _table.Columns.Count;
			_columnWidths = new int[_horizantalCells];
			_rowHeights = new int[_vertialCells];

			for(int y = 0; y < _vertialCells; y++) {
				int maxCellHeight = 0;
				for(int x = 0; x < _horizantalCells; x++) {
					object value = y == 0 ? _table.Columns[x] : _table.Rows[y - 1][x];
					if(y != 0)
						_table.Rows[y - 1][x] = new CustomCell(value);
					string[] lines = value.ToString().Split('\n');
					if(lines.Max(str => str.Length) > _columnWidths[x]) {
						_columnWidths[x] = lines.Max(str => str.Length);
					}

					if(lines.Length > maxCellHeight) {
						maxCellHeight = lines.Length;
					}
				}

				_rowHeights[y] = maxCellHeight;
			}

			if(Padding != 0)
				_columnWidths = _columnWidths.Select(c => {
						c += Padding;
						return c;
					})
					.ToArray();

			if(UseEvenCellHeights)
				_rowHeights = Enumerable.Repeat(_rowHeights.Max(), _vertialCells).ToArray();
			if(UseEvenCellWidths)
				_columnWidths = Enumerable.Repeat(_columnWidths.Max(), _horizantalCells).ToArray();

			string[,][] cells = new string[_horizantalCells, _vertialCells][];

			for(int x = 0; x < _horizantalCells; x++) {
				for(int y = 0; y < _vertialCells; y++) {
					cells[x, y] = y == 0 ? new[] {_table.Columns[x].ColumnName} : ((CustomCell) _table.Rows[y - 1][x]).Value.ToString().Split('\n');
				}
			}

			StringBuilder builder = new StringBuilder();

			int neededHeight = 1;

			HorizontalLine(TableStyle.UpperLeftCorner, TableStyle.UpperBorder, TableStyle.UpperSplit, TableStyle.UpperRightCorner);
			for(int y = 0; y < _vertialCells; y++) {
				for(int line = 0; line < _rowHeights[y]; line++) {
					Text(y, line, TableStyle.LeftBorder, TableStyle.Split, TableStyle.RightBorder);
					neededHeight++;
				}

				if(y != _vertialCells - 1)
					HorizontalLine(TableStyle.LeftSplit, TableStyle.MiddleBorder, TableStyle.MiddleSplit, TableStyle.RightSplit);

				neededHeight++;
			}

			HorizontalLine(TableStyle.LowerLeftCorner, TableStyle.LowerBorder, TableStyle.LowerSplit, TableStyle.LowerRightCorner);

			Console.Clear();

			int neededWidth = 2;

			for(int x = 0; x < _horizantalCells; x++) {
				neededWidth += _columnWidths[x];
				if(x != _horizantalCells - 1)
					neededWidth++;
			}

			while(Console.WindowWidth < neededWidth || Console.WindowHeight < neededHeight) {
				Console.Beep();
				Console.WriteLine(
					$"Console Size must be at least {neededWidth}x{neededHeight} (width, height), but currently is {Console.WindowWidth}x{Console.WindowHeight}.");
				Console.WriteLine("Press Enter once you have resized the window.");
				Console.ReadLine();
				Console.Clear();
			}

			builder.Length--;
			Console.BackgroundColor = DEFAULT_BACKGROUND_COLOR;
			Console.ForegroundColor = DEFAULT_FOREGROUND_COLOR;
			_drawn = builder.ToString();
			Console.Write(_drawn);

			void Text(int y, int line, char left, char center, char right) {
				builder.Append(left);
				for(int x = 0; x < _horizantalCells; x++) { //155,35
					string[] lines = (y == 0 ? new[] {_table.Columns[x].ColumnName} : ((CustomCell) _table.Rows[y - 1][x]).Value.ToString().Split('\n'))
						.PadVerticalCenter(_rowHeights[y]);
					builder.Append((lines.Length - 1 >= line ? lines[line] : "").PadCenter(_columnWidths[x]));
					if(x != _horizantalCells - 1)
						builder.Append(center);
				}

				builder.Append(right + "\n");
			}

			void HorizontalLine(char left, char spacing, char center, char right) {
				builder.Append(left);
				for(int x = 0; x < _horizantalCells; x++) {
					builder.Append(spacing, _columnWidths[x]);
					if(x != _horizantalCells - 1)
						builder.Append(center);
				}

				builder.Append(right + "\n");
			}
		}

		private string _drawn;

		public void ReDraw() {
			if(_drawn == null) {
				Draw();
				return;
			}

			Console.Clear();
			Console.Write(_drawn);
		}

		public (int x, int y) GetCellDrawnPosition(int x, int y) {
			(int x, int y) pos = (0, 0); //Reversed
			for(int x2 = 0; x2 < x; x2++) {
				pos.x += _columnWidths[x2];
			}

			pos.x += 1 + x;
			pos.y += y - 1;

			for(int y2 = 0; y2 <= y; y2++) {
				pos.y += _rowHeights[y2];
			}

			return pos;
		}

		public void RefreshCell(int x, int y) {
			var pos = GetCellDrawnPosition(x, y);
			CustomCell cell = (CustomCell) _table.Rows[y - 1][x];
			string[] values = cell.Value.ToString().Split('\n').PadVerticalCenter(_rowHeights[y]);
			for(int y2 = 0; y2 < values.Length; y2++) {
				Console.SetCursorPosition(pos.x, pos.y + y2 - 1);
				Console.BackgroundColor = cell.BackgroundColor ?? DEFAULT_BACKGROUND_COLOR;
				Console.ForegroundColor = cell.TextColor ?? DEFAULT_FOREGROUND_COLOR;
				Console.Write(values[y2].PadCenter(_columnWidths[x]));
				Console.BackgroundColor = DEFAULT_BACKGROUND_COLOR;
				Console.ForegroundColor = DEFAULT_FOREGROUND_COLOR;
				Console.SetCursorPosition(0, 0);
			}
		}

		public void EditCellAndColor(int x, int y, object obj, ConsoleColor? background = null, ConsoleColor? foreground = null) {
			ChangeCellColor(x, y, background, foreground);
			EditCell(x, y, obj);
		}

		public void EditCell(int x, int y, object obj) {
			((CustomCell) _table.Rows[y - 1][x]).Value = obj;
			RefreshCell(x, y);
		}

		public void ChangeCellColor(int x, int y, ConsoleColor? background = null, ConsoleColor? foreground = null) {
			if(y == 0)
				throw new ArgumentException();
			CustomCell cell = (CustomCell) _table.Rows[y - 1][x];
			cell.BackgroundColor = background;
			cell.TextColor = foreground;
		}

		public void ClearCell(int x, int y) {
			ChangeCellColor(x, y, DEFAULT_BACKGROUND_COLOR, DEFAULT_FOREGROUND_COLOR);
			EditCell(x, y, "");
		}
	}
}