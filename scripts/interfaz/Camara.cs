using Godot;
using System;

public partial class Camara : Camera2D
{
    [Export] public float Velocidad = 500.0f; // Velocidad del movimiento
    [Export] public float LimiteIzquierdo = 0.0f; // Posición mínima en X
    [Export] public float LimiteDerecho = 2000.0f; // Posición máxima en X (ajusta según tu mapa)

    public override void _Process(double delta)
    {
        Vector2 posicion = Position;

        // Movimiento con las flechas o teclas A/D
        float entrada = Input.GetAxis("ui_left", "ui_right");
        
        posicion.X += entrada * Velocidad * (float)delta;

        // Restringimos el movimiento para que no se salga del mapa
        posicion.X = Mathf.Clamp(posicion.X, LimiteIzquierdo, LimiteDerecho);

        Position = posicion;
    }
}
