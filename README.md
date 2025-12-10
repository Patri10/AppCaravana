# AppCaravana - Aplicación de Gestión de Caravanas

## Descripción
AppCaravana es una aplicación WPF desarrollada en C# .NET 10.0 para la gestión integral de un negocio de caravanas, incluyendo clientes, caravanas, ventas e informes con gráficos generados por Python.

## Características

### 1. Gestión de Clientes
- Crear, editar y eliminar clientes
- Guardar información: Nombre, Apellido, DNI, Teléfono, Email
- Formularios con interfaz mejorada y campos más grandes

### 2. Gestión de Caravanas
- Registrar nuevas caravanas
- Información detallada: Serie, Marca, Modelo, Año, Matrícula, Número SENASA, Tipo, Precio
- Control de disponibilidad
- Características y descripción ampliada
- Campos de entrada optimizados para fácil lectura

### 3. Registro de Ventas
- Registrar ventas de caravanas a clientes
- Control de fechas e importes
- Asociación automática de cliente-caravana

### 4. Informes con Gráficos (Python)
- Ventas por mes (cantidad e ingresos)
- Distribución de caravanas por tipo
- Top 10 clientes activos
- Resumen general con estadísticas

## Tecnologías Utilizadas

### Backend
- **.NET 10.0** - Framework principal
- **Entity Framework Core** - ORM para base de datos
- **SQLite** - Base de datos
- **WPF** - Interfaz gráfica

### Análisis y Reportes
- **Python 3.13** - Generación de gráficos
- **Matplotlib** - Creación de gráficos
- **Pandas** - Análisis de datos
- **openpyxl** - Soporte para Excel (futuro)

## Instalación

### Requisitos Previos
- .NET SDK 10.0 o superior
- Python 3.8 o superior
- Visual Studio Code o Visual Studio 2022+

### Pasos de Instalación

1. **Clonar/Descargar el repositorio**
   ```bash
   cd AppCaravana
   ```

2. **Instalar dependencias de Python**
   ```bash
   python -m pip install matplotlib pandas openpyxl
   ```

3. **Restaurar paquetes NuGet y compilar**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Crear base de datos (primera ejecución)**
   ```bash
   dotnet ef database update
   ```

## Ejecución

### Desde la línea de comandos
```bash
dotnet run
```

### Modo Release (Recomendado)
```bash
dotnet run --configuration Release
```

### Ejecutar el archivo .exe compilado
```bash
.\bin\Release\net10.0-windows\AppCaravana.exe
```

## Uso de la Aplicación

### 1. Agregar Clientes
- Clickear en botón "Clientes"
- Presionar "Nuevo Cliente"
- Rellenar formulario con datos del cliente
- Clickear "Guardar"

### 2. Registrar Caravanas
- Clickear en botón "Caravanas"
- Presionar "Nueva Caravana"
- Completar información de la caravana
- Marcar como "Disponible" si corresponde
- Guardar

### 3. Registrar Ventas
- Clickear en botón "Ventas"
- Presionar "Nueva Venta"
- Seleccionar cliente y caravana
- Ingresar fecha e importe
- Guardar

### 4. Generar Reportes
- Clickear en botón "Informes"
- Presionar "Generar Reportes"
- Los gráficos se mostrarán automáticamente:
  - **Ventas por Mes**: Cantidad y total de ventas
  - **Caravanas por Tipo**: Distribución porcentual
  - **Clientes Activos**: Top 10 clientes
  - **Resumen General**: Estadísticas globales

## Estructura del Proyecto

```
AppCaravana/
├── Models/                 # Modelos de datos
│   ├── Cliente.cs
│   ├── Caravana.cs
│   ├── Venta.cs
│   ├── Stock.cs
│   └── AutorizacionSENASA.cs
├── Views/                  # Interfaces de usuario
│   ├── ClientesView.xaml
│   ├── Caravana.xaml
│   ├── VentasView.xaml
│   └── InformesView.xaml
├── ViewModels/             # Lógica de presentación
├── Services/               # Servicios (ReportService)
├── Data/                   # Contexto de base de datos
├── Migrations/             # Migraciones de EF Core
├── Reports/                # Scripts de Python
│   └── generate_reports.py
└── Convertidores/          # Convertidores WPF

caravanas.db               # Base de datos SQLite
```

## Notas Importantes

### Base de Datos
- Se crea automáticamente en la carpeta de ejecución como `caravanas.db`
- Estructura definida mediante migraciones de Entity Framework Core

### Python
- Los scripts están en la carpeta `Reports/`
- Los gráficos se generan en `Reports/reports_output/`
- Se ejecutan automáticamente desde C# mediante `ReportService`

### Interfaz
- Formularios con tamaño de letra aumentado (14-16 pt)
- TextBox con altura de 40 px para mejor usabilidad
- Botones grandes y accesibles

## Troubleshooting

### Error: "Python no está instalado"
```bash
# Verificar instalación
python --version

# Instalar paquetes requeridos
pip install matplotlib pandas openpyxl
```

### Error: "Base de datos no encontrada"
```bash
# Recrear base de datos
dotnet ef database update
```

### Error: "No hay datos para generar reportes"
- Asegúrese de tener registrados clientes, caravanas y ventas

## Futuras Mejoras
- Exportación de reportes a PDF y Excel
- Filtros avanzados en reportes
- Gráficos adicionales (márgenes de ganancia, rentabilidad)
- Autenticación de usuarios
- Copias de seguridad automáticas

## Licencia
Proyecto de desarrollo personal

## Autor
AppCaravana Team - 2025
