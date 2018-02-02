namespace NFLDraftPicker {
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Player {
		public string Name;
		public string From;
		public uint Cost;

		public override string ToString() {
			return $"{Name}\n{From}\n${Cost:N0}";
		}
	}
}