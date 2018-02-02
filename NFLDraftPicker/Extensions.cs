using System;
using System.Linq;

namespace NFLDraftPicker {
	public static class Extensions {
		public static string PadCenter(this string str, int length) { // function "PadCenter" is from https://stackoverflow.com/a/40061134/3485939
			int padAmount = length - str.Length;

			if(padAmount <= 1)
				return padAmount == 1 ? str.PadRight(length) : str;

			int padLeft = padAmount / 2 + str.Length;

			return str.PadLeft(padLeft).PadRight(length);
		}

		public static string[] PadVerticalCenter(this string[] array, int legnth) {
			if(array.Length > legnth)
				throw new ArgumentException();
			array = array.Where(str => !string.IsNullOrEmpty(str)).ToArray();
			if(array.Length >= legnth) return array;
			string[] temp = Enumerable.Repeat(string.Empty, legnth).ToArray();

			for(int i = 0; i < array.Length; i++) {
				temp[legnth / 2 + i - array.Length / 2] = array[i];
			}

			return temp;
		}
	}
}