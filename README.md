# Sistema de Gestión de Producción ERP
Programa en C# que permita simular los datos generados por el ERP (Orden de producción, cantidad, fecha, código de material y descripción del material): solo la inserción de órdenes de producción La generación de los lotes se debe realizar con al azar: el código del lote

## Requisitos
- Plataforma: Se requiere tener instalado el entorno de desarrollo de .NET.
- Base de Datos: Se utiliza Microsoft SQL Server para almacenar los datos. Se necesita tener un servidor SQL disponible para conectar la aplicación.

## Configuración
- Base de Datos: El programa requiere una base de datos llamada DulcesMorita. Asegúrate de tener un servidor SQL disponible y modificar la cadena de conexión en el archivo Program.cs si es necesario.
```csharp
string connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";
string databaseName = "DulcesMorita";
```
## Ejecución
Compila el programa y ejecuta la aplicación. Esto creará la base de datos si no existe y configurará las tablas necesarias.

## Uso
El programa automatiza el proceso de gestión de órdenes de producción. Algunas funciones clave incluyen:

- Verificar y crear la base de datos y las tablas necesarias.
- Insertar órdenes de producción y dividirlas en lotes para su seguimiento.
- Registrar la producción realizada basada en los lotes creados.

## Autor
Este programa fue desarrollado por Juan José Portilla Rodriguez
