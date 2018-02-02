using System;

namespace NFLDraftPicker.Table {
	public class CustomCell {
		public object Value;
		public ConsoleColor? BackgroundColor;
		public ConsoleColor? TextColor;

		public CustomCell(object value, ConsoleColor? backgroundColor = null, ConsoleColor? textColor = null) {
			Value = value;
			BackgroundColor = backgroundColor;
			TextColor = textColor;
		}
	}
}