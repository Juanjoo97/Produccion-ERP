using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
namespace ERP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Definir las órdenes de producción
            var ordenesProduccion = new[]
{
                new OrdenProduccion {Referencia = "001", Cantidad = 1000},
                new OrdenProduccion {Referencia = "002", Cantidad = 300},
                new OrdenProduccion {Referencia = "003", Cantidad = 200},
                new OrdenProduccion {Referencia = "004", Cantidad = 2000},
                new OrdenProduccion {Referencia = "005", Cantidad = 800},
            };
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";
            string databaseName = "DulcesMorita";
            // Verificar si la base de datos existe o crearla si no
            if (VerificarCrearBaseDatos(connectionString, databaseName))
            {
                // Verificar Tabla de Ordenes de produccion
                VerificarCrearTablaOrdenesProduccion(connectionString, ordenesProduccion);
            }
            else
            {
                Console.WriteLine("Error en la conexion con la base de datos");
            }
        }

        static bool VerificarCrearBaseDatos(string connectionString, string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Verificar si la base de datos ya existe
                    SqlCommand checkDatabaseCommand = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", connection);
                    int databaseCount = (int)checkDatabaseCommand.ExecuteScalar();
                    if (databaseCount > 0)
                    {
                        Console.WriteLine($"La base de datos '{databaseName}' ya existe.");
                        Console.WriteLine();
                        connection.Close();
                    }
                    else
                    {
                        // Crear la base de datos
                        SqlCommand createDatabaseCommand = new SqlCommand($"CREATE DATABASE {databaseName}", connection);
                        createDatabaseCommand.ExecuteNonQuery();
                        Console.WriteLine("Base de datos creada exitosamente.");
                        Console.WriteLine();
                        connection.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al crear o verificar la base de datos: " + ex.Message);
                    return false;
                }
            }
        }
        static bool VerificarCrearTablaOrdenesProduccion(string connectionString, OrdenProduccion[] ordenesProduccion)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Cambiar a la base de datos DulcesMorita
                    SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                    useDatabaseCommand.ExecuteNonQuery();
                    // Verificar si la tabla OrdenesProduccion existe
                    SqlCommand checkTableCommand = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesProduccion '", connection);
                    int tableCount = (int)checkTableCommand.ExecuteScalar();
                    if (tableCount > 0)
                    {
                        Console.WriteLine("Tabla OrdenesProduccion existente.");
                        Console.WriteLine();
                        foreach (var orden in ordenesProduccion)
                        {
                            SqlCommand command = new SqlCommand($"UPDATE OrdenesProduccion SET Cantidad = '{orden.Cantidad}' WHERE Referencia = '{orden.Referencia}'", connection);
                            command.ExecuteNonQuery();
                        }
                        Console.WriteLine("Tabla 'OrdenesProduccion' modificado exitosamente.");
                        Console.WriteLine();
                        connection.Close();
                        VerificarCrearTablaLoteProduccion(connectionString, ordenesProduccion);
                    }
                    else
                    {
                        // Crear la tabla OrdenesProduccion
                        SqlCommand createTableCommand = new SqlCommand("CREATE TABLE OrdenesProduccion  (Id INT PRIMARY KEY IDENTITY, Referencia INT NOT NULL,Cantidad INT NOT NULL)", connection);
                        createTableCommand.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'OrdenesProduccion' creada exitosamente.");
                        Console.WriteLine();
                        connection.Close();
                        InsertarOrdenesProduccion(connectionString, ordenesProduccion);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al crear o verificar la tabla OrdenesProduccion: " + ex.Message);
                    return false;
                }
            }
        }
    static void InsertarOrdenesProduccion(string connectionString, OrdenProduccion[] ordenesProduccion)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    foreach (var orden in ordenesProduccion)
                    {
                        connection.Open();
                        // Cambiar a la base de datos DulcesMorita
                        SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                        useDatabaseCommand.ExecuteNonQuery();
                        // Verificar si la tabla OrdenesProduccion existe
                        SqlCommand command = new SqlCommand($"INSERT INTO OrdenesProduccion (Referencia, Cantidad) VALUES ('{orden.Referencia}', '{orden.Cantidad}')", connection);
                        command.ExecuteNonQuery();
                        // Obtener el ID de la orden de producción recién insertada
                        SqlCommand commandId = new SqlCommand("SELECT SCOPE_IDENTITY()", connection);
                        int ordenId = Convert.ToInt32(commandId.ExecuteScalar());
                        // Dividir la orden de producción en lotes y insertar en la base de datos
                        connection.Close();
                    }
                    Console.WriteLine("Órdenes de producción insertadas correctamente.");
                    Console.WriteLine();
                    VerificarCrearTablaLoteProduccion(connectionString, ordenesProduccion);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar los datos de la tabla OrdenesProduccion: " + ex.Message);
                }

            }
        }
        static bool VerificarCrearTablaLoteProduccion(string connectionString, OrdenProduccion[] ordenesProduccion)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Cambiar a la base de datos DulcesMorita
                    SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                    useDatabaseCommand.ExecuteNonQuery();
                    // Verificar si la tabla LoteProudccion existe
                    SqlCommand checkTableCommand = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoteProduccion'", connection);
                    int tableCount = (int)checkTableCommand.ExecuteScalar();
                    if (tableCount > 0)
                    {
                        Console.WriteLine("Tabla LoteProduccion existente.");
                        Console.WriteLine();
                        var transaction = connection.BeginTransaction();
                        try
                        {
                            // Deshabilitar la restricción de clave externa temporalmente
                            SqlCommand disableConstraintCmd = new SqlCommand(
                                "ALTER TABLE dbo.Produccion NOCHECK CONSTRAINT FK__Produccio__Refer__29572725", connection, transaction);
                            disableConstraintCmd.ExecuteNonQuery();
                            // Eliminar registros en LoteProduccion
                            SqlCommand deleteRecordsCmd = new SqlCommand("DELETE FROM LoteProduccion", connection, transaction);
                            deleteRecordsCmd.ExecuteNonQuery();
                            SqlCommand commandResetId = new SqlCommand("DBCC CHECKIDENT ('LoteProduccion', RESEED, 0)", connection, transaction);
                            commandResetId.ExecuteNonQuery();
                            // Habilitar la restricción de clave externa nuevamente
                            SqlCommand enableConstraintCmd = new SqlCommand(
                                "ALTER TABLE dbo.Produccion CHECK CONSTRAINT FK__Produccio__Refer__29572725", connection, transaction);
                            enableConstraintCmd.ExecuteNonQuery();
                            // Confirmar la transacción si todo fue exitoso
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Revertir la transacción si hay algún error
                            transaction.Rollback();
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        Console.WriteLine("Tabla LoteProduccion formateada.");
                        Console.WriteLine();
                        Random random = new Random();
                        foreach (var orden in ordenesProduccion)
                        {
                            int numeroAleatorio = random.Next(1, 6);
                            for (int i = 0; i < numeroAleatorio; i++)
                            {
                                int cantidadDividida = orden.Cantidad / numeroAleatorio;
                                string fechaParaSQL = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Console.WriteLine($"Referencia: {orden.Referencia}, Cantidad: {orden.Cantidad}, Número Aleatorio: {numeroAleatorio}, Cantidad Dividida: {cantidadDividida}");
                                Console.WriteLine();
                                SqlCommand command = new SqlCommand($"INSERT INTO LoteProduccion  (Referencia, Cantidad, FechaCreacion) VALUES ('{orden.Referencia}', '{cantidadDividida}', '{fechaParaSQL}')", connection);
                                command.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine("Tabla 'LoteProduccion' modificado exitosamente.");
                        Console.WriteLine();
                        connection.Close();
                        VerificarCrearTablaProduccion(connectionString);
                    }
                    else
                    {
                        // Crear la tabla OrdenesProduccion
                        SqlCommand createTableCommand = new SqlCommand("CREATE TABLE LoteProduccion  (Id INT PRIMARY KEY IDENTITY, Referencia INT NOT NULL,Cantidad INT NOT NULL,FechaCreacion DATETIME NOT NULL, FOREIGN KEY (Referencia) REFERENCES OrdenesProduccion(Id))", connection);
                        createTableCommand.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'LoteProduccion' creada exitosamente.");
                        Console.WriteLine();
                        connection.Close();
                        InsertarLoteProduccion(connectionString, ordenesProduccion);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al crear o verificar la tabla LoteProducto: " + ex.Message);
                    Console.ReadLine();
                    return false;
                }
            }
        }
        static void InsertarLoteProduccion(string connectionString, OrdenProduccion[] ordenesProduccion)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    Random random = new Random();
                    foreach (var orden in ordenesProduccion)
                    {
                        // Crear un diccionario para almacenar los objetos Random por referencia
                        string fechaParaSQL = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        // Insertar la orden de producción en la base de datos
                        connection.Open();
                        // Cambiar a la base de datos DulcesMorita
                        SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                        useDatabaseCommand.ExecuteNonQuery();
                        // Generar un número aleatorio entre 1 y 5
                        int numeroAleatorio = random.Next(1, 6); // Entre 1 y 5, puedes ajustar este rango según tu necesidad
                        // Dividir la cantidad por el número aleatorio y mostrar el resultado
                        for (int i = 0; i < numeroAleatorio; i++)
                        {
                        int cantidadDividida = orden.Cantidad / numeroAleatorio;
                            SqlCommand commandLote = new SqlCommand($"INSERT INTO LoteProduccion  (Referencia, Cantidad, FechaCreacion) VALUES ('{orden.Referencia}', '{cantidadDividida}', '{fechaParaSQL}')", connection);
                            commandLote.ExecuteNonQuery();
                            Console.WriteLine($"Referencia: {orden.Referencia}, Cantidad: {orden.Cantidad}, Número Aleatorio: {numeroAleatorio}, Cantidad Dividida: {cantidadDividida}");
                            Console.WriteLine();
                        }
                        connection.Close(); // Cerrar la conexión después de usarla
                        
                    }
                    Console.WriteLine("Lote de producción insertadas correctamente.");
                    Console.WriteLine();
                    VerificarCrearTablaProduccion(connectionString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar los datos de la tabla LoteProduccion: " + ex.Message);
                    Console.WriteLine();
                }
            }
        }
        static bool VerificarCrearTablaProduccion(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Cambiar a la base de datos DulcesMorita
                    SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                    useDatabaseCommand.ExecuteNonQuery();
                    // Verificar si la tabla LoteProudccion existe
                    SqlCommand checkTableCommand = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Produccion'", connection);
                    int tableCount = (int)checkTableCommand.ExecuteScalar();
                    if (tableCount > 0)
                    {
                        Console.WriteLine("Tabla Produccion existente.");
                        Console.WriteLine();
                        Random random = new Random();
                        // Crear un diccionario para almacenar los objetos Random por referencia
                        string fechaParaSQL = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        // Insertar la orden de producción en la base de datos
                        SqlCommand commandLote = new SqlCommand($"INSERT INTO Produccion (Referencia, CantidadMalas, FechaHoraInicio, FechaHoraFin) SELECT Referencia, Cantidad, FechaCreacion, FechaCreacion FROM LoteProduccion;", connection);
                        commandLote.ExecuteNonQuery();
                        Console.WriteLine("Producción insertadas correctamente.");
                        Console.WriteLine();
                        connection.Close(); // Cerrar la conexión después de usarla
                    }
                    else
                    {
                        // Crear la tabla Produccion
                        SqlCommand createTableCommand = new SqlCommand("CREATE TABLE Produccion (Id INT IDENTITY(1,1) PRIMARY KEY, Referencia INT NOT NULL, Operario NVARCHAR(MAX) NULL, CantidadBuenas INT NULL, CantidadMalas INT NOT NULL, FechaHoraInicio DATETIME2 NOT NULL, FechaHoraFin DATETIME2 NOT NULL,GastosAdicionales INT NULL, Observacion NVARCHAR(MAX) NULL, FOREIGN KEY(Referencia) REFERENCES LoteProduccion(Id));", connection);
                        createTableCommand.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'Produccion' creada exitosamente.");
                        connection.Close();
                        //InsertarLoteProduccion(connectionString, ordenesProduccion);
                        InsertarProduccion(connectionString);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al crear o verificar la tabla Produccion: " + ex.Message);
                    Console.ReadLine();
                    return false;
                }
            }
        }
        static void InsertarProduccion(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    Random random = new Random();
                    // Crear un diccionario para almacenar los objetos Random por referencia
                    string fechaParaSQL = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    // Insertar la orden de producción en la base de datos
                    connection.Open();
                    // Cambiar a la base de datos DulcesMorita
                    SqlCommand useDatabaseCommand = new SqlCommand("USE DulcesMorita;", connection);
                    useDatabaseCommand.ExecuteNonQuery();
                    // Dividir la cantidad por el número aleatorio y mostrar el resultado
                    SqlCommand commandLote = new SqlCommand($"INSERT INTO Produccion (Referencia, CantidadMalas, FechaHoraInicio, FechaHoraFin) SELECT Referencia, Cantidad, FechaCreacion, FechaCreacion FROM LoteProduccion;", connection);
                    commandLote.ExecuteNonQuery();
                    Console.WriteLine();
                    Console.WriteLine("Producción insertadas correctamente.");
                    Console.WriteLine();
                    connection.Close(); // Cerrar la conexión después de usarla
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar los datos de la tabla Produccion: " + ex.Message);
                    Console.WriteLine();
                }
            }
        }
    }

    class OrdenProduccion
    {
        public string Referencia { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }
}