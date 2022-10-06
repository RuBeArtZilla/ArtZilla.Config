namespace ArtZilla.Net.Config.Tests.Generators; 

public struct Hero {
	public string Name;
	public int Power;

	public Hero(string name, int power = 0) {
		Name = name;
		Power = power;
	}
}