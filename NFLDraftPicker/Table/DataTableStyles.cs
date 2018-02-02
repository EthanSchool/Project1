namespace NFLDraftPicker.Table {
	public static class DataTableStyles {
		public static readonly DataTableStyle DoublePipes = new DataTableStyle {
			UpperLeftCorner = '╔',
			UpperBorder = '═',
			UpperSplit = '╦',
			UpperRightCorner = '╗',

			LeftSplit = '╠',

			MiddleBorder = '═',
			MiddleSplit = '╬',

			RightSplit = '╣',

			LowerLeftCorner = '╚',
			LowerBorder = '═',
			LowerSplit = '╩',
			LowerRightCorner = '╝',

			LeftBorder = '║',
			Split = '║',
			RightBorder = '║'
		};

		public static readonly DataTableStyle SinglePipes = new DataTableStyle {
			UpperLeftCorner = '┌',
			UpperBorder = '─',
			UpperSplit = '┬',
			UpperRightCorner = '┐',

			LeftSplit = '├',

			MiddleBorder = '─',
			MiddleSplit = '┼',

			RightSplit = '┤',

			LowerLeftCorner = '└',
			LowerBorder = '─',
			LowerSplit = '┴',
			LowerRightCorner = '┘',

			LeftBorder = '│',
			Split = '│',
			RightBorder = '│'
		};
	}
}