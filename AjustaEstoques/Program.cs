using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.DataAccess.Client;
using AjustaEstoques.Banco.SqlConn;
using System.Data;
using AjustaEstoques.Banco.Exceptions;
using System.IO;
using System.Runtime.InteropServices;

namespace AjustaEstoques
{
    class Program
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {

            DateTime d1 = DateTime.Now;
            bool flag1Ciclo = true;
            Console.WriteLine("Dê enter para iniciar o programa!");
            Console.ReadLine();
            IntPtr hWnd = GetConsoleWindow();
            ShowWindow(hWnd, 0);
            do
            {
                d1 = DateTime.Now;
                //if(d1.Hour == 16 && d1.Minute == 45)
                //{
                //    flag1Ciclo = false;
                //}
            } while (!(d1.Hour == 15 && d1.Minute == 35));
            Console.WriteLine("Deu certo!");
            ShowWindow(hWnd, 1);

            OracleConnection conn = DBUtils.GetDBConnection();
            OracleCommand cmd = new OracleCommand();
            Console.WriteLine("Get Connection: " + conn);
            try
            {
                conn.Open();
                Console.WriteLine("Conexão bem sucedida");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine("## ERROR: " + ex.Message);
                Console.Read();
                return;
            }



        }
    }
}
