using Godot;
using System;
public partial class RobotTanque : Robot 
{
    public override void _Ready()
    {
        base._Ready(); // Ejecuta lo que hay en la clase base
        // El tanque es lento pero tiene mucha vida, 
        // estos valores los puedes cambiar en el Inspector de Godot
    }
    
    // Aquí puedes añadir lógica ÚNICA del tanque si quieres
}
