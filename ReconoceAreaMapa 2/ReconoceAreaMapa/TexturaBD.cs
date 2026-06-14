using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ReconoceAreaMapa
{
    class TexturaBD
    {
        private const string SERVIDOR = "(local)";
        private const string USUARIO = "user";
        private const string PASSWORD = "123456";
        private const string BASE_DATOS = "multimedia";
        private const string TABLA = "MuestrasTextura";

        private string connectionString;

        public TexturaBD()
        {
            connectionString =
                "server=" + SERVIDOR + ";" +
                "user=" + USUARIO + ";" +
                "pwd=" + PASSWORD + ";" +
                "database=" + BASE_DATOS + ";";
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(connectionString);
        }

        public void CrearTabla()
        {
            string sql = @"
                    IF NOT EXISTS (
                        SELECT *
                        FROM sysobjects
                        WHERE name='" + TABLA + @"'
                        AND xtype='U'
                    )
                    CREATE TABLE " + TABLA + @" (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        X INT,
                        Y INT,
                        R INT,
                        G INT,
                        B INT,
                        Zona VARCHAR(50)
                    )";

            using (SqlConnection con = ObtenerConexion())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
        }

        public bool InsertarTextura(int x,int y,int r, int g,int b,string zona)
        {
            try
            {
                string sql =
                    "INSERT INTO " + TABLA +
                    " (X,Y,R,G,B,Zona) " +
                    "VALUES (" +
                    x + "," +
                    y + "," +
                    r + "," +
                    g + "," +
                    b + "," +
                    "'" + zona + "')";

                using (SqlConnection con = ObtenerConexion())
                {
                    con.Open();

                    SqlCommand cmd =
                        new SqlCommand(sql, con);

                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataTable ObtenerTexturas()
        {
            DataTable dt = new DataTable();

            string sql = "SELECT X,Y,R,G,B,Zona " + 
                         "FROM " + TABLA;

            using (SqlConnection con = ObtenerConexion())
            {
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter(sql, con);

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable ObtenerPromediosPorZona()
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT " +
                "AVG(R) AS R, " +
                "AVG(G) AS G, " +
                "AVG(B) AS B, " +
                "Zona " +
                "FROM " + TABLA + " " +
                "GROUP BY Zona";

            using (SqlConnection con = ObtenerConexion())
            {
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter(sql, con);

                da.Fill(dt);
            }

            return dt;
        }

        public void Limpiar()
        {
            string sql = "DELETE FROM " + TABLA;

            using (SqlConnection con = ObtenerConexion())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(sql, con);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
