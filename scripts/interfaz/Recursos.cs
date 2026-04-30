using Godot;
using System;

public partial class Recursos : Node
{
	public static Recursos Instance;

	public int Energia = 0;
	public int EnergiaMejorada = 0;
	public int Health = 100;
	public int VidaEnemigo = 100;

	public override void _Ready()
	{
		Instance = this;
		SubidaEnergia();
	}

	public void SubidaEnergia()
	{
		SumarEnergia();
	}

	public void SumarEnergia()
	{
		Energia += 1 + EnergiaMejorada;
	}

	public void MejorarEnergia()
	{
		EnergiaMejorada += 1;
	}

    public void DanarJugador(int cantidad)
    {
        Health -= cantidad;
        if (Health < 0) Health = 0;
    }

    public void DanarEnemigo(int cantidad)
    {
        VidaEnemigo -= cantidad;
        if (VidaEnemigo < 0) VidaEnemigo = 0;
    }

	public void ReiniciarEnergia()
	{
		Energia = 0;
		EnergiaMejorada = 0;
		Health = 100;
		VidaEnemigo = 100;
	}
}
