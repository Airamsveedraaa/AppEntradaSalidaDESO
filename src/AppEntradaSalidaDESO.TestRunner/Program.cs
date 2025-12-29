using System;
using System.Collections.Generic;
using AppEntradaSalidaDESO.Services;
using AppEntradaSalidaDESO.Algorithms;

Console.WriteLine("=============================================");
Console.WriteLine(" VALIDACIÓN DE ALGORITMOS DE PLANIFICACIÓN");
Console.WriteLine("=============================================");
Console.WriteLine();

var service = new AlgorithmService();
var algorithms = service.GetAlgorithmNames();

// Datos de prueba comunes
int initialPos = 50;
var requests = new List<int> { 82, 170, 43, 140, 24, 16, 190 };
int min = 0;
int max = 199;
string direction = "up";

Console.WriteLine($"Datos de Prueba:");
Console.WriteLine($"  Posición Inicial: {initialPos}");
Console.WriteLine($"  Cola: {string.Join(", ", requests)}");
Console.WriteLine($"  Límites: {min}-{max}");
Console.WriteLine($"  Dirección: {direction}");
Console.WriteLine("---------------------------------------------");
Console.WriteLine();

bool allPassed = true;

foreach (var algName in algorithms)
{
    Console.Write($"Probando [{algName}]... ");
    
    try
    {
        var alg = service.GetAlgorithm(algName);
        if (alg == null)
        {
            Console.WriteLine("FAIL (No encontrado)");
            allPassed = false;
            continue;
        }

        var result = alg.Execute(initialPos, new List<int>(requests), min, max, direction);

        if (result != null && result.TotalHeadMovement > 0 && result.ProcessingOrder.Count >= requests.Count)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PASS");
            Console.ResetColor();
            Console.WriteLine($"   Movement: {result.TotalHeadMovement}");
            Console.WriteLine($"   Order: {string.Join(" -> ", result.ProcessingOrder)}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAIL (Resultados inválidos)");
            Console.ResetColor();
            allPassed = false;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"FAIL (Excepción: {ex.Message})");
        Console.ResetColor();
        allPassed = false;
    }
    Console.WriteLine();
}

Console.WriteLine("=============================================");
if (allPassed)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(" TODOS LOS TESTS PASARON CORRECTAMENTE");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(" ALGUNOS TESTS FALLARON");
}
Console.ResetColor();
Console.WriteLine("=============================================");
