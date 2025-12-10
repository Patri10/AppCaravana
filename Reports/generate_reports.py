import sqlite3
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
from datetime import datetime, timedelta
import os
import json
import sys
from pathlib import Path

# Configurar matplotlib para usar backend sin GUI
plt.switch_backend('Agg')

class ReportGenerator:
    def __init__(self, db_path):
        self.db_path = db_path
        self.reports_dir = Path(db_path).parent / "reports_output"
        self.reports_dir.mkdir(exist_ok=True)
        
    def get_connection(self):
        """Obtener conexion a la base de datos"""
        return sqlite3.connect(self.db_path)
    
    def generate_ventas_por_mes(self):
        """Generar grafico de ventas por mes"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        try:
            cursor.execute("""
                SELECT strftime('%Y-%m', Fecha) as mes, COUNT(*) as cantidad, SUM(Importe) as total
                FROM Ventas
                WHERE Fecha IS NOT NULL
                GROUP BY strftime('%Y-%m', Fecha)
                ORDER BY mes
            """)
            
            data = cursor.fetchall()
            
            if not data:
                print("No hay datos de ventas")
                return None
            
            meses = [row[0] for row in data]
            cantidades = [row[1] for row in data]
            totales = [row[2] if row[2] else 0 for row in data]
            
            fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(12, 10))
            
            # Grafico de cantidad de ventas
            ax1.bar(meses, cantidades, color='steelblue', alpha=0.7)
            ax1.set_title('Cantidad de Ventas por Mes', fontsize=16, fontweight='bold')
            ax1.set_xlabel('Mes', fontsize=12)
            ax1.set_ylabel('Cantidad de Ventas', fontsize=12)
            ax1.grid(axis='y', alpha=0.3)
            plt.setp(ax1.xaxis.get_majorticklabels(), rotation=45)
            
            # Grafico de total de ventas en dinero
            ax2.plot(meses, totales, marker='o', linewidth=2, markersize=8, color='darkgreen')
            ax2.fill_between(range(len(meses)), totales, alpha=0.3, color='lightgreen')
            ax2.set_title('Total de Ventas por Mes', fontsize=16, fontweight='bold')
            ax2.set_xlabel('Mes', fontsize=12)
            ax2.set_ylabel('Total ($)', fontsize=12)
            ax2.grid(True, alpha=0.3)
            plt.setp(ax2.xaxis.get_majorticklabels(), rotation=45)
            
            plt.tight_layout()
            output_path = self.reports_dir / "ventas_por_mes.png"
            plt.savefig(output_path, dpi=100, bbox_inches='tight')
            plt.close()
            
            return str(output_path)
        
        finally:
            conn.close()
    
    def generate_caravanas_por_tipo(self):
        """Generar grafico de caravanas por tipo"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        try:
            cursor.execute("""
                SELECT Tipo, COUNT(*) as cantidad
                FROM Caravanas
                GROUP BY Tipo
                ORDER BY cantidad DESC
            """)
            
            data = cursor.fetchall()
            
            if not data:
                print("No hay datos de caravanas")
                return None
            
            tipos = [row[0] if row[0] else "Sin especificar" for row in data]
            cantidades = [row[1] for row in data]
            
            fig, ax = plt.subplots(figsize=(12, 8))
            
            colors = plt.cm.Set3(range(len(tipos)))
            wedges, texts, autotexts = ax.pie(cantidades, labels=tipos, autopct='%1.1f%%',
                                              colors=colors, startangle=90, textprops={'fontsize': 11})
            
            for autotext in autotexts:
                autotext.set_color('black')
                autotext.set_fontweight('bold')
            
            ax.set_title('Distribucion de Caravanas por Tipo', fontsize=16, fontweight='bold')
            
            plt.tight_layout()
            output_path = self.reports_dir / "caravanas_por_tipo.png"
            plt.savefig(output_path, dpi=100, bbox_inches='tight')
            plt.close()
            
            return str(output_path)
        
        finally:
            conn.close()
    
    def generate_clientes_activos(self):
        """Generar grafico de clientes con mas ventas"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        try:
            cursor.execute("""
                SELECT c.Apellido || ', ' || c.Nombre as cliente, COUNT(v.Id) as ventas, SUM(v.Importe) as total
                FROM Clientes c
                LEFT JOIN Ventas v ON c.Id = v.ClienteId
                GROUP BY c.Id
                ORDER BY ventas DESC
                LIMIT 10
            """)
            
            data = cursor.fetchall()
            
            if not data:
                print("No hay datos de clientes")
                return None
            
            clientes = [row[0][:20] for row in data]
            ventas = [row[1] for row in data]
            totales = [row[2] if row[2] else 0 for row in data]
            
            fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(16, 8))
            
            # Grafico de cantidad de ventas por cliente
            ax1.barh(clientes, ventas, color='coral', alpha=0.7)
            ax1.set_title('Top 10 Clientes por Cantidad de Ventas', fontsize=14, fontweight='bold')
            ax1.set_xlabel('Cantidad de Ventas', fontsize=12)
            ax1.grid(axis='x', alpha=0.3)
            
            # Grafico de total de ventas por cliente
            ax2.barh(clientes, totales, color='skyblue', alpha=0.7)
            ax2.set_title('Top 10 Clientes por Monto Total', fontsize=14, fontweight='bold')
            ax2.set_xlabel('Total ($)', fontsize=12)
            ax2.grid(axis='x', alpha=0.3)
            
            plt.tight_layout()
            output_path = self.reports_dir / "clientes_activos.png"
            plt.savefig(output_path, dpi=100, bbox_inches='tight')
            plt.close()
            
            return str(output_path)
        
        finally:
            conn.close()
    
    def generate_resumen_general(self):
        """Generar grafico de resumen general"""
        conn = self.get_connection()
        cursor = conn.cursor()
        
        try:
            # Obtener estadisticas
            cursor.execute("SELECT COUNT(*) FROM Clientes")
            total_clientes = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Caravanas")
            total_caravanas = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Ventas")
            total_ventas = cursor.fetchone()[0]
            
            cursor.execute("SELECT SUM(Importe) FROM Ventas")
            total_ingresos = cursor.fetchone()[0] or 0
            
            cursor.execute("SELECT COUNT(*) FROM Caravanas WHERE Disponible = 1")
            caravanas_disponibles = cursor.fetchone()[0]
            
            # Crear visualizacion de resumen
            fig, ax = plt.subplots(figsize=(12, 8))
            ax.axis('off')
            
            # Titulo
            fig.suptitle('Resumen General de AppCaravana', fontsize=20, fontweight='bold', y=0.98)
            
            # Informacion en tabla
            info_data = [
                ['Metrica', 'Valor'],
                ['Total de Clientes', str(total_clientes)],
                ['Total de Caravanas', str(total_caravanas)],
                ['Caravanas Disponibles', str(caravanas_disponibles)],
                ['Total de Ventas', str(total_ventas)],
                ['Ingresos Totales', f'${total_ingresos:,.2f}'],
            ]
            
            table = ax.table(cellText=info_data, loc='center', cellLoc='center',
                           colWidths=[0.4, 0.4])
            
            table.auto_set_font_size(False)
            table.set_fontsize(14)
            table.scale(1, 3)
            
            # Formato del encabezado
            for i in range(2):
                table[(0, i)].set_facecolor('#4472C4')
                table[(0, i)].set_text_props(weight='bold', color='white')
            
            # Formato de filas alternadas
            for i in range(1, len(info_data)):
                color = '#D9E1F2' if i % 2 == 0 else 'white'
                for j in range(2):
                    table[(i, j)].set_facecolor(color)
            
            plt.tight_layout()
            output_path = self.reports_dir / "resumen_general.png"
            plt.savefig(output_path, dpi=100, bbox_inches='tight')
            plt.close()
            
            return str(output_path)
        
        finally:
            conn.close()
    
    def generate_all_reports(self):
        """Generar todos los reportes"""
        reports = {}
        
        print("Generando reportes...", file=sys.stderr)
        
        try:
            path = self.generate_ventas_por_mes()
            if path:
                reports['ventas_por_mes'] = path
                print(f"Reporte de ventas por mes generado: {path}", file=sys.stderr)
        except Exception as e:
            print(f"Error en reporte de ventas por mes: {e}", file=sys.stderr)
        
        try:
            path = self.generate_caravanas_por_tipo()
            if path:
                reports['caravanas_por_tipo'] = path
                print(f"Reporte de caravanas por tipo generado: {path}", file=sys.stderr)
        except Exception as e:
            print(f"Error en reporte de caravanas por tipo: {e}", file=sys.stderr)
        
        try:
            path = self.generate_clientes_activos()
            if path:
                reports['clientes_activos'] = path
                print(f"Reporte de clientes activos generado: {path}", file=sys.stderr)
        except Exception as e:
            print(f"Error en reporte de clientes activos: {e}", file=sys.stderr)
        
        try:
            path = self.generate_resumen_general()
            if path:
                reports['resumen_general'] = path
                print(f"Resumen general generado: {path}", file=sys.stderr)
        except Exception as e:
            print(f"Error en resumen general: {e}", file=sys.stderr)
        
        return reports


def main():
    """Funcion principal"""
    if len(sys.argv) < 2:
        print("Uso: python generate_reports.py <ruta_base_datos>", file=sys.stderr)
        sys.exit(1)
    
    db_path = sys.argv[1]
    
    if not os.path.exists(db_path):
        print(f"Error: Base de datos no encontrada en {db_path}", file=sys.stderr)
        sys.exit(1)
    
    generator = ReportGenerator(db_path)
    reports = generator.generate_all_reports()
    
    # Retornar JSON con rutas de los reportes
    print(json.dumps(reports, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()

