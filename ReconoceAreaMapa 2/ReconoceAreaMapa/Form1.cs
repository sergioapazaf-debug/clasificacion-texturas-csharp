using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.SqlClient;

namespace ReconoceAreaMapa
{
    public partial class Form1 : Form
    {
        Bitmap btm = null;
        List<string> areas = new List<string>();
        Color colorPixel;
        Color textura;
        Tuple<int, int> coordenada = new Tuple<int, int>(-1,-1);
        TexturaBD bd;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bd = new TexturaBD();
            bd.CrearTabla();

            areas.Add("Cesped");
            areas.Add("Tierra");
            areas.Add("Cemento");
            areas.Add("Asfalto");
            areas.Add("Agua");

            comboBox1.Items.AddRange(areas.ToArray());
            comboBox1.SelectedIndex = 0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                btm = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = btm;
            }
            else
                Debug.WriteLine("No se pudo cargar la imagen.");
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            coordenada = new Tuple<int, int>(e.X, e.Y);
            
            if (btm == null)
            {
                Debug.WriteLine("No hay imagen cargada");
                return;
            }

            //pixel
            colorPixel = btm.GetPixel(coordenada.Item1, coordenada.Item2);
            textBox1.Text = colorPixel.R.ToString();
            textBox2.Text = colorPixel.G.ToString();
            textBox3.Text = colorPixel.B.ToString();

            // Textura local 3x3
            textura = ObtenerColorPromedio(
                btm,
                coordenada.Item1,
                coordenada.Item2,
                3
            );

            textBox4.Text = textura.R.ToString();
            textBox5.Text = textura.G.ToString();
            textBox6.Text = textura.B.ToString();
        }

        private void btn_guardar_Click(object sender, EventArgs e)
        {
            if (btm == null)
            {
                Debug.WriteLine("No hay imagen cargada");
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                Debug.WriteLine("No hay zona seleccionada");
                return;
            }

            if (coordenada.Item1 < 0 || coordenada.Item2 < 0)
            {
                Debug.WriteLine("Seleccione una textura");
                return;
            }

            string zona = comboBox1.SelectedItem.ToString();

            bool guardado = bd.InsertarTextura(
                coordenada.Item1,
                coordenada.Item2,
                textura.R,
                textura.G,
                textura.B,
                zona
            );

            if (guardado)
            {
                lb_message.Text =
                    "Guardada muestra de: " + zona;
            }
            else
            {
                lb_message.Text =
                    "Error al guardar la muestra";
            }
        }

        private Color ObtenerColorPromedio(Bitmap btm, int xCentro, int yCentro, int tamano)
        {
            int mitad = tamano / 2;

            long sumaR = 0;
            long sumaG = 0;
            long sumaB = 0;

            int cantidad = 0;

            for (int y = yCentro - mitad; y <= yCentro + mitad; y++)
            {
                for (int x = xCentro - mitad; x <= xCentro + mitad; x++)
                {
                    if (x >= 0 && x < btm.Width && y >= 0 && y < btm.Height)
                    {
                        Color c = btm.GetPixel(x, y);

                        sumaR += c.R;
                        sumaG += c.G;
                        sumaB += c.B;

                        cantidad++;
                    }
                }
            }

            return Color.FromArgb(
                (int)(sumaR / cantidad),
                (int)(sumaG / cantidad),
                (int)(sumaB / cantidad)
            );
        }

        private void btn_reconocer_Click(object sender, EventArgs e)
        {
            if (btm == null)
            {
                lb_message.Text = "No hay imagen cargada";
                return;
            }

            if (coordenada.Item1 < 0 || coordenada.Item2 < 0)
            {
                lb_message.Text = "Seleccione una textura";
                return;
            }

            int umbral = 40;

            if (!int.TryParse(txtb_umbral.Text, out umbral) || umbral <= 0)
            {
                umbral = 40;
                txtb_umbral.Text = "40";
            }

            string zonaProbable = ClasificarTextura2(umbral);

            lb_message.Text = "Zona probable: " + zonaProbable;
        }

        private string ClasificarTextura(int umbral)
        {
            DataTable dt = bd.ObtenerPromediosPorZona();

            if (dt.Rows.Count == 0)
            {
                return "Sin muestras";
            }

            double mejorDistancia = double.MaxValue;
            string mejorZona = "Desconocido";

            foreach (DataRow fila in dt.Rows)
            {
                int r = Convert.ToInt32(fila["R"]);
                int g = Convert.ToInt32(fila["G"]);
                int b = Convert.ToInt32(fila["B"]);

                string zona = fila["Zona"].ToString();

                int dr = textura.R - r;
                int dg = textura.G - g;
                int db = textura.B - b;

                //distancia euclidiana
                double distancia =  Math.Sqrt(dr * dr + dg * dg + db * db);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejorZona = zona;
                }
            }

            if (mejorDistancia > umbral)
            {
                return "Desconocido";
            }

            return mejorZona;
        }

        private string ClasificarTextura2(int umbral)
        {
            DataTable dt = bd.ObtenerTexturas();

            double mejorDistancia = double.MaxValue;
            string mejorZona = "Desconocido";

            foreach (DataRow fila in dt.Rows)
            {
                int r = Convert.ToInt32(fila["R"]);
                int g = Convert.ToInt32(fila["G"]);
                int b = Convert.ToInt32(fila["B"]);

                string zona = fila["Zona"].ToString();

                int dr = textura.R - r;
                int dg = textura.G - g;
                int db = textura.B - b;

                double distancia = Math.Sqrt(dr * dr + dg * dg + db * db);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejorZona = zona;
                }
            }

            if (mejorDistancia > umbral)
            {
                return "Desconocido";
            }

            return mejorZona;
        }

        private void btnBorrarTbl_Click(object sender, EventArgs e)
        {
            DialogResult resultado =
                MessageBox.Show(
                    "¿Desea eliminar todas las muestras de textura?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (resultado == DialogResult.Yes)
            {
                bd.Limpiar();

                coordenada = new Tuple<int, int>(-1, -1);

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();

                textBox4.Clear();
                textBox5.Clear();
                textBox6.Clear();

                lb_message.Text = "Muestras eliminadas";
            }
        }
    }
}
