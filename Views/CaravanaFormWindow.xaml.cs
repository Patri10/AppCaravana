using System;
using System.Linq;
using System.Windows;
using AppCaravana.Data;
using AppCaravana.Models;

namespace AppCaravana.Views
{
    public partial class CaravanaFormWindow : Window
    {
        private int? _caravanaId;

        public CaravanaFormWindow(int? caravanaId = null)
        {
            InitializeComponent();
            _caravanaId = caravanaId;

            if (_caravanaId.HasValue)
                CargarCaravana();
        }

        private void CargarCaravana()
        {
            using (var db = new AppDbContext())
            {
                var caravana = db.Caravanas.FirstOrDefault(c => c.Id == _caravanaId.Value);
                if (caravana == null) return;

                txtSerie.Text = caravana.Serie;
                txtMarca.Text = caravana.Marca;
                txtModelo.Text = caravana.Modelo;
                txtAño.Text = caravana.Año.ToString();
                txtMatricula.Text = caravana.Matricula;
                txtSenasa.Text = caravana.NumeroSenasa;
                txtTipo.Text = caravana.Tipo;
                txtPrecio.Text = caravana.Precio.ToString();
                chkDisponible.IsChecked = caravana.Disponible;
                txtCaracteristicas.Text = caravana.Caracteristicas;
                txtDescripcion.Text = caravana.Descripcion;
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new AppDbContext())
            {
                Caravana caravana;

                if (_caravanaId.HasValue)
                    caravana = db.Caravanas.First(c => c.Id == _caravanaId.Value);
                else
                    caravana = new Caravana();

                // Asignación
                caravana.Serie = txtSerie.Text;
                caravana.Marca = txtMarca.Text;
                caravana.Modelo = txtModelo.Text;
                caravana.Año = int.Parse(txtAño.Text);
                caravana.Matricula = txtMatricula.Text;
                caravana.NumeroSenasa = txtSenasa.Text;
                caravana.Tipo = txtTipo.Text;
                caravana.Precio = decimal.Parse(txtPrecio.Text);
                caravana.Disponible = chkDisponible.IsChecked ?? false;
                caravana.Caracteristicas = txtCaracteristicas.Text;
                caravana.Descripcion = txtDescripcion.Text;

                // Guardar
                if (_caravanaId.HasValue == false)
                    db.Caravanas.Add(caravana);

                db.SaveChanges();
            }

            MessageBox.Show("Caravana guardada correctamente.");
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
