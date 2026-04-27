using Godot;
using System;

public partial class Recursos : Node
{
	public static Recursos Instance;

	public int Energia = 0;
	public int EnergiaMejorada = 0;
	public int Health = 100;

	public override void _Ready()
	{
		Instance = this;
		SubidaEnergia();
	}

	private async void SubidaEnergia()
	{
		SumarEnergia();
		await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
	}

	public void SumarEnergia()
	{
		Energia += 10 + EnergiaMejorada;
	}

	public void MejorarEnergia()
	{
		EnergiaMejorada += 10;
	}

	public void ReiniciarEnergia()
	{
		Energia = 0;
		EnergiaMejorada = 0;
		Health = 100;
	}

	/** Ligero --> Artillero
	 * Penetrador --> Tanque
	 * Tanque --> Ligero
	 * Artillero --> Penetrador
	 */
}
