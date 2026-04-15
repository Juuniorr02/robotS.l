using Godot;
using System;

// Hereda de Robot en lugar de CharacterBody2D
public partial class RobotPenetrador : Robot 
{
    public override void _Ready()
    {
        base._Ready(); // Ejecuta lo que hay en la clase base
        // El penetrador es rápido y tiene mucho daño pero tiene poca vida, 
        // estos valores los puedes cambiar en el Inspector de Godot
    }
    
    // Aquí puedes añadir lógica ÚNICA del penetrador si quieres
}

